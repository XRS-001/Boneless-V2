using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static EnumDeclaration;
public class GrabPhysics : MonoBehaviour
{
    public handTypeEnum handType;
    public Rigidbody rb;
    public SetPose poseSetup { get; private set; }

    [Tooltip("The collider group of the hand")]
    public GameObject colliderGroup;
    public Collider forearm;

    public InputActionProperty grabInputSource;
    public float radius = 0.1f;
    [Tooltip("The local offset for the grab zone of the hand")]
    public Vector3 grabZoneOffset;
    [HideInInspector]
    public Vector3 grabZonePosition;
    public LayerMask grabLayer;

    private ConfigurableJoint configJoint;
    [HideInInspector]
    public GrabTwoAttach grab;
    public bool isGrabbing = false;
    [HideInInspector]
    public bool isClimbing = false;
    private bool canGrab = true;
    public float connectedMass;
    Collider[] nearbyColliders;
    Collider closestCollider;
    Rigidbody nearbyRigidbody;
    private void Start()
    {
        poseSetup = GetComponent<SetPose>();
    }
    public void Grab()
    {
        grab.SetAttachPoint(handType);
        isGrabbing = true;
        configJoint = gameObject.AddComponent<ConfigurableJoint>();
        configJoint.autoConfigureConnectedAnchor = false;
        configJoint.connectedAnchor = grab.attachPoint;

        if (!(grab is GrabDynamic))
        {
            transform.rotation = nearbyRigidbody.rotation * Quaternion.Euler(grab.attachRotation);
        }
        else
        {
            transform.rotation = Quaternion.Euler(grab.attachRotation);
        }

        configJoint.xMotion = ConfigurableJointMotion.Locked;
        configJoint.yMotion = ConfigurableJointMotion.Locked;
        configJoint.zMotion = ConfigurableJointMotion.Locked;

        configJoint.angularXMotion = ConfigurableJointMotion.Locked;
        configJoint.angularYMotion = ConfigurableJointMotion.Locked;
        configJoint.angularZMotion = ConfigurableJointMotion.Locked;

        configJoint.connectedBody = nearbyRigidbody;
        connectedMass = nearbyRigidbody.mass;
        if(grab is GrabDynamic)
        {
            poseSetup.setDynamicPose = true;
        }
        grab.SetPose(handType);
        poseSetup.pose = grab.pose;
        poseSetup.SetupPose();
        grab.isGrabbing = true;
        foreach(Collider collider in grab.colliders)
        {
            Physics.IgnoreCollision(collider, forearm);
        }
        nearbyRigidbody.mass = 1;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        grabZonePosition = transform.position - (transform.rotation * grabZoneOffset);
        bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;
        bool isGrabButtonPressedThisFrame = grabInputSource.action.WasPressedThisFrame();

        nearbyColliders = Physics.OverlapSphere(grabZonePosition, radius, grabLayer, QueryTriggerInteraction.Ignore);
        if (nearbyColliders.Length > 0)
        {
            closestCollider = FindClosestInteractable(nearbyColliders);
            if(closestCollider.attachedRigidbody)
            {
                nearbyRigidbody = closestCollider.attachedRigidbody;
            }
            else
            {
                nearbyRigidbody = null;
            }
            if (nearbyRigidbody)
            {
                if (!nearbyRigidbody.GetComponent<GrabTwoAttach>().twoHanded)
                {
                    if (!nearbyRigidbody.GetComponent<GrabTwoAttach>().isGrabbing)
                    {
                        grab = nearbyRigidbody.GetComponent<GrabTwoAttach>();
                    }
                }
                else
                {
                    grab = nearbyRigidbody.GetComponent<GrabTwoAttach>();
                }
                if (grab)
                {
                    grab.isHovering = true;
                }
            }
            else if (closestCollider)
            {
                grab = closestCollider.GetComponent<GrabDynamic>();
                if (grab == null)
                {
                    grab = closestCollider.transform.parent.GetComponent<GrabDynamic>();
                }
                if (grab)
                {
                    grab.isHovering = true;
                }
            }
            if (isGrabButtonPressedThisFrame && !isGrabbing && canGrab)
            {
                if(nearbyRigidbody)
                {
                    if (grab)
                    {
                        if (!grab.isGrabbing)
                        {
                            colliderGroup.SetActive(false);
                            StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                            grab.handGrabbing = this;
                            Grab();
                        }
                        else if (grab.twoHanded)
                        {
                            grab.isTwoHandGrabbing = true;
                            StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                            grab.secondHandGrabbing = this;
                            colliderGroup.SetActive(false);
                            Grab();
                        }
                    }
                }
                else
                {
                    isClimbing = true;
                    if (!grab.isGrabbing)
                    {
                        grab.handGrabbing = this;
                    }
                    else
                    {
                        grab.secondHandGrabbing = this;
                    }
                    grab.isHovering = true;
                    colliderGroup.SetActive(false);

                    grab.SetAttachPoint(handType);
                    isGrabbing = true;
                    transform.rotation = Quaternion.Euler(grab.attachRotation);
                    poseSetup.setDynamicPose = true;
                    grab.SetPose(handType);
                    poseSetup.pose = grab.pose;
                    poseSetup.SetupPose();
                    grab.isGrabbing = true;
                    configJoint = gameObject.AddComponent<ConfigurableJoint>();

                    configJoint.xMotion = ConfigurableJointMotion.Locked;
                    configJoint.yMotion = ConfigurableJointMotion.Locked;
                    configJoint.zMotion = ConfigurableJointMotion.Locked;

                    configJoint.angularXMotion = ConfigurableJointMotion.Locked;
                    configJoint.angularYMotion = ConfigurableJointMotion.Locked;
                    configJoint.angularZMotion = ConfigurableJointMotion.Locked;

                    configJoint.autoConfigureConnectedAnchor = false;
                    configJoint.connectedAnchor = grab.transform.TransformPoint(grab.attachPoint);
                    isGrabbing = true;
                }
            }
        }
        else
        {
            if (grab)
            {
                grab.isHovering = false;
            }
            closestCollider = null;
            nearbyRigidbody = null;
        }
        if (!isGrabButtonPressed && isGrabbing)
        {
            isClimbing = false;
            isGrabbing = false;
            if (grab != null)
            {
                if (!grab.isTwoHandGrabbing)
                {
                    if (configJoint)
                    {
                        Destroy(configJoint);
                    }
                    grab.handGrabbing = null;
                    grab.isGrabbing = false;
                    colliderGroup.SetActive(true);
                    grab.transform.parent = null;
                    foreach (Collider collider in grab.colliders)
                    {
                        Physics.IgnoreCollision(collider, forearm, false);
                    }
                }
                else
                {
                    if (grab.handGrabbing == this)
                    {
                        grab.handGrabbing = grab.secondHandGrabbing;
                    }
                    if (configJoint)
                    {
                        Destroy(configJoint);
                    }
                    StartCoroutine(DelayCollisionExit());
                    grab.isTwoHandGrabbing = false;
                    if (grab.handGrabbing == this)
                    {
                        grab.secondHandGrabbing.colliderGroup.SetActive(true);
                    }
                    grab.secondHandGrabbing = null;
                    foreach (Collider collider in grab.colliders)
                    {
                        Physics.IgnoreCollision(collider, forearm, false);
                    }
                }
                nearbyRigidbody.mass = connectedMass;
                connectedMass = 0;
                poseSetup.exitingDynamicPose = true;
                poseSetup.UnSetPose();
            }
            else
            {
                Destroy(configJoint);
            }
            if(grab is GrabDynamic)
            {
                StartCoroutine(WaitTillGrab());
            }
            grab = null;
        }
    }
    IEnumerator WaitTillGrab()
    {
        canGrab = false;
        yield return new WaitForSeconds(poseSetup.poseTransitionDuration);
        canGrab = true;
    }
    IEnumerator DelayCollisionExit()
    {
        GrabPhysics oldHandGrabbing = grab.handGrabbing;
        yield return new WaitForSeconds(0.25f);

        colliderGroup.SetActive(true);
        oldHandGrabbing.colliderGroup.SetActive(true);
    }
    IEnumerator IgnoreCollisionInteractables(Collider collider, Collider[] collidersToIgnore)
    {
        foreach(Collider colliderToIgnore in collidersToIgnore)
        {
            Physics.IgnoreCollision(collider, colliderToIgnore, true);
        }
        yield return new WaitForSeconds(0.1f);

        foreach (Collider colliderToIgnore in collidersToIgnore)
        {
            Physics.IgnoreCollision(collider, colliderToIgnore, false);
        }
    }
    public Collider FindClosestInteractable(Collider[] collidersGrabbed)
    {
        Collider closestCollider = collidersGrabbed[0];
        Vector3 closestPosition = collidersGrabbed[0].transform.position;
        float closestDistance = Vector3.Distance(transform.position, closestPosition);

        // Loop through all positions and find the closest one
        for (int i = 1; i < collidersGrabbed.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, collidersGrabbed[i].transform.position);

            if (distance < closestDistance)
            {
                closestCollider = collidersGrabbed[i];
                closestDistance = distance;
            }
        }
        return closestCollider;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawSphere(grabZonePosition, radius);
    }
}
