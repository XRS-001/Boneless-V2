using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCollisionJoint : MonoBehaviour
{
    public bool isColliding;
    public bool hand;
    private GameObject colliderGroup;
    private void Start()
    {
        if(hand)
        {
            colliderGroup = GetComponent<GrabPhysics>().colliders[0].transform.parent.gameObject;
        }
    }
    private void Update()
    {
        if (colliderGroup)
        {
            if (!colliderGroup.activeInHierarchy)
            {
                isColliding = false;
            }
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        //Check if it's not colliding with hand/body layers
        if (collision.gameObject.layer != LayerMask.NameToLayer("LeftHand") && collision.gameObject.layer != LayerMask.NameToLayer("RightHand") && collision.gameObject.layer != LayerMask.NameToLayer("Body"))
        {
            isColliding = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("LeftHand") && collision.gameObject.layer != LayerMask.NameToLayer("RightHand") && collision.gameObject.layer != LayerMask.NameToLayer("Body"))
        {
            isColliding = false;
        }
    }
}
