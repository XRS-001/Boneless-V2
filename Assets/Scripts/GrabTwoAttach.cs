using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using static GrabPhysics;

public class GrabTwoAttach : BaseGrab
{
    [Header("RightAttach")]
    public HandData rightPose;
    public Vector3 rightAttachPosition;
    public Vector3 rightAttachRotation;
    [Header("LeftAttach")]
    public HandData leftPose;
    public Vector3 leftAttachPosition;
    public Vector3 leftAttachRotation;
    public bool twoHanded;
    public void SetAttachPoint(handTypeEnum handType)
    {
        if(handType == handTypeEnum.Left)
        {
            attachPoint = leftAttachPosition;
            attachRotation = leftAttachRotation;
        }
        else
        {
            attachPoint = rightAttachPosition;
            attachRotation = rightAttachRotation;
        }
    }
    public void SetPose(handTypeEnum handType)
    {
        if (handType == handTypeEnum.Left)
        {
            pose = leftPose;
        }
        else
        {
            pose = rightPose;
        }
    }
#if UNITY_EDITOR

    [MenuItem("Tools/mirror selected right grab pose")]
    public static void MirrorRightPose()
    {
        GrabTwoAttach handPose = Selection.activeGameObject.GetComponent<GrabTwoAttach>();
        handPose.MirrorPose(handPose.leftPose, handPose.rightPose);
    }
#endif
    public void MirrorPose(HandData poseToMirror, HandData poseUsedToMirror)
    {
        Vector3 mirroredPosition = poseToMirror.root.localPosition;
        mirroredPosition.x *= -1;

        Quaternion mirroredQuaternion = poseUsedToMirror.root.localRotation;
        mirroredPosition.y *= -1;
        mirroredPosition.z *= -1;

        poseToMirror.root.localPosition = mirroredPosition;
        poseToMirror.root.rotation = mirroredQuaternion;

        for (int i = 0; i < poseUsedToMirror.fingerBones.Length; i++)
        {
            poseToMirror.fingerBones[i].localRotation = poseUsedToMirror.fingerBones[i].localRotation;
        }
    }
}
