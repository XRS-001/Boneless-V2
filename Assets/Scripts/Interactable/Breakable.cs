using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public GameObject breakableParent;
    private List<Rigidbody> breakables;
    public float forceNeededToBreak;
    public AudioClip breakClip;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 5f)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Interactable")
                || collision.gameObject.layer == LayerMask.NameToLayer("NonInteractive")
                || collision.gameObject.layer == LayerMask.NameToLayer("Default")
                || collision.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
                forceNeededToBreak -= collision.relativeVelocity.magnitude * 2;
            else
                forceNeededToBreak -= collision.relativeVelocity.magnitude;
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Projectile"))
                forceNeededToBreak -= collision.rigidbody.velocity.magnitude;

        if (forceNeededToBreak < 0 && collision.gameObject.layer != LayerMask.NameToLayer("Projectile"))
            Break(collision.relativeVelocity.magnitude * 2, collision.relativeVelocity, collision.GetContact(0).point);
        else if (forceNeededToBreak < 0)
            Break(collision.rigidbody.velocity.magnitude * 2, collision.relativeVelocity, collision.GetContact(0).point);
    }
    public void Break(float breakForce, Vector3 velocity, Vector3 breakPoint)
    {
        BaseGrab grab = GetComponent<BaseGrab>();
        if (grab.isGrabbing)
        {
            if (grab.secondHandGrabbing)
            {
                grab.secondHandGrabbing.UnGrab();
            }
            if (grab.handGrabbing)
            {
                grab.handGrabbing.UnGrab();
            }
        }
        grab.enabled = false;
        breakableParent.SetActive(true);

        AudioSource.PlayClipAtPoint(breakClip, breakPoint, Mathf.Clamp(velocity.magnitude / 10, 0.5f, 1.5f));
        breakableParent.transform.parent = null;
        gameObject.SetActive(false);

        breakables = breakableParent.GetComponentsInChildren<Rigidbody>().ToList();
        foreach (Rigidbody rb in breakables)
        {
            rb.AddExplosionForce(Mathf.Clamp(breakForce, 0, 500), transform.position, 100);
            rb.AddForce(Vector3.ClampMagnitude(velocity * 25, 250));
        }
    }
}
