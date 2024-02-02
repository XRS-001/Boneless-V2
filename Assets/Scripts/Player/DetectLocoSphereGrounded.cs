using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectLocoSphereGrounded : MonoBehaviour
{
    public bool isGrounded;
    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
