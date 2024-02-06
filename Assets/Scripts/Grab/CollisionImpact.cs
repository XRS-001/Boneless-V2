using System.Collections;
using UnityEngine;

public class CollisionImpact : MonoBehaviour
{
    public AudioClip impactSound;
    public float volumeModifier = 1;
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
        if (canCollide && gameObject.activeInHierarchy && collision.gameObject.layer != LayerMask.NameToLayer("LeftHand") && collision.gameObject.layer != LayerMask.NameToLayer("RightHand"))
        {
            audioSource.PlayOneShot(impactSound, Mathf.Clamp(collision.relativeVelocity.magnitude / 10, 0, 0.1f) * volumeModifier);
            StartCoroutine(WaitToCollide());
        }
        else if (canCollide && gameObject.activeInHierarchy)
        {
            if (collision.transform.parent.parent.GetComponent<AudioSource>())
            {
                collision.transform.parent.parent.GetComponent<AudioSource>().PlayOneShot(impactSound, Mathf.Clamp(collision.relativeVelocity.magnitude / 10, 0, 0.1f) * volumeModifier);
            }
            else if (collision.transform.GetChild(0).GetComponent<AudioSource>())
            {
                collision.transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(impactSound, Mathf.Clamp(collision.relativeVelocity.magnitude / 10, 0, 0.1f) * volumeModifier);
            }
            StartCoroutine(WaitToCollide());
        }
    }
    IEnumerator WaitToCollide()
    {
        canCollide = false;
        yield return new WaitForSeconds(0.25f);
        canCollide = true;
    }
}
