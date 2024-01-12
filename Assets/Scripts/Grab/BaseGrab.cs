using System;
using UnityEngine;

public class BaseGrab : MonoBehaviour
{
    public Vector3 attachPoint { get; protected set; }
    public Vector3 attachRotation { get; protected set; }
    public Collider[] colliders;
    [HideInInspector]
    public bool isGrabbing;
    [HideInInspector]
    public bool isTwoHandGrabbing;
    public GrabPhysics handGrabbing;
    public GrabPhysics secondHandGrabbing;
    [NonSerialized]
    public HandData pose;
    [HideInInspector]
    public Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
}
