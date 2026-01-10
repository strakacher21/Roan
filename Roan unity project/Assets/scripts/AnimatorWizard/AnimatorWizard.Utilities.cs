#if UNITY_EDITOR

using AnimatorAsCode.V1;
using VRLabs.AV3Manager;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;

public partial class AnimatorWizard : MonoBehaviour
{
    protected const string Left = "Left";
    protected const string Right = "Right";

    protected BlendTree BlendshapeTree(AacFlLayer layer, SkinnedMeshRenderer skin, AacFlParameter param, float min = 0, float max = 100)
    {
        return BlendshapeTree(layer, skin, param.Name, param, min, max);
    }

    protected BlendTree BlendshapeTree(AacFlLayer layer, SkinnedMeshRenderer skin, string shapeName, AacFlParameter param, float min = 0, float max = 100)
    {
        var state000 = _aac.NewClip().BlendShape(skin, shapeName, min);
        state000.Clip.name = param.Name + ":0";

        var state100 = _aac.NewClip().BlendShape(skin, shapeName, max);
        state100.Clip.name = param.Name + ":1";

        return Subtree(new Motion[] { state000.Clip, state100.Clip }, new[] { 0f, 1f }, param);
    }

    protected BlendTree DualBlendshapeTree(
        AacFlLayer layer, AacFlParameter param, SkinnedMeshRenderer skin,
        string minShapeName, string maxShapeName,
        float minValue, float neutralValue, float maxValue)
    {
        var minClip = _aac.NewClip()
            .BlendShape(skin, minShapeName, 100)
            .BlendShape(skin, maxShapeName, 0);
        minClip.Clip.name = param.Name + ":" + minShapeName;

        var neutralClip = _aac.NewClip()
            .BlendShape(skin, minShapeName, 0)
            .BlendShape(skin, maxShapeName, 0);
        neutralClip.Clip.name = param.Name + ":neutral";

        var maxClip = _aac.NewClip()
            .BlendShape(skin, minShapeName, 0)
            .BlendShape(skin, maxShapeName, 100);
        maxClip.Clip.name = param.Name + ":" + maxShapeName;

        return Subtree(new Motion[] { minClip.Clip, neutralClip.Clip, maxClip.Clip },
            new[] { minValue, neutralValue, maxValue }, param);
    }

    protected BlendTree Subtree(Motion[] motions, float[] thresholds, AacFlParameter param)
    {
        var tree = Create1DTree(param.Name, 0, 1);
        ChildMotion[] children = new ChildMotion[motions.Length];

        for (int i = 0; i < motions.Length; i++)
        {
            children[i] = new ChildMotion { motion = motions[i], threshold = thresholds[i], timeScale = 1 };
        }

        tree.children = children;
        return tree;
    }

    protected BlendTree Create1DTree(string paramName, float min, float max)
    {
        var tree = _aac.NewBlendTreeAsRaw();
        tree.useAutomaticThresholds = false;
        tree.name = paramName;
        tree.blendParameter = paramName;
        tree.minThreshold = min;
        tree.maxThreshold = max;
        tree.blendType = BlendTreeType.Simple1D;
        return tree;
    }

    protected static int EachSide(ref string str)
    {
        if (str.EndsWith(Right))
        {
            str = str.Replace(Right, Left);
        }
        else if (str.EndsWith(Left))
        {
            str = str.Replace(Left, Right);
        }
        else
        {
            return 1;
        }

        return 2;
    }

    protected static string GetSide(string str)
    {
        if (str.EndsWith(Right))
            return Right;

        if (str.EndsWith(Left))
            return Left;

        return "";
    }

    protected AacFlIntParameter CreateIntParam(AacFlLayer layer, string paramName, bool save, int val)
    {
        _vrcParams.Add(new VRCExpressionParameters.Parameter()
        {
            name = paramName,
            valueType = VRCExpressionParameters.ValueType.Int,
            saved = save,
            networkSynced = true,
            defaultValue = val,
        });

        return layer.IntParameter(paramName);
    }

