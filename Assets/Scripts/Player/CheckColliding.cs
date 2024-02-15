using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckColliding : MonoBehaviour
{
    public bool collided;
    private void OnCollisionStay(Collision collision)
    {
        collided = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        collided = false;
    }
}
