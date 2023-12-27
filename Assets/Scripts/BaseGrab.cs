using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGrab : MonoBehaviour
{
    public Vector3 attachPoint { get; protected set; }
    public Vector3 attachRotation { get; protected set; }
    public Collider[] colliders;
    public bool isGrabbing;
    public bool isTwoHandGrabbing;
    public GrabPhysics handGrabbing;
    public GrabPhysics secondHandGrabbing;
    [NonSerialized]
    public HandData pose;
}
