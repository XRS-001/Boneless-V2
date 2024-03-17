using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletImpact : MonoBehaviour
{
    public float damage;
    public GameObject[] bloodWounds;
    public AudioClip sound;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<DecalSurface>())
        {
            collision.collider.GetComponent<DecalSurface>().ImpactEffect(collision.contacts[0].point + Quaternion.LookRotation(collision.contacts[0].normal) * Vector3.forward / 20f, Quaternion.LookRotation(collision.contacts[0].normal), collision.transform);
        }
        else if (collision.collider.transform.root.GetComponent<NPC>())
        {
            collision.collider.transform.root.GetComponent<NPC>().DealDamage(collision.collider.tag, damage, false);
            GameObject bloodWound = Instantiate(bloodWounds[Random.Range(0, bloodWounds.Length - 1)], collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal), collision.transform);
            Destroy(bloodWound, 5f);
        }
        AudioSource.PlayClipAtPoint(sound, collision.contacts[0].point, 0.3f);
        Destroy(gameObject, 0.001f);
    }
}
