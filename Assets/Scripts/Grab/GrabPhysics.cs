using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static EnumDeclaration;
using System.Linq;
using System.Collections.Generic;

public class GrabPhysics : MonoBehaviour
{
    [Header("Hand Info")]
    public InputActionProperty grabInputSource;
    public handTypeEnum handType;
    public Transform controller;
    public Rigidbody rb;
    [Tooltip("The collider group of the hand")]
    public GameObject colliderGroup;
    public Collider forearm;
    [Tooltip("The hand presence")]
    public FollowTarget followTarget;
    private PhysicsRig rig;

    private AudioSource audioSource;
    [Header("Grabbing")]
    public float radius = 0.1f;
    [Tooltip("How close does the hand need to be to the interactable for it to calculate the attach point")]
    public float calculationDistance;
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

    private FixedJoint fixedJoint;
    [HideInInspector]
    public GrabTwoAttach grab;
    [HideInInspector]
    private bool canGrab = true;

    [Header("Grabbed Data")]
    public bool isGrabbing = false;
    public float connectedMass;

    List<Collider> nearbyColliders;
    Collider closestCollider;
    [HideInInspector]
    public Rigidbody nearbyRigidbody;
    //two handed interaction
    private GameObject twoHandedInteraction;
    private Quaternion initialRotationOffset = Quaternion.identity;
    private void Start()
    {
        rig = transform.root.GetComponent<PhysicsRig>();
        //look for a capsule collider on the distanceGrabTransform
        distanceGrabZone = distanceGrabDetection.GetComponent<CapsuleCollider>();
        audioSource = gameObject.AddComponent<AudioSource>();
        poseSetup = GetComponent<SetPose>();
    }
    public void Grab(bool setPose)
    {
        if (grab is not GrabSword || !grab.isTwoHandGrabbing)
        {
            //check if it's regrabbing from a two handed ungrab
            if(setPose)
            {
                if (grab is GrabDynamic)
                {
                    poseSetup.setDynamicPose = true;
                }
                grab.SetPose(handType);
                poseSetup.pose = grab.pose;
                poseSetup.SetupPose();
                grab.SetAttachPoint(handType);
            }
            grab.handGrabbing = this;
            isGrabbing = true;
            fixedJoint = gameObject.AddComponent<FixedJoint>();
            fixedJoint.autoConfigureConnectedAnchor = false;
            fixedJoint.connectedAnchor = grab.attachPoint;

            if (grab is not GrabDynamic)
            {
                transform.rotation = nearbyRigidbody.rotation * Quaternion.Euler(grab.attachRotation);
            }
            else
            {
                transform.rotation = Quaternion.Euler(grab.attachRotation);
            }

            fixedJoint.connectedBody = nearbyRigidbody;
            if (!grab.secondHandGrabbing)
            {
                connectedMass = nearbyRigidbody.mass;
            }
            else
            {
                connectedMass = grab.handGrabbing.connectedMass;
            }
            grab.isGrabbing = true;
            foreach (Collider collider in grab.colliders)
            {
                Physics.IgnoreCollision(collider, forearm);
            }
            nearbyRigidbody.mass = 1;
            audioSource.PlayOneShot(grabSound, grabVolume);
        }
        else
        {
            GrabSword grabSword = grab as GrabSword;
            if (Vector3.Distance(transform.position, grab.transform.TransformPoint(grabSword.guardPosition)) < Vector3.Distance(grab.handGrabbing.transform.position, grab.transform.TransformPoint(grabSword.guardPosition)))
                grabSword.higherHand = controller;
            else
            {
                grabSword.higherHand = grab.handGrabbing.controller.transform;
            }
            twoHandedInteraction = new GameObject("TwoHandedInteraction");
            twoHandedInteraction.transform.parent = GameObject.Find("TrackedObjects").transform;
            if (handType == handTypeEnum.Left)
            {
                rig.rightHandPhysicsTarget = twoHandedInteraction.transform;
            }
            else
            {
                rig.leftHandPhysicsTarget = twoHandedInteraction.transform;
            }
            grab.SetAttachPoint(handType);
            isGrabbing = true;
            grab.SetPose(handType);
            poseSetup.pose = grab.pose;
            poseSetup.SetupPose();
            audioSource.PlayOneShot(grabSound, grabVolume);
            foreach (Collider collider in grab.colliders)
            {
                Physics.IgnoreCollision(collider, forearm);
            }
        }

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
        colliderGroup.SetActive(false);
        grab.SetAttachPoint(handType);
        isGrabbing = true;
        transform.rotation = Quaternion.Euler(grab.attachRotation);
        poseSetup.setDynamicPose = true;
        grab.SetPose(handType);
        poseSetup.pose = grab.pose;
        poseSetup.SetupPose();
        grab.isGrabbing = true;
        fixedJoint = gameObject.AddComponent<FixedJoint>();
        fixedJoint.autoConfigureConnectedAnchor = false;
        fixedJoint.connectedAnchor = grab.transform.TransformPoint(grab.attachPoint);
        isGrabbing = true;
        audioSource.PlayOneShot(grabSound, grabVolume);
    }
    public void SwordUnGrab()
    {
        if (grab.secondHandGrabbing == this)
        {
            if (handType == handTypeEnum.Right)
            {
                rig.leftHandPhysicsTarget = grab.handGrabbing.controller;
            }
            else
            {
                rig.rightHandPhysicsTarget = grab.handGrabbing.controller;
            }
        }
        else
        {
            if (handType == handTypeEnum.Right)
            {
                rig.rightHandPhysicsTarget = grab.handGrabbing.controller;
            }
            else
            {
                rig.leftHandPhysicsTarget = grab.handGrabbing.controller;
            }
            grab.isGrabbing = false;
            GrabPhysics secondHand = grab.secondHandGrabbing;
            grab.secondHandGrabbing.UnGrab(false);
            grab.handGrabbing = secondHand;
            grab.handGrabbing.grab = grab;
            grab.handGrabbing.Grab(false);
        }
        initialRotationOffset = Quaternion.identity;
        followTarget.overrideTarget = false;
        Destroy(twoHandedInteraction);
    }
    public void UnGrab(bool setPose)
    {
        isGrabbing = false;
        if (grab != null)
        {
            if (setPose)
            {
                poseSetup.exitingDynamicPose = true;
                poseSetup.UnSetPose();
            }
            if (!grab.isTwoHandGrabbing)
            {
                if (fixedJoint)
                {
                    Destroy(fixedJoint);
                }
                grab.handGrabbing = null;
                grab.isGrabbing = false;
                foreach (Collider collider in grab.colliders)
                {
                    Physics.IgnoreCollision(collider, forearm, false);
                }
                colliderGroup.SetActive(true);
            }
            else
            {
                if(grab is GrabSword)
                {
                    //custom logic for swords
                    SwordUnGrab();
                }
                grab.handGrabbing.colliderGroup.SetActive(true);
                colliderGroup.SetActive(true);
                grab.isTwoHandGrabbing = false;
                grab.secondHandGrabbing = null;
                foreach (Collider collider in grab.colliders)
                {
                    Physics.IgnoreCollision(collider, forearm, false);
                }
                if (grab.handGrabbing == this)
                {
                    grab.handGrabbing = grab.secondHandGrabbing;
                }
                if (fixedJoint)
                {
                    Destroy(fixedJoint);
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
        nearbyRigidbody = closestCollider.attachedRigidbody ?? null;
        if (nearbyRigidbody)
        {
            grab = nearbyRigidbody.GetComponent<GrabTwoAttach>();
            //verify the grab is not an enemy ragdoll before creating the hover icon
            if (!spawnedIcon && grab.gameObject.layer != LayerMask.NameToLayer("Ragdoll") && !isGrabbing && !grab.isGrabbing)
            {
                spawnedIcon = Instantiate(hoverIcon);
            }
        }
        else if (closestCollider)
        {
            grab = closestCollider.GetComponent<GrabDynamic>();
            //check for interactable components to be on parents instead of the collider itself
            if (grab == null)
            {
                grab = closestCollider.transform.parent.GetComponent<GrabDynamic>();
            }
        }
    }
    private void CheckDistanceGrab()
    {
        Vector3 point0 = distanceGrabDetection.position - distanceGrabDetection.forward * distanceGrabZone.height;
        Vector3 point1 = distanceGrabDetection.position + distanceGrabDetection.forward * distanceGrabZone.height;

        nearbyColliders = Physics.OverlapCapsule(point0, point1, 0.2f, grabLayer).ToList();
        if (nearbyColliders.Count > 0)
        {
            closestCollider = FindClosestInteractable(nearbyColliders);
            if (closestCollider.GetComponent<GrabDynamic>())
            {
                //do not allow for distance grabbing dynamic interactables
                nearbyColliders.Clear();
            }
            Pierce pierce = closestCollider.transform.root.GetComponent<Pierce>();
            if (pierce)
            {
                if(pierce.stabbed)
                {
                    //do not let grab if stabbing
                    nearbyColliders.Clear();
                }
            }
        }
    }
    void TwoHandedGrabbing()
    {
        followTarget.overrideTarget = true;
        followTarget.Overriding(grab.transform.TransformPoint(grab.attachPoint), grab.rb.rotation * Quaternion.Euler(grab.attachRotation));

        if(initialRotationOffset == Quaternion.identity)
        {
            initialRotationOffset = Quaternion.Inverse(Quaternion.LookRotation(grab.handGrabbing.controller.position - grab.secondHandGrabbing.controller.position, grab.handGrabbing.controller.up) * grab.handGrabbing.controller.rotation);
        }

        GrabSword grabSword = grab as GrabSword;
        twoHandedInteraction.transform.rotation = grabSword.higherHand.transform.rotation * Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(grab.handGrabbing.controller.position - grab.secondHandGrabbing.controller.position, grab.handGrabbing.controller.up) * initialRotationOffset, 0.25f);
        twoHandedInteraction.transform.position = (grab.handGrabbing.controller.position + grab.secondHandGrabbing.controller.position) / 2;
    }
    void FixedUpdate()
    {
        HoverIcon();
        grabZonePosition = transform.position - (transform.rotation * grabZoneOffset);
        bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;
        bool isGrabButtonPressedThisFrame = grabInputSource.action.WasPressedThisFrame();

        if (!isGrabbing)
        {
            nearbyColliders = Physics.OverlapSphere(grabZonePosition, radius, grabLayer, QueryTriggerInteraction.Ignore).ToList();
            //if distance grab is enabled, check if an object is in the distance grab zone as defined by a capsule collider under the hand
            if (!(nearbyColliders.Count > 0) && distanceGrab)
            {
                CheckDistanceGrab();
            }
        }
        if (nearbyColliders.Count > 0)
        {
            SetGrab();
            if (isGrabButtonPressedThisFrame && !isGrabbing && canGrab)
            {
                if(nearbyRigidbody)
                {
                    if (grab)
                    {
                        //if the interactable is a dynamic grab, verify it's valid for grabbing
                        if (grab is GrabDynamic)
                        {
                            GrabDynamic grabDynamic = grab as GrabDynamic;
                            if (grabDynamic.dynamicSettings.isGrabbable)
                            {
                                if (!grab.isGrabbing)
                                {
                                    StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                                    grab.handGrabbing = this;
                                    Grab(true);
                                }
                                else if (grab.twoHanded)
                                {
                                    grab.isTwoHandGrabbing = true;
                                    StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                                    grab.secondHandGrabbing = this;
                                    grab.handGrabbing.colliderGroup.SetActive(false);
                                    colliderGroup.SetActive(false);
                                    Grab(true);
                                }
                            }
                        }
                        else
                        {
                            if (!grab.isGrabbing)
                            {
                                StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                                grab.handGrabbing = this;
                                Grab(true);
                            }
                            else if (grab.twoHanded)
                            {
                                grab.isTwoHandGrabbing = true;
                                StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                                grab.secondHandGrabbing = this;
                                grab.handGrabbing.colliderGroup.SetActive(false);
                                colliderGroup.SetActive(false);
                                Grab(true);
                            }
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
        if (grab)
        {
            if (grab.isTwoHandGrabbing && grab is GrabSword && grab.secondHandGrabbing == this)
            {
                TwoHandedGrabbing();
            }
        }
        if (!isGrabButtonPressed && isGrabbing)
        {
            UnGrab(true);
        }
    }
    void HoverIcon()
    {
        if (grab)
        {
            if (nearbyRigidbody != grab.rb)
            {
                Destroy(spawnedIcon);
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
    }
    IEnumerator WaitTillGrab()
    {
        //delay until you can regrab dynamic attaches
        canGrab = false;
        yield return new WaitForSeconds(poseSetup.poseTransitionDuration);
        canGrab = true;
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
