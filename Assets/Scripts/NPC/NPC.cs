using RootMotion.Dynamics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Joint
{
    public Transform joint;
    public Transform target;
}
public class NPC : MonoBehaviour
{
    private PuppetMaster puppetMaster;
    public float checkRadius;
    public float checkHeight;
    public LayerMask dynamicLayers;
    public Joint[] joints;
    private bool isOverlapping = false;
    public bool isBleeding;
    // Start is called before the first frame update
    void Start()
    {
        puppetMaster = GetComponentInChildren<PuppetMaster>();
    }
    private void FixedUpdate()
    {
        Collider[] overlapColliders = Physics.OverlapCapsule(transform.position, transform.position + (transform.up * checkHeight), checkRadius, dynamicLayers);
        isOverlapping = false;
        foreach (Collider collider in overlapColliders)
        {
            BaseGrab grab = collider.transform.root.GetComponent<BaseGrab>();
            if (grab)
            {
                if (grab.isGrabbing)
                {
                    isOverlapping = true;
                }
            }
            else if (collider.transform.root != transform)
            {
                isOverlapping = true;
            }
        }
        if (isOverlapping)
        {
            Activate();
        }
        else
        {
            Disable();
        }
    }
    private void Activate()
    {
        puppetMaster.mode = PuppetMaster.Mode.Active;
    }
    private void Disable()
    {
        puppetMaster.mode = PuppetMaster.Mode.Disabled;
    }
}
