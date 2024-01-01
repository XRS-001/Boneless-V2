using System;
using UnityEngine;

public class BaseGrab : MonoBehaviour
{
    public bool displayAttachGizmos;
    public readonly Color color = new Color(1, 0.75f, 0, 0.5f);

    public Vector3 attachPoint { get; protected set; }
    public Vector3 attachRotation { get; protected set; }
    public Collider[] colliders;
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
