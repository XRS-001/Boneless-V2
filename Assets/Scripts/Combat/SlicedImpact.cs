using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicedImpact : MonoBehaviour
{
    public AudioClip slicedClip;
    private bool canCollide = true;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Blade") && collision.relativeVelocity.magnitude > 5 && canCollide)
        {
            AudioSource.PlayClipAtPoint(slicedClip, collision.GetContact(0).point, 0.15f);
            canCollide = false;
            Invoke(nameof(WaitTillCanCollide), 0.5f);
        }
    }
    void WaitTillCanCollide()
    {
        canCollide = true;
    }
}
