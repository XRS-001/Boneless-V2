using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static EnumDeclaration;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class GrabPhysics : MonoBehaviour
{
    [Header("Hand Info")]
    public InputActionProperty grabInputSource;
    public handTypeEnum handType;
    public Transform controller;
    public Rigidbody rb;
    [Tooltip("The colliders of the hand")]
    public GameObject colliderGroup;

    private AudioSource audioSource;
    [Header("Grabbing")]
    public float radius = 0.1f;
    [Tooltip("The local offset for the grab zone of the hand")]
    public Vector3 grabZoneOffset;
    [HideInInspector]
    public Vector3 grabZonePosition;
    public LayerMask grabLayer;
    public SetPose poseSetup { get; private set; }

    [Header("Distance Grabbing")]
    [Tooltip("A small icon to show on the interactable when hovering")]
    public GameObject hoverIcon;
    private GameObject spawnedIcon;
    public LayerMask distanceGrabLayer;

    private Joint joint;
    [HideInInspector]
    public GrabTwoAttach grab;
    [HideInInspector]
    private bool canGrab = true;

    [Header("Grabbed Data")]
    public bool isGrabbing = false;
    public float connectedMass;

    Collider[] nearbyColliders;
    Collider closestCollider;
    Rigidbody nearbyRigidbody;

    //two handed interaction
    private GameObject twoHandedInteraction;
    private Quaternion initialRotationOffset = Quaternion.identity;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        poseSetup = GetComponent<SetPose>();
    }
    void GrabTwoHandSword()
    {
        GrabSword grabSword = grab as GrabSword;
        if (Vector3.Distance(transform.position, grab.transform.TransformPoint(grabSword.guardPosition)) < Vector3.Distance(grab.handGrabbing.transform.position, grab.transform.TransformPoint(grabSword.guardPosition)))
        {
            grabSword.higherHand = controller;

            ConfigurableJoint configJoint = joint as ConfigurableJoint;
            configJoint.angularXMotion = ConfigurableJointMotion.Free;
            configJoint.angularYMotion = ConfigurableJointMotion.Free;
            configJoint.angularZMotion = ConfigurableJointMotion.Free;
        }
        else
        {
            grabSword.higherHand = grab.handGrabbing.controller.transform;

            ConfigurableJoint configJoint = grab.handGrabbing.joint as ConfigurableJoint;
            configJoint.angularXMotion = ConfigurableJointMotion.Free;
            configJoint.angularYMotion = ConfigurableJointMotion.Free;
            configJoint.angularZMotion = ConfigurableJointMotion.Free;
        }

        twoHandedInteraction = new GameObject("TwoHandedInteraction");
        twoHandedInteraction.transform.parent = controller.parent.transform;
    }
    void UnGrabTwoHandSword()
    {
        ConfigurableJoint configJoint = grab.handGrabbing.joint as ConfigurableJoint;
        if (configJoint.angularXMotion == ConfigurableJointMotion.Free)
        {
            configJoint.angularXMotion = ConfigurableJointMotion.Locked;
            configJoint.angularYMotion = ConfigurableJointMotion.Locked;
            configJoint.angularZMotion = ConfigurableJointMotion.Locked;
        }
        if (configJoint.angularXMotion == ConfigurableJointMotion.Free)
        {
            configJoint.angularXMotion = ConfigurableJointMotion.Locked;
            configJoint.angularYMotion = ConfigurableJointMotion.Locked;
            configJoint.angularZMotion = ConfigurableJointMotion.Locked;
        }
    }
    void TwoHandedGrabbingSword()
    {
        GrabSword grabSword = grab as GrabSword;
        if (initialRotationOffset == Quaternion.identity)
        {
            Quaternion offset = grabSword.higherHand.rotation * Quaternion.AngleAxis(-30, grabSword.higherHand.up);
            initialRotationOffset = Quaternion.Inverse(Quaternion.FromToRotation(grab.secondHandGrabbing.controller.position, grab.handGrabbing.controller.position)) * offset;
        }

        twoHandedInteraction.transform.rotation = grabSword.higherHand.rotation * Quaternion.FromToRotation(grab.secondHandGrabbing.controller.position, grab.handGrabbing.controller.position) * initialRotationOffset;
        twoHandedInteraction.transform.position = (grab.handGrabbing.controller.position + grab.secondHandGrabbing.controller.position) / 2;
    }
    void CheckTwoHandedInteraction()
    {
        if (twoHandedInteraction)
        {
            if (grab.handGrabbing.handType == handTypeEnum.Left)
            {
            }
            else
            {
                if (grab.secondHandGrabbing)
                {

                }
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
                }
                else
                {
                }
                Destroy(twoHandedInteraction);
            }
        }
    }
    void GenericGrab()
    {
        if (grab is GrabSword && grab.secondHandGrabbing)
            GrabTwoHandSword();

        audioSource.Play();
        colliderGroup.SetActive(false);

        if (grab is GrabDynamic)
        {
            poseSetup.setDynamicPose = true;
        }

        grab.SetPose(handType);
        poseSetup.pose = grab.pose;
        poseSetup.SetupPose();

        grab.SetAttachPoint(handType);

        isGrabbing = true;
        grab.isGrabbing = true;

        joint = gameObject.AddComponent<ConfigurableJoint>();
        ConfigurableJoint configJoint = joint as ConfigurableJoint;

        configJoint.xMotion = ConfigurableJointMotion.Locked;
        configJoint.yMotion = ConfigurableJointMotion.Locked;
        configJoint.zMotion = ConfigurableJointMotion.Locked;

        if (grab is not GrabDynamic)
        {
            transform.rotation = nearbyRigidbody.rotation * Quaternion.Euler(grab.attachRotation);
        }
        else
        {
            transform.rotation = Quaternion.Euler(grab.attachRotation);
        }

        configJoint.angularXMotion = ConfigurableJointMotion.Locked;
        configJoint.angularYMotion = ConfigurableJointMotion.Locked;
        configJoint.angularZMotion = ConfigurableJointMotion.Locked;

        configJoint.autoConfigureConnectedAnchor = false;
        configJoint.connectedAnchor = grab.attachPoint;
        configJoint.connectedBody = nearbyRigidbody;

        if (!grab.secondHandGrabbing)
        {
            connectedMass = nearbyRigidbody.mass;
        }
        else
        {
            connectedMass = grab.handGrabbing.connectedMass;
        }

        nearbyRigidbody.mass = 1;
    }
    public void GrabClimbable()
    {
        audioSource.Play();
        colliderGroup.SetActive(false);

        if (!grab.isGrabbing)
        {
            grab.handGrabbing = this;
        }
        else
        {
            grab.secondHandGrabbing = this;
        }

        grab.SetAttachPoint(handType);

        isGrabbing = true;
        grab.isGrabbing = true;

        poseSetup.setDynamicPose = true;
        grab.SetPose(handType);
        poseSetup.pose = grab.pose;
        poseSetup.SetupPose();

        joint = gameObject.AddComponent<ConfigurableJoint>();

        ConfigurableJoint configJoint = joint as ConfigurableJoint;
        configJoint.xMotion = ConfigurableJointMotion.Locked;
        configJoint.yMotion = ConfigurableJointMotion.Locked;
        configJoint.zMotion = ConfigurableJointMotion.Locked;

        transform.rotation = Quaternion.Euler(grab.attachRotation);

        configJoint.angularXMotion = ConfigurableJointMotion.Locked;
        configJoint.angularYMotion = ConfigurableJointMotion.Locked;
        configJoint.angularZMotion = ConfigurableJointMotion.Locked;

        configJoint.autoConfigureConnectedAnchor = false;
        configJoint.connectedAnchor = grab.transform.TransformPoint(grab.attachPoint);
    }
    public void UnGrab()
    {
        if (grab is GrabSword && grab.isTwoHandGrabbing)
        {
            UnGrabTwoHandSword();
        }
        CheckTwoHandedInteraction();
        isGrabbing = false;
        if (grab != null)
        {
            poseSetup.exitingDynamicPose = true;
            poseSetup.UnSetPose();
            if (!grab.isTwoHandGrabbing)
            {
                if (joint)
                {
                    Destroy(joint);
                }
                grab.handGrabbing = null;
                grab.isGrabbing = false;
                colliderGroup.SetActive(true);
            }
            else
            {
                if (grab.handGrabbing == this)
                {
                    grab.handGrabbing = grab.secondHandGrabbing;
                }
                if (joint)
                {
                    Destroy(joint);
                }
                grab.handGrabbing.colliderGroup.SetActive(true);
                colliderGroup.SetActive(true);

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
            connectedMass = 0;
        }
        if (grab is GrabDynamic)
        {
            StartCoroutine(WaitTillGrab());
        }
        grab = null;
    }
    void CheckGrabInput()
    {
        bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;
        bool isGrabButtonPressedThisFrame = grabInputSource.action.WasPressedThisFrame();
        if (isGrabButtonPressedThisFrame && !isGrabbing && canGrab && grab)
        {
            if (nearbyRigidbody)
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
                            GenericGrab();
                        }
                        else if (grab.twoHanded)
                        {
                            grab.isTwoHandGrabbing = true;
                            StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                            grab.secondHandGrabbing = this;
                            GenericGrab();
                        }
                    }
                }
                else
                {
                    if (!grab.isGrabbing)
                    {
                        StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                        grab.handGrabbing = this;
                        GenericGrab();
                    }
                    else if (grab.twoHanded)
                    {
                        grab.isTwoHandGrabbing = true;
                        StartCoroutine(IgnoreCollisionInteractables(closestCollider, nearbyColliders));
                        grab.secondHandGrabbing = this;
                        GenericGrab();
                    }
                }
            }
            else
            {
                GrabClimbable();
            }
        }
        else if (!isGrabButtonPressed && isGrabbing)
        {
            UnGrab();
        }
    }
    void CheckForInteractable(Collider[] nearbyColliders)
    {
        closestCollider = null;
        nearbyRigidbody = null;
        if (grab)
        {
            bool isHovering = false;
            foreach (Collider nearbyCollider in nearbyColliders)
            {
                foreach (Collider collider in grab.colliders)
                {
                    if (collider == nearbyCollider)
                    {
                        isHovering = true;
                    }
                }
            }
            if (!isHovering)
            {
                grab.isHovering = false;
                grab = null;
            }
        }
        if (nearbyColliders.Length > 0)
        {
            closestCollider = FindClosestInteractable(nearbyColliders);
            nearbyRigidbody = closestCollider.attachedRigidbody ?? null;
            if (nearbyRigidbody)
            {
                grab = nearbyRigidbody.GetComponent<GrabTwoAttach>();
                grab.isHovering = true;
            }
            else
            {
                grab = closestCollider.GetComponent<GrabTwoAttach>() ?? closestCollider.transform.parent.GetComponent<GrabTwoAttach>() ?? closestCollider.transform.root.GetComponent<GrabTwoAttach>();
                grab.isHovering = true;
            }
        }
    }
    void FixedUpdate()
    {
        grabZonePosition = transform.position - (transform.rotation * grabZoneOffset);
        nearbyColliders = Physics.OverlapSphere(grabZonePosition, radius, grabLayer, QueryTriggerInteraction.Ignore);
        CheckForInteractable(nearbyColliders);

        if (nearbyColliders.Length > 0)
        {
            CheckGrabInput();
        }
        if (grab)
        {
            if (grab.isTwoHandGrabbing && grab is GrabSword && grab.secondHandGrabbing == this)
            {
                TwoHandedGrabbingSword();
            }
            if (grab.isHovering && grab is not GrabDynamic)
            {
                HoverIcon();
            }
            if (grab.isGrabbing && spawnedIcon)
            {
                Destroy(spawnedIcon);
            }
        }
        else if (spawnedIcon)
        {
            Destroy(spawnedIcon);
        }
    }
    void HoverIcon()
    {
        if(!spawnedIcon)
        {
            spawnedIcon = Instantiate(hoverIcon);
        }
        if(grab is GrabSword)
        {
            GrabSword grabSword = grab as GrabSword;
            if(handType == handTypeEnum.Left)
            {
                Vector3 position = Vector3.zero;
                switch (grabSword.handleDirection)
                {
                    case upDirection.forward:
                        position = grab.transform.TransformPoint(new Vector3(0, 0, grabSword.leftAttach.leftAttachPosition.z));
                        break;
                    case upDirection.up:
                        position = grab.transform.TransformPoint(new Vector3(0, grabSword.leftAttach.leftAttachPosition.y, 0));
                        break;
                    case upDirection.right:
                        position = grab.transform.TransformPoint(new Vector3(grabSword.leftAttach.leftAttachPosition.x, 0, 0));
                        break;
                }
                spawnedIcon.transform.position = position;
            }
            else
            {
                Vector3 position = Vector3.zero;
                switch (grabSword.handleDirection)
                {
                    case upDirection.forward:
                        position = grab.transform.TransformPoint(new Vector3(0, 0, grabSword.rightAttach.rightAttachPosition.z));
                        break;
                    case upDirection.up:
                        position = grab.transform.TransformPoint(new Vector3(0, grabSword.rightAttach.rightAttachPosition.y, 0));
                        break;
                    case upDirection.right:
                        position = grab.transform.TransformPoint(new Vector3(grabSword.rightAttach.rightAttachPosition.x, 0, 0));
                        break;
                }
                spawnedIcon.transform.position = position;
            }
        }
        else
            spawnedIcon.transform.position = grab.transform.position;

        spawnedIcon.transform.LookAt(GameObject.Find("CameraDriven").transform, Vector3.up);
    }
    IEnumerator WaitTillGrab()
    {
        //delay until you can regrab dynamic attaches
        canGrab = false;
        yield return new WaitForSeconds(poseSetup.poseTransitionDuration);
        canGrab = true;
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
