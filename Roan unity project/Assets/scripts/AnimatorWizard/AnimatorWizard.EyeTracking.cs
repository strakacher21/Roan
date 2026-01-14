#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createEyeTracking = true;

    public bool UseSameEyeAnimationsForBothEyes = true;

    public float maxEyeMotionValue = 0.7f;

    public AvatarMask EyeLeftMask;
    public AvatarMask EyeRightMask;

    public Motion[] LeftEyePoses;
    public Motion[] RightEyePoses;

    private void InitializeEyeTracking(SkinnedMeshRenderer skin, VRCAvatarDescriptor avatar)
    {
        if (!createEyeTracking)
            return;

        var AdditiveLayer = _aac.CreateMainIdleLayer();

        AacFlBoolParameter etActiveParam = CreateBoolParam(AdditiveLayer, FullFaceTrackingPrefix + "EyeTrackingActive", true, false);

        // WORKAROUND OSCsmooth does not work if the Blend param is not updated. need to fix this!
        AacFlFloatParameter WORKAROUNDBlendParam = AdditiveLayer.FloatParameter("OSCsmooth/Blend");

        AacFlFloatParameter EyeXParam = CreateFloatParam(AdditiveLayer, FullFaceTrackingPrefix + "EyeX", false, 0.0f);
        AacFlFloatParameter EyeYParam = CreateFloatParam(AdditiveLayer, FullFaceTrackingPrefix + "EyeY", false, 0.0f);

        Motion[] leftPoses = LeftEyePoses;
        Motion[] rightPoses = UseSameEyeAnimationsForBothEyes ? LeftEyePoses : RightEyePoses;

        BlendTree leftEyeTree = setupEyeTracking(EyeXParam, EyeYParam, etActiveParam, Left, maxEyeMotionValue, leftPoses, EyeLeftMask);
        BlendTree rightEyeTree = setupEyeTracking(EyeXParam, EyeYParam, etActiveParam, Right, maxEyeMotionValue, rightPoses, EyeRightMask);

        // OSC Eye Tracking smooth
        if (createOSCsmooth)
        {
            var allEyeParams = new List<string> { EyeXParam.Name, EyeYParam.Name };
            var OSCLayerEye = _aac.CreateSupportingIdleLayer("OSC smoothing");
            ApplyOSCSmoothing(OSCLayerEye, localSmoothness, remoteSmoothness, allEyeParams, new List<BlendTree> { leftEyeTree, rightEyeTree });
        }
    }

    private BlendTree setupEyeTracking(
        AacFlFloatParameter EyeXParam,
        AacFlFloatParameter EyeYParam,
        AacFlBoolParameter etActiveParam,
        string side,
        float maxMotionValue,
        Motion[] poses,
        AvatarMask mask)
    {
        var layer = _aac.CreateSupportingIdleLayer($"Eye {side} Tracking").WithAvatarMask(mask);

        if (poses == null || poses.Length != 9)
            throw new Exception($"The {side} eye poses array must contain exactly 9 motions!");

        var VRCEyeControlState = layer.NewState($"VRC Eye {side} Control")
            .TrackingTracks(AacAv3.Av3TrackingElement.Eyes);

        var EyeTrackingTree = _aac.NewBlendTreeAsRaw();
        EyeTrackingTree.name = $"Eye {side} Tracking";
        EyeTrackingTree.blendType = BlendTreeType.FreeformCartesian2D;
        EyeTrackingTree.blendParameter = EyeXParam.Name;
        EyeTrackingTree.blendParameterY = EyeYParam.Name;

        // add motions to the blend tree
        var positions = new[]
        {
            new Vector2(-maxMotionValue,  maxMotionValue), // LeftUp
            new Vector2(0,               maxMotionValue), // Up
            new Vector2( maxMotionValue, maxMotionValue), // RightUp
            new Vector2(-maxMotionValue, 0),              // Left
            Vector2.zero,                                 // Neutral
            new Vector2( maxMotionValue, 0),              // Right
            new Vector2(-maxMotionValue, -maxMotionValue),// LeftDown
            new Vector2(0,               -maxMotionValue),// Down
            new Vector2( maxMotionValue, -maxMotionValue) // RightDown
        };

        for (int i = 0; i < poses.Length; i++)
        {
            if (poses[i] == null)
                throw new Exception($"Eye tracking animation at index {i} is missing!");

            var child = new ChildMotion
            {
                motion = poses[i],
                position = positions[i],
                timeScale = 1f
            };

            var newChildren = new ChildMotion[EyeTrackingTree.children.Length + 1];
            Array.Copy(EyeTrackingTree.children, newChildren, EyeTrackingTree.children.Length);
            newChildren[EyeTrackingTree.children.Length] = child;
            EyeTrackingTree.children = newChildren;
        }

        var EyeTrackingState = layer.NewState($"Eye {side} Tracking")
            .WithAnimation(EyeTrackingTree)
            .TrackingAnimates(AacAv3.Av3TrackingElement.Eyes);

        layer.AnyTransitionsTo(VRCEyeControlState).When(etActiveParam.IsFalse());
        layer.AnyTransitionsTo(EyeTrackingState).WithTransitionToSelf().When(etActiveParam.IsTrue());

        return EyeTrackingTree;
    }

    partial void ApplyOSCSmoothing(
        AacFlLayer layer,
        float localSmoothness,
        float remoteSmoothness,
        List<string> parameters,
        List<BlendTree> trees);
}

#endif