using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Limb : MonoBehaviour
{
    [Tooltip("The magnitude of the collision for hits to count")]
    public float hitThreshold;
    public float damageMultiplier;
    private NPC npc;
    private bool canHit = true;
    private void Start()
    {
        npc = transform.root.GetComponent<NPC>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("LeftHand") || collision.collider.gameObject.layer == LayerMask.NameToLayer("RightHand") || collision.collider.gameObject.layer == LayerMask.NameToLayer("Interactable") || collision.collider.gameObject.layer == LayerMask.NameToLayer("NonInteractive"))
        {
            if (collision.relativeVelocity.magnitude > hitThreshold && canHit)
            {
                npc.DealDamage(transform.tag, collision.relativeVelocity.magnitude * damageMultiplier, true);
            }
        }
    }
}
