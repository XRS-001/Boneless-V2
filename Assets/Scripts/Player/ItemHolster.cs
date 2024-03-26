using RootMotion.Demos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolster : MonoBehaviour
{
    public GameObject itemHolstered;
    public AudioClip holsterAudio;
    public void Holster(GameObject objectToHolster)
    {
        if (!itemHolstered)
        {
            itemHolstered = objectToHolster;
            itemHolstered.SetActive(false);
            AudioSource.PlayClipAtPoint(holsterAudio, transform.TransformPoint(GetComponent<SphereCollider>().center), 1);
        }
    }
    public void GrabFromHolster(GrabPhysics grab)
    {
        itemHolstered.transform.position = grab.transform.position;
        itemHolstered.SetActive(true);
        StartCoroutine(Delay(grab));
    }
    IEnumerator Delay(GrabPhysics grab)
    {
        yield return new WaitForSeconds(0.1f);
        if(itemHolstered)
        {
            GrabTwoAttach spawnedGrab = itemHolstered.GetComponent<GrabTwoAttach>();
            grab.grab = spawnedGrab;
            grab.GenericGrab(null, spawnedGrab.GetComponent<Rigidbody>());
            grab.grab.handGrabbing = grab;
            itemHolstered = null;
            foreach(Collider collider in spawnedGrab.colliders)
                collider.enabled = false;
            yield return new WaitForSeconds(1f);
            foreach (Collider collider in spawnedGrab.colliders)
                collider.enabled = true;
        }
    }
    private void LateUpdate()
    {
        Collider[] potentialItems = Physics.OverlapSphere(transform.TransformPoint(GetComponent<SphereCollider>().center), GetComponent<SphereCollider>().radius);
        foreach (Collider collider in potentialItems)
        {
            if (collider.GetComponentInParent<BaseGrab>())
            {
                if (collider.GetComponentInParent<BaseGrab>().canHolster)
                {
                    collider.GetComponentInParent<BaseGrab>().potentialHolster = this;
                }
            }
        }
    }
}
