using System;
using System.Collections;
using UnityEngine;

public class BaseGrab : MonoBehaviour
{
    public Vector3 attachPoint { get; protected set; }
    public Vector3 attachRotation { get; protected set; }
    public Collider[] colliders;
    [HideInInspector]
    public bool isGrabbing;
    public bool isHovering;
    [HideInInspector]
    public bool isTwoHandGrabbing;
    public GrabPhysics handGrabbing;
    public GrabPhysics secondHandGrabbing;
    [NonSerialized]
    public HandData pose;
    [Tooltip("Only applicable to non dynamic grabs")]
    public bool indexFingerFreedom = true;
    public bool despawn;
    public float despawnTime;

    public IEnumerator Despawn()
    {
        if (despawn)
        {
            float countdown = despawnTime;
            while (countdown > 0)
            {
                countdown -= Time.deltaTime;
                if (isGrabbing)
                {
                    countdown = despawnTime;
                }
                yield return null;
            }
            Destroy(gameObject);
        }
        else
            yield return null;
    }
}
