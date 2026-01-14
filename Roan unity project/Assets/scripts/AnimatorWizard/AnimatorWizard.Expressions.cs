#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createFaceToggle = true;

    public string[] GestureExpressionsBlockParamNames =
    {
        "contact/confuse"
    };

    public bool createFacialExpressionsControl = false;
    public string expTrackName = "ExpressionTrackingActive";

    public string mouthPrefix = "exp/mouth/";
    public string[] mouthShapeNames =
    {
        "basis",
        "frown",
        "smile",
        "grimace",
        "smile",
        "grimace",
        "grimace",
        "frown",
    };

    public string browPrefix = "exp/brows/";
    public string[] browShapeNames =
    {
        "basis",
        "down",
        "up",
        "curious",
        "up",
        "worried",
        "curious",
        "down",
    };

    protected void InitializeGestureExpressions(
        SkinnedMeshRenderer skin,
        AacFlBoolParameter ftActiveParam,
        AacFlBoolParameter ExpTrackActiveParam,
        AacFlBoolParameter FaceToggleActive,
        List<AacFlBoolParameter> customGestureBlocksNames
        )
    {
        // brow Gesture expressions
        MapHandPosesToShapes("brow expressions", skin, browShapeNames, browPrefix, false, ftActiveParam, ExpTrackActiveParam, FaceToggleActive, customGestureBlocksNames);

        // mouth Gesture expressions
        MapHandPosesToShapes("mouth expressions", skin, mouthShapeNames, mouthPrefix, true, ftActiveParam, ExpTrackActiveParam, FaceToggleActive, customGestureBlocksNames);
    }

    private void MapHandPosesToShapes(
        string layerName,
        SkinnedMeshRenderer skin,
        string[] shapeNames,
        string prefix,
        bool rightHand,
        AacFlBoolParameter ftActiveParam,
        AacFlBoolParameter ExpTrackActiveParam,
        AacFlBoolParameter FaceToggleActive,
        List<AacFlBoolParameter> customGestureBlocksNames
        )
    {
        var layer = _aac.CreateSupportingFxLayer(layerName).WithAvatarMask(fxMask);
        var Gesture = layer.IntParameter("Gesture" + (rightHand ? Right : Left));

        List<string> allExpressions = new List<string>();
        foreach (var shapeName in shapeNames)
        {
            if (!allExpressions.Contains(shapeName))
                allExpressions.Add(shapeName);
        }

        if (shapeNames.Length != 8)
            throw new Exception("Number of face poses must equal number of hand gestures (8)!");

        for (int i = 0; i < shapeNames.Length; i++)
        {
            var clip = _aac.NewClip();

            foreach (var shapeName in shapeNames)
            {
                clip.BlendShape(skin, prefix + shapeName, shapeName == shapeNames[i] ? 100 : 0);
            }

            var state = layer.NewState(shapeNames[i], 1, i).WithAnimation(clip);

            var enter = layer.EntryTransitionsTo(state).When(Gesture.IsEqualTo(i));
            var exit = state.Exits()
                .WithTransitionDurationSeconds(TransitionSpeed)
                .When(Gesture.IsNotEqualTo(i));

            if (createFaceTracking)
            {
                if (i == 0)
                {
                    enter.Or().When(ftActiveParam.IsTrue());
                    exit.And(ftActiveParam.IsFalse());
                }
                else
                {
                    enter.And(ftActiveParam.IsFalse());
                    exit.Or().When(ftActiveParam.IsTrue());
                }
            }

            if (createFacialExpressionsControl)
            {
                if (i == 0)
                {
                    enter.Or().When(ExpTrackActiveParam.IsFalse());
                    exit.And(ExpTrackActiveParam.IsTrue());
                }
                else
                {
                    enter.And(ExpTrackActiveParam.IsTrue());
                    exit.Or().When(ExpTrackActiveParam.IsFalse());
                }
            }

            if (createFaceToggle)
            {
                if (i == 0)
                {
                    enter.Or().When(FaceToggleActive.IsTrue());
                    exit.And(FaceToggleActive.IsFalse());
                }
                else
                {
                    enter.And(FaceToggleActive.IsFalse());
                    exit.Or().When(FaceToggleActive.IsTrue());
                }
            }

            if (customGestureBlocksNames != null && customGestureBlocksNames.Count > 0)
            {
                foreach (var block in customGestureBlocksNames)
                {
                    if (block == null) continue;

                    if (i == 0) { enter.Or().When(block.IsTrue()); exit.And(block.IsFalse()); }
                    else { enter.And(block.IsFalse()); exit.Or().When(block.IsTrue()); }
                }
            }
        }
    }
}

#endif