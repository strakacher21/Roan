#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneSwitcher", menuName = "SceneSwitcher/Switcher Asset")]
public class SceneSwitcherAsset : ScriptableObject
{
    public SceneAsset[] scenes;
}
#endif