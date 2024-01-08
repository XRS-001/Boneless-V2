using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Runtime.CompilerServices;
using static EnumDeclaration;

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
    [HideInInspector]
    public bool isHovering;
    public void SetAttachPoint(handTypeEnum handType)
    {
        if (handType == handTypeEnum.Left)
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
}
