using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using static GrabPhysics;

public class GrabTwoAttach : BaseGrab
{
    [System.Serializable]
    public class LeftAttach
    {
        public HandData leftPose;
        public Vector3 leftAttachPosition;
        public Vector3 leftAttachRotation;
    }
    [System.Serializable]
    public class RightAttach
    {
        public HandData rightPose;
        public Vector3 rightAttachPosition;
        public Vector3 rightAttachRotation;
    }
    public LeftAttach leftAttach;
    public RightAttach rightAttach;
    public bool twoHanded;
    private void OnDrawGizmosSelected()
    {
        if (displayAttachGizmos)
        {
            //draw visual aids for the attach points
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.TransformPoint(leftAttach.leftAttachPosition), 0.025f);
            Gizmos.DrawSphere(transform.TransformPoint(rightAttach.rightAttachPosition), 0.025f);
        }
    }
    public void SetAttachPoint(handTypeEnum handType)
    {
        if(handType == handTypeEnum.Left)
        {
            attachPoint = leftAttach.leftAttachPosition;
            attachRotation = leftAttach.leftAttachRotation;
        }
        else
        {
            attachPoint = rightAttach.rightAttachPosition;
            attachRotation = rightAttach.rightAttachRotation;
        }
    }
    public void SetPose(handTypeEnum handType)
    {
        if (handType == handTypeEnum.Left)
        {
            pose = leftAttach.leftPose;
        }
        else
        {
            pose = rightAttach.rightPose;
        }
    }
#if UNITY_EDITOR

    [MenuItem("Tools/mirror selected right grab pose")]
    public static void MirrorRightPose()
    {
        GrabTwoAttach handPose = Selection.activeGameObject.GetComponent<GrabTwoAttach>();
        handPose.MirrorPose(handPose.leftAttach.leftPose, handPose.rightAttach.rightPose);
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
