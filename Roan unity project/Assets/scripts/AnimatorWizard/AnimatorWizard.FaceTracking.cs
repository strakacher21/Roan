#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

[Serializable]
public struct DualShape
{
    public string paramName;
    public string minShapeName;
    public string maxShapeName;
    public float minValue;
    public float neutralValue;
    public float maxValue;

    public DualShape(string paramName, string minShapeName, string maxShapeName, float minValue, float neutralValue, float maxValue)
    {
        this.paramName = paramName;
        this.minShapeName = minShapeName;
        this.maxShapeName = maxShapeName;
        this.minValue = minValue;
        this.neutralValue = neutralValue;
        this.maxValue = maxValue;
    }

    public DualShape(string paramName, string minShapeName, string maxShapeName)
    {
        this.paramName = paramName;
        this.minShapeName = minShapeName;
        this.maxShapeName = maxShapeName;
        minValue = -1;
        neutralValue = 0;
        maxValue = 1;
    }
}

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createFaceTracking = true;

    public bool MirrorFTparams = false;

    public bool createFTLipSyncControl = false;
    public string lipSyncName = "LipSyncTrackingActive";

    public string[] ftShapes = new[]
    {
        "JawOpen",
        "LipFunnel",
        "LipPucker",
        "MouthClosed",
        "MouthStretch",
        "MouthUpperUpLeft",
        "MouthLowerDownLeft",
        "MouthRaiserLower",
        "TongueOut",
        "EyeSquintLeft",
    };

    public DualShape[] ftDualShapes = new[]
    {
        new DualShape("SmileSad", "MouthSad", "MouthSmile"),
        new DualShape("JawX", "JawLeft", "JawRight"),
        new DualShape("JawZ", "JawBackward", "JawForward"),
        new DualShape("MouthX", "MouthLeft", "MouthRight"),
        new DualShape("EyeLidLeft", "EyeClosedLeft", "EyeWideLeft", 0, 0.75f, 1),
        new DualShape("BrowExpressionLeft", "BrowDown", "BrowUp"),
        new DualShape("CheekPuffSuck", "CheekSuck", "CheekPuff"),
    };

    private void InitializeFaceTracking(SkinnedMeshRenderer skin, VRCAvatarDescriptor avatar)
    {
        if (!createFaceTracking)
            return;

        var layer = _aac.CreateSupportingFxLayer("face animations toggle").WithAvatarMask(fxMask);

        var ftActiveParam = CreateBoolParam(layer, FullFaceTrackingPrefix + "LipTrackingActive", true, false);
        var ftBlendParam = layer.FloatParameter(FullFaceTrackingPrefix + "LipTrackingActive-float");

        // States with Lip Sync Control
        if (createFTLipSyncControl)
        {
            AacFlBoolParameter lipSyncActiveParam;
            if (createFTLipSyncControl)
                lipSyncActiveParam = CreateBoolParam(layer, FullFaceTrackingPrefix + lipSyncName, true, false);
            else
                lipSyncActiveParam = layer.BoolParameter(FullFaceTrackingPrefix + lipSyncName);

            var offFaceTrackingLipSyncTrackingAnimatesState = layer.NewState("face tracking off")
            .Drives(ftBlendParam, 0)
            .TrackingAnimates(AacAv3.Av3TrackingElement.Mouth);

            var onFaceTrackingLipSyncTrackingAnimatesState = layer.NewState("face tracking on")
                .Drives(ftBlendParam, 1)
                .TrackingAnimates(AacAv3.Av3TrackingElement.Mouth);

            var offFaceTrackingLipSyncTrackingTracksState = layer.NewState("face tracking off (LipSync Enabled)")
                .Drives(ftBlendParam, 0)
                .TrackingTracks(AacAv3.Av3TrackingElement.Mouth);

            var onFaceTrackingLipSyncTrackingTracksState = layer.NewState("face tracking on (LipSync Enabled)")
                .Drives(ftBlendParam, 1)
                .TrackingTracks(AacAv3.Av3TrackingElement.Mouth);

            var offFaceTrackingLipSyncTransition = layer.AnyTransitionsTo(offFaceTrackingLipSyncTrackingTracksState)
                .When(ftActiveParam.IsFalse())
                .And(lipSyncActiveParam.IsTrue());

            var onFaceTrackingLipSyncTransition = layer.AnyTransitionsTo(onFaceTrackingLipSyncTrackingTracksState)
                .WithTransitionToSelf()
                .When(ftActiveParam.IsTrue())
                .And(lipSyncActiveParam.IsTrue());

            layer.AnyTransitionsTo(offFaceTrackingLipSyncTrackingAnimatesState)
                .When(ftActiveParam.IsFalse())
                .And(lipSyncActiveParam.IsFalse());

            layer.AnyTransitionsTo(onFaceTrackingLipSyncTrackingAnimatesState)
                .WithTransitionToSelf()
                .When(ftActiveParam.IsTrue())
                .And(lipSyncActiveParam.IsFalse());
        }

        // States without Lip Sync Control
        else
        {
            var offFaceTrackingState = layer.NewState("face tracking off")
                .Drives(ftBlendParam, 0);

            var onFaceTrackingState = layer.NewState("face tracking on")
                .Drives(ftBlendParam, 1);

            layer.AnyTransitionsTo(offFaceTrackingState).When(ftActiveParam.IsFalse());
            layer.AnyTransitionsTo(onFaceTrackingState).When(ftActiveParam.IsTrue());
        }

        // Tree face tracking
        var tree = _masterTree.CreateBlendTreeChild(0);
        tree.name = "Face Tracking";
        tree.blendType = BlendTreeType.Direct;

        var allShapes = new List<string>();

        // adding blend shapes
        for (int i = 0; i < ftShapes.Length; i++)
        {
            string shapeName = ftShapes[i];

            if (MirrorFTparams)
            {
                for (int flip = 0; flip < EachSide(ref shapeName); flip++)
                {
                    var param = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + shapeName, false, 0);
                    tree.AddChild(BlendshapeTree(_fxTreeLayer, skin, param));

                    if (createOSCsmooth)
                        allShapes.Add(FullFaceTrackingPrefix + shapeName);
                }
            }
            else
            {
                var param = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + shapeName, false, 0);
                tree.AddChild(BlendshapeTree(_fxTreeLayer, skin, param));

                if (createOSCsmooth)
                    allShapes.Add(FullFaceTrackingPrefix + shapeName);
            }
        }

        // adding dual blend shapes
        for (int i = 0; i < ftDualShapes.Length; i++)
        {
            DualShape dualshape = ftDualShapes[i];
            string dualshapeName = dualshape.paramName;

            if (MirrorFTparams)
            {
                for (int flip = 0; flip < EachSide(ref dualshapeName); flip++)
                {
                    var param = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + dualshapeName, false, 0);
                    tree.AddChild(DualBlendshapeTree(
                        _fxTreeLayer,
                        param,
                        skin,
                        FullFaceTrackingPrefix + dualshape.minShapeName + GetSide(param.Name),
                        FullFaceTrackingPrefix + dualshape.maxShapeName + GetSide(param.Name),
                        dualshape.minValue,
                        dualshape.neutralValue,
                        dualshape.maxValue
                    ));

                    if (createOSCsmooth)
                        allShapes.Add(FullFaceTrackingPrefix + dualshapeName);
                }
            }
            else
            {
                var param = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + dualshape.paramName, false, 0);
                tree.AddChild(DualBlendshapeTree(
                    _fxTreeLayer,
                    param,
                    skin,
                    FullFaceTrackingPrefix + dualshape.minShapeName,
                    FullFaceTrackingPrefix + dualshape.maxShapeName,
                    dualshape.minValue,
                    dualshape.neutralValue,
                    dualshape.maxValue
                ));

                if (createOSCsmooth)
                    allShapes.Add(FullFaceTrackingPrefix + dualshape.paramName);
            }
        }

        var children = _masterTree.children;
        children[children.Length - 1].directBlendParameter = ftBlendParam.Name;
        _masterTree.children = children;

        // OSC Face Tracking smooth
        if (createOSCsmooth)
        {
            var oscLayer = _aac.CreateSupportingFxLayer("OSC smoothing").WithAvatarMask(fxMask);
            ApplyOSCSmoothing(oscLayer, localSmoothness, remoteSmoothness, allShapes, new List<BlendTree> { _masterTree });
        }
    }
}

#endif