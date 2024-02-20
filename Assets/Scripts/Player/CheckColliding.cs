using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckColliding : MonoBehaviour
{
    public bool collided;
    public Collider colliderColliding;
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.layer != LayerMask.NameToLayer("LeftHand") && collision.gameObject.layer != LayerMask.NameToLayer("RightHand"))
            collided = true;
            colliderColliding = collision.collider;
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("LeftHand") && collision.gameObject.layer != LayerMask.NameToLayer("RightHand"))
            collided = false;
            colliderColliding = null;
    }
}
