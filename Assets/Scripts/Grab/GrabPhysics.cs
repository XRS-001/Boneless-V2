using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static EnumDeclaration;
using System.Linq;
using System.Collections.Generic;
using RootMotion.Demos;
using Unity.VisualScripting;

public class GrabPhysics : MonoBehaviour
{
    [Header("Hand Info")]
    public InputActionProperty grabInputSource;
    public handTypeEnum handType;
    public Transform controller;
    public Rigidbody rb;
    [Tooltip("The colliders of the hand")]
    public Collider[] colliders;
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

    private ConfigurableJoint configJoint;
    [HideInInspector]
    public GrabTwoAttach grab;
    [HideInInspector]
    public DetectCollisionJoint detectCollision;
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
    public void Grab()
    {
        if(grab is GrabSword && grab.secondHandGrabbing)
        {
            GrabSword grabSword = grab as GrabSword;
            if (Vector3.Distance(transform.position, grab.transform.TransformPoint(grabSword.guardPosition)) < Vector3.Distance(grab.handGrabbing.transform.position, grab.transform.TransformPoint(grabSword.guardPosition)))
            {
                grabSword.higherHand = controller;

                configJoint.angularXMotion = ConfigurableJointMotion.Free;
                configJoint.angularYMotion = ConfigurableJointMotion.Free;
                configJoint.angularZMotion = ConfigurableJointMotion.Free;
            }
            else
            {
                grabSword.higherHand = grab.handGrabbing.controller.transform;

                grab.handGrabbing.configJoint.angularXMotion = ConfigurableJointMotion.Free;
                grab.handGrabbing.configJoint.angularYMotion = ConfigurableJointMotion.Free;
                grab.handGrabbing.configJoint.angularZMotion = ConfigurableJointMotion.Free;
            }

            twoHandedInteraction = new GameObject("TwoHandedInteraction");
            twoHandedInteraction.transform.parent = controller.parent.transform;
            rig.rightHandPhysicsTarget = twoHandedInteraction.transform;
            rig.leftHandPhysicsTarget = twoHandedInteraction.transform;
        }
        if (grab is GrabDynamic)
        {
            poseSetup.setDynamicPose = true;
        }
        grab.SetPose(handType);
        poseSetup.pose = grab.pose;
        poseSetup.SetupPose();
        grab.SetAttachPoint(handType);
        isGrabbing = true;
        configJoint = gameObject.AddComponent<ConfigurableJoint>();

        configJoint.xMotion = ConfigurableJointMotion.Locked;
        configJoint.yMotion = ConfigurableJointMotion.Locked;
        configJoint.zMotion = ConfigurableJointMotion.Locked;

        configJoint.angularXMotion = ConfigurableJointMotion.Locked;
        configJoint.angularYMotion = ConfigurableJointMotion.Locked;
        configJoint.angularZMotion = ConfigurableJointMotion.Locked;


        configJoint.autoConfigureConnectedAnchor = false;
        configJoint.connectedAnchor = grab.attachPoint;

        if (grab is not GrabDynamic)
        {
            transform.rotation = nearbyRigidbody.rotation * Quaternion.Euler(grab.attachRotation);
        }
        else
        {
            transform.rotation = Quaternion.Euler(grab.attachRotation);
        }

        configJoint.connectedBody = nearbyRigidbody;
        if (!grab.secondHandGrabbing)
        {
            detectCollision = grab.AddComponent<DetectCollisionJoint>();
            connectedMass = nearbyRigidbody.mass;
        }
        else
        {
            detectCollision = grab.handGrabbing.detectCollision;
            connectedMass = grab.handGrabbing.connectedMass;
        }
        grab.isGrabbing = true;
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
        colliders[0].transform.parent.gameObject.SetActive(false);
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
        if(grab is GrabSword && grab.isTwoHandGrabbing)
        {
            if(grab.handGrabbing.configJoint.angularXMotion == ConfigurableJointMotion.Free)
            {
                grab.handGrabbing.configJoint.angularXMotion = ConfigurableJointMotion.Locked;
                grab.handGrabbing.configJoint.angularYMotion = ConfigurableJointMotion.Locked;
                grab.handGrabbing.configJoint.angularZMotion = ConfigurableJointMotion.Locked;
            }
            if(grab.secondHandGrabbing.configJoint.angularXMotion == ConfigurableJointMotion.Free)
            {
                grab.handGrabbing.configJoint.angularXMotion = ConfigurableJointMotion.Locked;
                grab.handGrabbing.configJoint.angularYMotion = ConfigurableJointMotion.Locked;
                grab.handGrabbing.configJoint.angularZMotion = ConfigurableJointMotion.Locked;
            }
        }
        if (twoHandedInteraction)
        {
            if(grab.handGrabbing.handType == handTypeEnum.Left)
            {
                rig.leftHandPhysicsTarget = grab.handGrabbing.controller;
                if (grab.secondHandGrabbing)
                rig.rightHandPhysicsTarget = grab.secondHandGrabbing.controller;
            }
            else
            {
                if (grab.secondHandGrabbing)
                rig.leftHandPhysicsTarget = grab.secondHandGrabbing.controller;

                rig.rightHandPhysicsTarget = grab.handGrabbing.controller;
            }
            Destroy(twoHandedInteraction);
        }
        else
        {
            twoHandedInteraction = grab.secondHandGrabbing?.twoHandedInteraction;
            if (twoHandedInteraction)
            {
                if (grab.handGrabbing.handType == handTypeEnum.Left)
                {
                    rig.leftHandPhysicsTarget = grab.handGrabbing.controller;
                    rig.rightHandPhysicsTarget = grab.secondHandGrabbing.controller;
                }
                else
                {
                    rig.leftHandPhysicsTarget = grab.secondHandGrabbing.controller;
                    rig.rightHandPhysicsTarget = grab.handGrabbing.controller;
                }
                Destroy(twoHandedInteraction);
            }
        }
        isGrabbing = false;
        if (grab != null)
        {
            poseSetup.exitingDynamicPose = true;
            poseSetup.UnSetPose();
            if (!grab.isTwoHandGrabbing)
            {
                Destroy(detectCollision);
                if (configJoint)
                {
                    Destroy(configJoint);
                }
                grab.handGrabbing = null;
                grab.isGrabbing = false;
                colliders[0].transform.parent.gameObject.SetActive(true);
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
                grab.handGrabbing.colliders[0].transform.parent.gameObject.SetActive(true);
                colliders[0].transform.parent.gameObject.SetActive(true);
                grab.isTwoHandGrabbing = false;
                grab.secondHandGrabbing = null;
            }
            if (nearbyRigidbody)
            {
                if(!grab.isGrabbing)
                {
                    nearbyRigidbody.mass = connectedMass;
                }
            }
            StartCoroutine(ExitGrab(grab.colliders));
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

        nearbyColliders = Physics.OverlapCapsule(point0, point1, 0.2f, distanceGrabLayer).ToList();
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
        GrabSword grabSword = grab as GrabSword;
        if (initialRotationOffset == Quaternion.identity)
        {
            initialRotationOffset = Quaternion.Inverse(Quaternion.FromToRotation(grab.secondHandGrabbing.controller.position * 15, grab.handGrabbing.controller.position * 15)) * grabSword.higherHand.rotation * Quaternion.AngleAxis(15, grabSword.higherHand.right);
        }

        twoHandedInteraction.transform.rotation = grabSword.higherHand.rotation * Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(grab.secondHandGrabbing.controller.position * 15, grab.handGrabbing.controller.position * 15) * initialRotationOffset, 1);
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
                                    Grab();
                                }
                                else if (grab.twoHanded)
                                {
                                    grab.isTwoHandGrabbing = true;
                                    StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                                    grab.secondHandGrabbing = this;
                                    grab.handGrabbing.colliders[0].transform.parent.gameObject.SetActive(false);
                                    colliders[0].transform.parent.gameObject.SetActive(false);
                                    Grab();
                                }
                            }
                        }
                        else
                        {
                            if (!grab.isGrabbing)
                            {
                                StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                                grab.handGrabbing = this;
                                Grab();
                            }
                            else if (grab.twoHanded)
                            {
                                grab.isTwoHandGrabbing = true;
                                StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                                grab.secondHandGrabbing = this;
                                grab.handGrabbing.colliders[0].transform.parent.gameObject.SetActive(false);
                                colliders[0].transform.parent.gameObject.SetActive(false);
                                Grab();
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
            UnGrab();
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
    IEnumerator ExitGrab(Collider[] inputColliders)
    {
        foreach (Collider collider in inputColliders)
        {
            foreach (Collider handCollider in colliders)
            {
                Physics.IgnoreCollision(collider, handCollider, true);
            }
        }
        yield return new WaitForSeconds(1);

        foreach(Collider collider in inputColliders)
        {
            foreach(Collider handCollider in colliders)
            {
                Physics.IgnoreCollision(collider, handCollider, false);
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
