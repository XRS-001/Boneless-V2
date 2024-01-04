using System;
using UnityEngine;

public class BaseGrab : MonoBehaviour
{
    public Vector3 attachPoint { get; protected set; }
    public Vector3 attachRotation { get; protected set; }
    public Collider[] colliders;
    protected Rigidbody rb;
    [HideInInspector]
    public bool isGrabbing;
    [HideInInspector]
    public bool isTwoHandGrabbing;
    [HideInInspector]
    public GrabPhysics handGrabbing;
    [HideInInspector]
    public GrabPhysics secondHandGrabbing;
    [NonSerialized]
    public HandData pose;
}
