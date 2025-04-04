using System.Collections;
using UnityEngine;

public class CollisionImpact : MonoBehaviour
{
    public AudioClip impactSound;
    public float volumeModifier = 1;
    private bool canCollide = true;
    public LayerMask layers;
    private void OnCollisionEnter(Collision collision)
    {
        if (canCollide && gameObject.activeInHierarchy && (layers.value & 1 << collision.gameObject.layer) != 0 && collision.transform.CompareTag("Blade"))
        {
            AudioSource.PlayClipAtPoint(impactSound, collision.GetContact(0).point, Mathf.Clamp(collision.relativeVelocity.magnitude / 10, 0, 0.1f) * volumeModifier);
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
