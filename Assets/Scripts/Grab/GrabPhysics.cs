using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static EnumDeclaration;
using System.Linq;
using System;
using System.Collections.Generic;

public class GrabPhysics : MonoBehaviour
{
    [Header("Hand Info")]
    public InputActionProperty grabInputSource;
    public handTypeEnum handType;
    public Rigidbody rb;
    [Tooltip("The collider group of the hand")]
    public GameObject colliderGroup;
    public Collider forearm;

    [Header("Grabbing")]
    private AudioSource audioSource;
    public float radius = 0.1f;
    [Tooltip("The local offset for the grab zone of the hand")]
    public Vector3 grabZoneOffset;
    [HideInInspector]
    public Vector3 grabZonePosition;
    public LayerMask grabLayer;
    public AudioClip grabSound;
    public float grabVolume;
    public SetPose poseSetup { get; private set; }

    [Header("Distance Grabbing")]
    [Tooltip("A small icon to show on the interactable when hovering")]
    public GameObject hoverIcon;
    private GameObject spawnedIcon;
    public Transform cameraTransform;
    public bool distanceGrab;
    public Transform distanceGrabDetection;
    private CapsuleCollider distanceGrabZone;
    public LayerMask distanceGrabLayer;

    private ConfigurableJoint configJoint;
    [HideInInspector]
    public GrabTwoAttach grab;
    [HideInInspector]
    private bool canGrab = true;

    [Header("Grabbed Data")]
    public bool isGrabbing = false;
    public float connectedMass;

    List<Collider> nearbyColliders;
    Collider closestCollider;
    Rigidbody nearbyRigidbody;
    private void Start()
    {
        //look for a capsule collider on the distanceGrabTransform
        distanceGrabZone = distanceGrabDetection.GetComponent<CapsuleCollider>();
        audioSource = gameObject.AddComponent<AudioSource>();
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
        if(!grab.secondHandGrabbing)
        {
            connectedMass = nearbyRigidbody.mass;
        }
        else
        {
            connectedMass = grab.handGrabbing.connectedMass;
        }
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
        audioSource.PlayOneShot(grabSound, grabVolume);
    }
    public void GrabClimbable()
    {
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
        audioSource.PlayOneShot(grabSound, grabVolume);
    }
    public void UnGrab()
    {
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
            if (nearbyRigidbody)
            {
                if(!grab.isGrabbing)
                {
                    nearbyRigidbody.mass = connectedMass;
                }
            }
            connectedMass = 0;
            poseSetup.exitingDynamicPose = true;
            poseSetup.UnSetPose();
        }
        else
        {
            Destroy(configJoint);
        }
        if (grab is GrabDynamic)
        {
            StartCoroutine(WaitTillGrab());
        }
        grab = null;
    }
    public void SetGrab()
    {
        closestCollider = FindClosestInteractable(nearbyColliders);
        if (closestCollider.attachedRigidbody)
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
                //verify the grab is not an enemy ragdoll before creating the hover icon
                if (!spawnedIcon && grab.gameObject.layer != LayerMask.NameToLayer("Ragdoll") && !isGrabbing)
                {
                    spawnedIcon = Instantiate(hoverIcon);
                }
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
    }
    private void CheckDistanceGrab()
    {
        if (!(nearbyColliders.Count > 0) && distanceGrab)
        {
            Vector3 direction = distanceGrabDetection.forward;
            float offset = distanceGrabZone.height / 2 - distanceGrabZone.radius;

            Vector3 localPoint0 = distanceGrabZone.center - direction * offset;
            Vector3 localPoint1 = distanceGrabZone.center + direction * offset;

            Vector3 point0 = distanceGrabDetection.TransformPoint(localPoint0);
            Vector3 point1 = distanceGrabDetection.TransformPoint(localPoint1);

            Vector3 r = transform.TransformVector(distanceGrabZone.radius, distanceGrabZone.radius, distanceGrabZone.radius);
            float distanceRadius = Enumerable.Range(0, 3).Select(xyz => xyz == distanceGrabZone.direction ? 0 : r[xyz]).Select(Mathf.Abs).Max();

            nearbyColliders = Physics.OverlapCapsule(point0, point1, distanceRadius, grabLayer).ToList();
            if (nearbyColliders.Count > 0)
            {
                closestCollider = FindClosestInteractable(nearbyColliders);
                //prevent grabbing if it's a dynamic grab
                if (closestCollider.GetComponent<GrabDynamic>())
                {
                    nearbyColliders.Clear();
                }
            }
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (nearbyColliders != null)
        {
            if (nearbyColliders.Count == 0)
            {
                //destroy the hover icon if not hovering
                if (grab)
                {
                    Destroy(spawnedIcon);
                    grab.isHovering = false;
                }
            }
        }
        //set the hover icons transform if not grabbing, else destroy it
        if (spawnedIcon && grab)
        {
            spawnedIcon.transform.position = grab.transform.position;
            spawnedIcon.transform.LookAt(cameraTransform.position);
            if (grab.isGrabbing)
            {
                Destroy(spawnedIcon);
            }
        }
        grabZonePosition = transform.position - (transform.rotation * grabZoneOffset);
        bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;
        bool isGrabButtonPressedThisFrame = grabInputSource.action.WasPressedThisFrame();

        nearbyColliders = Physics.OverlapSphere(grabZonePosition, radius, grabLayer, QueryTriggerInteraction.Ignore).ToList();
        //if distance grab is enabled, check if an object is in the distance grab zone as defined by a capsule collider under the hand
        CheckDistanceGrab();
        if (nearbyColliders.Count > 0)
        {
            SetGrab();
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
                    GrabClimbable();
                }
            }
        }
        else
        {
            closestCollider = null;
            nearbyRigidbody = null;
        }
        if (!isGrabButtonPressed && isGrabbing)
        {
            UnGrab();
        }
    }
    IEnumerator WaitTillGrab()
    {
        //delay until you can regrab dynamic attaches
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
    IEnumerator IgnoreCollisionInteractables(Collider collider, List<Collider> collidersToIgnore)
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
    public Collider FindClosestInteractable(List<Collider> collidersGrabbed)
    {
        Collider closestCollider = collidersGrabbed[0];
        Vector3 closestPosition = collidersGrabbed[0].transform.position;
        float closestDistance = Vector3.Distance(transform.position, closestPosition);

        // Loop through all positions and find the closest one
        for (int i = 1; i < collidersGrabbed.Count; i++)
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
