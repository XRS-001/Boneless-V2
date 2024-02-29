using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLimb : MonoBehaviour
{
    [Tooltip("The magnitude of the collision for hits to count")]
    public float hitThreshold;
    public float damage;
    public GameManager gameManager;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
        {
            if (collision.relativeVelocity.magnitude > hitThreshold)
            {
                gameManager.DealDamage(damage);
            }
        }
    }
}
