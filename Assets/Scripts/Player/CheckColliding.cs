using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckColliding : MonoBehaviour
{
    public bool collided;
    public Collider colliderColliding;
    private void OnCollisionStay(Collision collision)
    {
        collided = true;
        colliderColliding = collision.collider;
    }
    private void OnCollisionExit(Collision collision)
    {
        collided = false;
        colliderColliding = null;
    }
}
