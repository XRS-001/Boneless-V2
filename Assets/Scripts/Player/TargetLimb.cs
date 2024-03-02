using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetLimb : MonoBehaviour
{
    public bool root;
    public Transform target;
    public Transform relative;
    private ConfigurableJoint joint;
    private Quaternion initialRotation;
    public bool isColliding;
    public Collider colliderColliding = null;
    // Start is called before the first frame update
    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        if (root)
        {
            initialRotation = relative.rotation * target.transform.rotation;
        }
        else
        {
            initialRotation = target.transform.localRotation;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (root)
        {
            joint.targetRotation = Quaternion.Inverse(Quaternion.Inverse(relative.rotation) * target.rotation) * initialRotation;
        }
        else
        {
            joint.targetRotation = Quaternion.Inverse(target.localRotation) * initialRotation;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Interactable") && collision.gameObject.layer != LayerMask.NameToLayer("Ragdoll"))
        {
            isColliding = true;
            colliderColliding = collision.collider;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Interactable") && collision.gameObject.layer != LayerMask.NameToLayer("Ragdoll"))
        {
            isColliding = false;
        }
    }
}
