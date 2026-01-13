#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

public partial class AnimatorWizard : MonoBehaviour
{
    protected AacFlBase _aac;
    protected List<VRCExpressionParameters.Parameter> _vrcParams;

    protected AacFlLayer _fxTreeLayer;
    protected BlendTree _masterTree;

    private const bool UseWriteDefaults = true;
    protected const float TransitionSpeed = 0.05f;

    public AnimatorController assetContainer;
    public AvatarMask fxMask;

    public bool saveVRCExpressionParameters = false;
    public string SystemName = "AnimatorWizard";

    public string shapePreferenceSliderPrefix = "pref/slider/";
    public string shapePreferenceTogglesPrefix = "pref/toggle/";

    public void Create()
    {
        SkinnedMeshRenderer skin = GetComponentInChildren<SkinnedMeshRenderer>();
        VRCAvatarDescriptor avatar = GetComponentInChildren<VRCAvatarDescriptor>();

        if (skin == null || avatar == null)
            throw new Exception("SkinnedMeshRenderer or VRCAvatarDescriptor not found on avatar!");

        _vrcParams = new List<VRCExpressionParameters.Parameter>();

        InitializeAAC(avatar);

        // clear assetContainer
        //_aac.ClearPreviousAssets(); // Broken in new version
        ClearAssetContainer();
        DeleteAnimatorWizardLayers(avatar, SystemName);

        InitializeGestureLayers();
        InitializeFXLayer(skin);

        InitializeEyeTracking(skin, avatar);
        InitializeFaceTracking(skin, avatar);

        InitializeClothingCustomization(skin);
        InitializeColorCustomization();
        InitializeShapePreferences(skin);
        InitializeFaceToggle();

        if (saveVRCExpressionParameters)
        {
            avatar.expressionParameters.parameters = _vrcParams.ToArray();
            EditorUtility.SetDirty(avatar.expressionParameters);
        }

        RepackAnimatorControllers(avatar);
        SortAnimatorWizardLayers(avatar, SystemName);
    }

    private void InitializeAAC(VRCAvatarDescriptor avatar)
    {
        _aac = AacV1.Create(new AacConfiguration
        {
            SystemName = SystemName,
            AnimatorRoot = avatar.transform,
            DefaultValueRoot = avatar.transform,
            AssetContainer = assetContainer,
            ContainerMode = AacConfiguration.Container.Everything,
            AssetKey = SystemName,
            DefaultsProvider = new AacDefaultsProvider(UseWriteDefaults),
            //AssetContainerProvider = null
        }.WithAvatarDescriptor(avatar));
    }

    private void ClearAssetContainer()
    {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(assetContainer)))
        {
            if (asset is AnimationClip or BlendTree)
                AssetDatabase.RemoveObjectFromAsset(asset);
        }
    }

    public AacFlBase GetAAC() => _aac;
    public List<VRCExpressionParameters.Parameter> GetVRCParams() => _vrcParams;

    private void InitializeFXLayer(SkinnedMeshRenderer skin)
    {
        if (skin == null)
            throw new Exception("SkinnedMeshRenderer is null (InitializeFXLayer).");

        // FX layer
        var fxLayer = _aac.CreateMainFxLayer().WithAvatarMask(fxMask);

        var blendParam = fxLayer.FloatParameter("Blend");
        fxLayer.OverrideValue(blendParam, 1f);

        // master fx tree
        _fxTreeLayer = _aac.CreateSupportingFxLayer("tree").WithAvatarMask(fxMask);

        _masterTree = _aac.NewBlendTreeAsRaw();
        _masterTree.name = "master tree";
        _masterTree.blendType = BlendTreeType.Direct;
        _masterTree.blendParameter = blendParam.Name;

        _fxTreeLayer.NewState(_masterTree.name).WithAnimation(_masterTree);

        var ftActiveParam = fxLayer.BoolParameter(FullFaceTrackingPrefix + "LipTrackingActive");
        var faceToggleActiveParam = fxLayer.BoolParameter("FaceToggleActive");

        AacFlBoolParameter expTrackActiveParam;
        if (createFacialExpressionsControl)
            expTrackActiveParam = CreateBoolParam(fxLayer, FullFaceTrackingPrefix + expTrackName, true, true);
        else
            expTrackActiveParam = fxLayer.BoolParameter(FullFaceTrackingPrefix + expTrackName);

        var customGestureBlocksNames = new List<AacFlBoolParameter>();
        foreach (var name in GestureExpressionsBlockParamNames)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;
            customGestureBlocksNames.Add(fxLayer.BoolParameter(name));
        }

        InitializeGestureExpressions(skin, ftActiveParam, expTrackActiveParam, faceToggleActiveParam, customGestureBlocksNames);

    }
}

#endif