    protected AacFlFloatParameter CreateFloatParam(AacFlLayer layer, string paramName, bool save, float val)
    {
        _vrcParams.Add(new VRCExpressionParameters.Parameter()
        {
            name = paramName,
            valueType = VRCExpressionParameters.ValueType.Float,
            saved = save,
            networkSynced = true,
            defaultValue = val,
        });

        return layer.FloatParameter(paramName);
    }

    protected AacFlBoolParameter CreateBoolParam(AacFlLayer layer, string paramName, bool save, bool val)
    {
        _vrcParams.Add(new VRCExpressionParameters.Parameter()
        {
            name = paramName,
            valueType = VRCExpressionParameters.ValueType.Bool,
            saved = save,
            networkSynced = true,
            defaultValue = val ? 1 : 0,
        });

        return layer.BoolParameter(paramName);
    }

    protected void RepackAnimatorControllers(VRCAvatarDescriptor avatar)
    {
        if (avatar == null)
            return;

        var processedPaths = new HashSet<string>();

        UnityEditor.AssetDatabase.StartAssetEditing();
        try
        {
            for (int pass = 0; pass < 2; pass++)
            {
                var isBase = pass == 0;
                var layers = isBase ? avatar.baseAnimationLayers : avatar.specialAnimationLayers;
                var changed = false;

                for (int i = 0; i < layers.Length; i++)
                {
                    var layer = layers[i];
                    if (layer.isDefault)
                        continue;

                    if (layer.type != VRCAvatarDescriptor.AnimLayerType.FX &&
                        layer.type != VRCAvatarDescriptor.AnimLayerType.Gesture &&
                        layer.type != VRCAvatarDescriptor.AnimLayerType.Additive)
                        continue;

                    var sourceController = layer.animatorController as AnimatorController;
                    if (sourceController == null)
                        continue;

                    var originalPath = UnityEditor.AssetDatabase.GetAssetPath(sourceController);
                    if (string.IsNullOrEmpty(originalPath))
                        continue;

                    if (processedPaths.Contains(originalPath))
                    {
                        var reused = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimatorController>(originalPath);
                        if (reused != null)
                        {
                            layer.animatorController = reused;
                            layers[i] = layer;
                            changed = true;
                        }
                        continue;
                    }

                    var expectedOriginalName = System.IO.Path.GetFileNameWithoutExtension(originalPath);
                    var originalFileName = System.IO.Path.GetFileName(originalPath);

                    var tempFolderName = "AnimatorWizard_Temp_" + System.Guid.NewGuid().ToString("N");
                    UnityEditor.AssetDatabase.CreateFolder("Assets", tempFolderName);
                    var tempFolderPath = "Assets/" + tempFolderName;

                    var tempPath = tempFolderPath + "/" + originalFileName;

                    var newController = new AnimatorController { name = expectedOriginalName };
                    UnityEditor.AssetDatabase.CreateAsset(newController, tempPath);
                    UnityEditor.AssetDatabase.SaveAssets();

                    AnimatorCloner.MergeControllers(newController, sourceController, null, false);
                    UnityEditor.AssetDatabase.SaveAssets();

                    var absTempPath = System.IO.Path.GetFullPath(tempPath);
                    var absOriginalPath = System.IO.Path.GetFullPath(originalPath);
                    System.IO.File.Copy(absTempPath, absOriginalPath, true);

                    UnityEditor.AssetDatabase.ImportAsset(originalPath, UnityEditor.ImportAssetOptions.ForceUpdate);

                    var finalController = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimatorController>(originalPath);
                    if (finalController != null)
                    {
                        finalController.name = expectedOriginalName;
                        UnityEditor.EditorUtility.SetDirty(finalController);

                        layer.animatorController = finalController;
                        layers[i] = layer;
                        changed = true;
                    }

                    UnityEditor.AssetDatabase.DeleteAsset(tempFolderPath);
                    UnityEditor.AssetDatabase.SaveAssets();

                    processedPaths.Add(originalPath);
                }

                if (changed)
                {
                    if (isBase)
                        avatar.baseAnimationLayers = layers;
                    else
                        avatar.specialAnimationLayers = layers;

                    UnityEditor.EditorUtility.SetDirty(avatar);
                }
            }
        }
        finally
        {
            UnityEditor.AssetDatabase.StopAssetEditing();
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
    }

}

#endif