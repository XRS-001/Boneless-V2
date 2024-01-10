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
            if (!collision.gameObject.GetComponent<Surface>())
            {
                audioSource.pitch = 1;
                audioSource.PlayOneShot(impactSound, Mathf.Clamp(collision.relativeVelocity.magnitude / 10, 0, 0.1f));
                StartCoroutine(WaitToCollide());
            }
            else
            {
                audioSource.pitch = 1;
                audioSource.PlayOneShot(collision.gameObject.GetComponent<Surface>().surfaceImpactClip, Mathf.Clamp(collision.relativeVelocity.magnitude / 10, 0, 0.1f));
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
