using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Runtime.CompilerServices;
using static EnumDeclaration;
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
public class GrabTwoAttach : BaseGrab
{
    public LeftAttach leftAttach;
    public RightAttach rightAttach;
    public bool twoHanded;
    [HideInInspector]
    private void FixedUpdate()
    {
        if (handGrabbing)
        {
            if (handGrabbing.connectedMass > 1)
            {
                if (!secondHandGrabbing)
                {
                    rb.AddForce(Vector3.down * (rb.mass * 250));
                }
                else
                {
                    //halve the force of gravity if two hands are grabbing
                    rb.AddForce(Vector3.down * (rb.mass * 125));
                }
            }
        }
    }
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
