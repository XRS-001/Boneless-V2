using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollisionImpact : MonoBehaviour
{
    public AudioClip impactSound;
    public AudioSource audioSource { get; private set; }
    private bool canCollide = true;
    // Start is called before the first frame update
    void Start()
    {
        if (!GetComponent<AudioSource>())
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(canCollide)
        {
            if (!collision.transform.root.GetComponent<Surface>())
            {
                //make the sound louder the harder the collision was ( / 10 to make it smaller)
                audioSource.PlayOneShot(impactSound, Mathf.Clamp(collision.relativeVelocity.magnitude / 20, 0, 0.05f));
                StartCoroutine(WaitToCollide());
            }
            else
            {
                audioSource.PlayOneShot(collision.transform.root.GetComponent<Surface>().surfaceImpactClip, Mathf.Clamp(collision.relativeVelocity.magnitude / 20, 0, 0.05f));
                StartCoroutine(WaitToCollide());
            }
        }
    }
    IEnumerator WaitToCollide()
    {
        canCollide = false;
        yield return new WaitForSeconds(0.25f);
        canCollide = true;
    }
}
