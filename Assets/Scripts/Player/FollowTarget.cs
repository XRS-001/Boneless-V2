using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;
    public bool overrideTarget = false;
    // Update is called once per frame
    void FixedUpdate()
    {
        if(!overrideTarget)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }
    public void Overriding(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
