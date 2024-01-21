using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Breakable : MonoBehaviour
{
    public GameObject breakableParent;
    private List<Rigidbody> breakables;
    public float forceNeededToBreak;
    public AudioSource audioSource;
    public AudioClip breakClip;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 5f)
        {
            forceNeededToBreak -= collision.relativeVelocity.magnitude;
        }
        if (forceNeededToBreak < 0)
        {
            Break(collision.relativeVelocity.magnitude * 2, collision.relativeVelocity);
        }
    }
    public void Break(float breakForce, Vector3 velocity)
    {
        BaseGrab grab = GetComponent<BaseGrab>();
        if (grab.isGrabbing)
        {
            if (grab.handGrabbing)
            {
                grab.handGrabbing.UnGrab();
            }
            if (grab.secondHandGrabbing)
            {
                grab.secondHandGrabbing.UnGrab();
            }
        }
        grab.enabled = false;
        breakableParent.SetActive(true);
        audioSource.pitch = Random.Range(0.8f, 1.1f);
        audioSource.volume = Mathf.Clamp(velocity.magnitude / 10, 0.5f, 1.5f);
        audioSource.PlayOneShot(breakClip);
        breakableParent.transform.parent = null;
        gameObject.SetActive(false);
        breakables = breakableParent.GetComponentsInChildren<Rigidbody>().ToList();
        foreach (Rigidbody rb in breakables)
        {
            rb.AddExplosionForce(breakForce, transform.position, 100);
            rb.AddForce(velocity * 25);
        }
    }
}
