using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCollisionFeet : MonoBehaviour
{
    public bool isColliding;
    public LayerMask groundLayer;

    private void Update()
    {
        isColliding = Physics.CheckSphere(transform.position, 0.1f, groundLayer);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
