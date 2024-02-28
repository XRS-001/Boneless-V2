using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Limb : MonoBehaviour
{
    [Tooltip("The magnitude of the collision for hits to count")]
    public float hitThreshold;
    public float damageMultiplier;
    public GameObject decal;
    private NPC npc;
    private bool canHit = true;
    private void Start()
    {
        npc = transform.root.GetComponent<NPC>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("LeftHand") || collision.collider.gameObject.layer == LayerMask.NameToLayer("RightHand") || collision.collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            if (collision.relativeVelocity.magnitude > hitThreshold && canHit)
            {
                npc.DealDamage(transform.tag, collision.relativeVelocity.magnitude * damageMultiplier, true);

                DecalProjector decalProjector = Instantiate(decal, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal), transform).GetComponent<DecalProjector>();
                StartCoroutine(BloodOpacity(decalProjector));
            }
        }
    }
    IEnumerator BloodOpacity(DecalProjector decal)
    {
        float timer = 0;
        while (timer < 0.1f)
        {
            decal.fadeFactor = Mathf.Lerp(0, 1, timer / 0.1f);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
