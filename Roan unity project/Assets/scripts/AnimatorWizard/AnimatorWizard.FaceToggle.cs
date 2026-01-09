#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using UnityEngine;

public partial class AnimatorWizard : MonoBehaviour
{
    public Motion[] FaceToggleNames;

    private void InitializeFaceToggle()
    {
        if (!createFaceToggle)
            return;

        var FaceToggleLayer = _aac.CreateSupportingFxLayer("Face Toggle").WithAvatarMask(fxMask);

        var FaceToggleActive = FaceToggleLayer.BoolParameter("FaceToggleActive");
        var FaceToggleActiveParam = CreateIntParam(FaceToggleLayer, FullFaceTrackingPrefix + "animFacePresets", false, 0);

        var ftActiveParam = FaceToggleLayer.BoolParameter(FullFaceTrackingPrefix + "LipTrackingActive");

        var FaceToggleWaitingState = FaceToggleLayer.NewState("Waiting command")
            .Drives(FaceToggleActive, false);

        var waitingTransition = FaceToggleLayer.AnyTransitionsTo(FaceToggleWaitingState)
            .WithTransitionDurationSeconds(0.25f)
            .When(FaceToggleActiveParam.IsEqualTo(0));

        if (createFaceTracking)
            waitingTransition.Or().When(ftActiveParam.IsTrue());

        if (FaceToggleNames == null)
            throw new Exception("FaceToggleNames array is not assigned.");

        for (int i = 0; i < FaceToggleNames.Length; i++)
        {
            setupFaceToggle(FaceToggleLayer, FaceToggleNames[i], ftActiveParam, FaceToggleActiveParam, FaceToggleActive, i);
        }
    }

    private void setupFaceToggle(
        AacFlLayer FaceToggleLayer,
        Motion motion,
        AacFlBoolParameter ftActiveParam,
        AacFlIntParameter FaceToggleActiveParam,
        AacFlBoolParameter FaceToggleActive,
        int index)
    {
        if (motion == null)
            throw new Exception($"Face toggle motion at index {index} is missing!");

        var faceToggleState = FaceToggleLayer.NewState(motion.name)
            .Drives(FaceToggleActive, true)
            .WithAnimation(motion);

        FaceToggleLayer.AnyTransitionsTo(faceToggleState)
            .WithTransitionDurationSeconds(0.25f)
            .When(FaceToggleActiveParam.IsEqualTo(index + 1))
            .And(ftActiveParam.IsFalse());
    }
}

#endif