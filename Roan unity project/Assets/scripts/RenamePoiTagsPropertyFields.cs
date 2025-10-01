using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RenamePoiTagsPropertyFields : MonoBehaviour
{
    public RuntimeAnimatorController controller;
    public string KeyWord = "material";
    public string tag = "28PC29";
}

#if UNITY_EDITOR
[CustomEditor(typeof(RenamePoiTagsPropertyFields))]
class RenamePoiTagsPropertyFieldsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var comp = (RenamePoiTagsPropertyFields)target;
        GUILayout.Space(6);
        if (GUILayout.Button("Apply"))
        {
            if (comp.controller == null)
            {
                EditorUtility.DisplayDialog("Error", "Controller is not assigned.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Confirm", "AnimationClip property names will be modified. Continue?", "Yes", "Cancel"))
                return;
            ApplyChanges(comp);
        }
    }

    static AnimationClip[] GetClips(RenamePoiTagsPropertyFields comp)
    {
        return comp.controller != null ? comp.controller.animationClips ?? new AnimationClip[0] : new AnimationClip[0];
    }

    static bool TryModify(string input, string keyWord, string tag, out string output)
    {
        output = input;
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(keyWord))
            return false;
        int idx = input.LastIndexOf(keyWord, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return false;
        int endIdx = idx + keyWord.Length;
        string after = input.Length > endIdx ? input.Substring(endIdx) : "";
        if (string.IsNullOrEmpty(tag))
        {
            if (string.IsNullOrEmpty(after)) return false;
            output = input.Substring(0, endIdx);
            return true;
        }
        else
        {
            if (after == tag) return false;
            output = input.Substring(0, endIdx) + tag;
            return true;
        }
    }

    static void ApplyChanges(RenamePoiTagsPropertyFields comp)
    {
        var clips = GetClips(comp);
        if (clips == null || clips.Length == 0)
        {
            EditorUtility.DisplayDialog("Info", "No AnimClips found in the controller.", "OK");
            return;
        }

        int changedBindings = 0;
        int changedClips = 0;

        foreach (var clip in clips)
        {
            bool clipChanged = false;
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var toAddCurves = new List<(EditorCurveBinding, AnimationCurve)>();
            var toRemoveBindings = new List<EditorCurveBinding>();

            foreach (var b in bindings)
            {
                string newPath = b.path;
                string newProp = b.propertyName;
                bool p1 = TryModify(b.path, comp.KeyWord, comp.tag, out newPath);
                bool p2 = TryModify(b.propertyName, comp.KeyWord, comp.tag, out newProp);
                if (!p1 && !p2) continue;
                var curve = AnimationUtility.GetEditorCurve(clip, b);
                var nb = b;
                nb.path = newPath;
                nb.propertyName = newProp;
                toAddCurves.Add((nb, curve));
                toRemoveBindings.Add(b);
                clipChanged = true;
                changedBindings++;
            }

            if (clipChanged)
            {
                Undo.RegisterCompleteObjectUndo(clip, "Rename property fields");
                foreach (var b in toRemoveBindings) AnimationUtility.SetEditorCurve(clip, b, null);
                foreach (var pair in toAddCurves) AnimationUtility.SetEditorCurve(clip, pair.Item1, pair.Item2);
                EditorUtility.SetDirty(clip);
                changedClips++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Done", $"Modified bindings: {changedBindings}. Modified clips: {changedClips}.", "OK");
        Debug.Log($"[RenamePoiTagsPropertyFields] Modified bindings: {changedBindings}, clips: {changedClips}");
    }
}
#endif
