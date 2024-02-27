using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static EnumDeclaration;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using static GrabPhysics;

public class GrabPhysics : MonoBehaviour
{
    [Header("Hand Info")]
    public InputActionProperty grabInputSource;
    public handTypeEnum handType;
    public Transform controller;
    public Rigidbody rb;
    [System.Serializable]
    public class ArmJoint
    {
        public Collider[] colliders;
        public Rigidbody rb;
        public ConfigurableJoint joint;
        [HideInInspector]
        public JointDrive startDrive;
    }
    public ArmJoint[] armJoints;
    public HexaBody hexaBody;

    [Header("Grabbing")]
    public float radius = 0.1f;
    [Tooltip("The local offset for the grab zone of the hand")]
    public Vector3 grabZoneOffset;
    [HideInInspector]
    public Vector3 grabZonePosition;
    public LayerMask grabLayer;
    public SetPose poseSetup { get; private set; }
    public AudioClip grabSound;

    [Header("Distance Grabbing")]
    [Tooltip("A small icon to show on the interactable when hovering")]
    public GameObject hoverIcon;
    private GameObject spawnedIcon;
    public LayerMask distanceGrabLayer;
    public Transform gazeTarget;
    private bool distanceHovering;

    private Joint joint;
    [HideInInspector]
    public GrabTwoAttach grab;
    [HideInInspector]
    public bool canGrab = true;

    [Header("Grabbed Data")]
    public bool isGrabbing = false;
    [HideInInspector]
    public bool isClimbing = false;
    public float connectedMass { get; private set; }

    Collider[] nearbyColliders;
    Collider closestCollider;
    Rigidbody nearbyRigidbody;

    [HideInInspector]
    public CheckColliding grabColliding;
    private void Start()
    {
        foreach (ArmJoint joint in armJoints)
        {
            joint.startDrive = joint.joint.angularXDrive;
        }
        poseSetup = GetComponent<SetPose>();
    }
    void HandleDrive(bool climbing)
    {
        if(!climbing)
        {
            if (connectedMass > 1)
            {
                if(grab.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
                    foreach (ArmJoint joint in armJoints)
                    {
                        JointDrive newDrive = joint.startDrive;
                        newDrive.positionDamper /= connectedMass / 6;
                        newDrive.positionSpring /= connectedMass / 3;

                        joint.joint.angularXDrive = newDrive;
                        joint.joint.angularYZDrive = newDrive;
                    }
                else
                    foreach (ArmJoint joint in armJoints)
                    {
                        JointDrive newDrive = joint.startDrive;
                        newDrive.positionDamper /= connectedMass / 4;
                        newDrive.positionSpring /= connectedMass / 2;

                        joint.joint.angularXDrive = newDrive;
                        joint.joint.angularYZDrive = newDrive;
                    }
            }
            else
            {
                foreach (ArmJoint joint in armJoints)
                {
                    joint.joint.angularXDrive = joint.startDrive;
                    joint.joint.angularYZDrive = joint.startDrive;
                }
            }
        }
        else
        {
            foreach (ArmJoint joint in armJoints)
            {
                JointDrive newDrive = joint.startDrive;

                newDrive.positionDamper *= 2;

                joint.joint.angularXDrive = newDrive;
                joint.joint.angularYZDrive = newDrive;
            }
        }
    }
    void GenericGrab()
    {
        if(distanceHovering && grab.GetComponent<Blade>())
            if (grab.GetComponent<Blade>().stabbed)
                grab.GetComponent<Blade>().UnStab();

        StartCoroutine(DelayGrab());
        foreach (Collider collider in grab.colliders)
        {
            foreach (ArmJoint armJoint in armJoints)
            {
                foreach (Collider armCollider in armJoint.colliders)
                {
                    Physics.IgnoreCollision(collider, armCollider, true);
                }
            }
        }

        AudioSource.PlayClipAtPoint(grabSound, closestCollider.ClosestPoint(transform.position));

        if (grab is GrabDynamic)
        {
            poseSetup.setDynamicPose = true;
        }

        grab.SetPose(handType);
        poseSetup.pose = grab.pose;
        poseSetup.SetupPose();

        grab.SetAttachPoint(handType);

        isGrabbing = true;
        if (!grab.isGrabbing)
        {
            connectedMass = nearbyRigidbody.mass;
            grabColliding = grab.AddComponent<CheckColliding>();
        }
        else
        {
            connectedMass = grab.handGrabbing.connectedMass;
            grabColliding = grab.GetComponent<CheckColliding>();
        }

        HandleDrive(false);
        grab.isGrabbing = true;

        nearbyRigidbody.mass = 1;
        joint = gameObject.AddComponent<ConfigurableJoint>();
        ConfigurableJoint configJoint = joint as ConfigurableJoint;

        if (grab is not GrabDynamic)
        {
            transform.rotation = (nearbyRigidbody.rotation * Quaternion.Euler(grab.attachRotation));
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

        configJoint.autoConfigureConnectedAnchor = false;
        configJoint.connectedAnchor = grab.attachPoint;
        configJoint.connectedBody = nearbyRigidbody;
        canGrab = false;
    }
    IEnumerator DelayGrab()
    {
        if(distanceHovering)
        {
            foreach(Collider collider in grab.colliders)
            {
                collider.enabled = false;
            }
            yield return new WaitForSeconds(0.025f);
            foreach (Collider collider in grab.colliders)
            {
                collider.enabled = true;
            }
        }
        if (grab.gameObject.layer != LayerMask.NameToLayer("Ragdoll"))
        {
            foreach (ArmJoint joint in armJoints)
            {
                joint.rb.isKinematic = true;
            }
            yield return new WaitForSeconds(0.001f);

            foreach (ArmJoint joint in armJoints)
            {
                joint.rb.isKinematic = false;
            }
        }
        if(grab.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
        {
            grab.transform.root.GetComponent<NPC>().isGrabbing = true;
            hexaBody.Monoball.GetComponent<Rigidbody>().isKinematic = true;
            hexaBody.Chest.GetComponent<Rigidbody>().isKinematic = true;
            hexaBody.Fender.GetComponent<Rigidbody>().isKinematic = true;
            hexaBody.Head.GetComponent<Rigidbody>().isKinematic = true;

            foreach (ArmJoint joint in armJoints)
            {
                foreach(Collider collider in joint.colliders)
                {
                    foreach(Collider npcCollider in grab.transform.root.GetComponent<NPC>().colliders)
                    {
                        Physics.IgnoreCollision(collider, npcCollider, true);
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);

            hexaBody.Monoball.GetComponent<Rigidbody>().isKinematic = false;
            hexaBody.Chest.GetComponent<Rigidbody>().isKinematic = false;
            hexaBody.Fender.GetComponent<Rigidbody>().isKinematic = false;
            hexaBody.Head.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
    public void GrabClimbable()
    {
        isClimbing = true;
        foreach (Collider collider in grab.colliders)
        {
            foreach (ArmJoint armJoint in armJoints)
            {
                foreach (Collider armCollider in armJoint.colliders)
                {
                    Physics.IgnoreCollision(collider, armCollider, true);
                }
            }
        }
        AudioSource.PlayClipAtPoint(grabSound, closestCollider.ClosestPoint(transform.position));

        if (!grab.isGrabbing)
        {
            grab.handGrabbing = this;
        }
        else
        {
            grab.secondHandGrabbing = this;
        }

        grab.SetAttachPoint(handType);
        HandleDrive(true);

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
        StartCoroutine(DelayExit(grab));
        isGrabbing = false;
        isClimbing = false;
        if (grab != null)
        {
            poseSetup.exitingDynamicPose = true;
            poseSetup.UnSetPose();
            if (!grab.isTwoHandGrabbing)
            {
                if(grab.transform.root.GetComponent<NPC>())
                    grab.transform.root.GetComponent<NPC>().isGrabbing = false;

                if (nearbyRigidbody)
                {
                    nearbyRigidbody.mass = connectedMass;
                    Destroy(grabColliding);
                }
                if (joint)
                {
                    Destroy(joint);
                }
                grab.handGrabbing = null;
                grab.isGrabbing = false;
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

                grab.isTwoHandGrabbing = false;
                grab.secondHandGrabbing = null;
            }
            connectedMass = 0;
            HandleDrive(false);
        }
        if (grab is GrabDynamic)
            StartCoroutine(WaitTillGrab());
        else
            canGrab = true;
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
    void CheckForInteractable(List<Collider> colliders)
    {
        if (colliders.Count <= 0)
        {
            if (grab)
            {
                grab.isHovering = false;
                grab = null;
            }
            colliders = Physics.OverlapCapsule(grabZonePosition, grabZonePosition + Vector3.Lerp(gazeTarget.forward, transform.forward + transform.up, 0.5f).normalized * 2, 0.125f, distanceGrabLayer).ToList();

            if (colliders.Count > 0)
                distanceHovering = true;
            else
                distanceHovering = false;
        }
        else
        {
            distanceHovering = false;
        }
        closestCollider = null;
        nearbyRigidbody = null;
        if (colliders.Count > 0)
        {
            closestCollider = FindClosestInteractable(colliders.ToArray());
            nearbyRigidbody = closestCollider.attachedRigidbody ?? null;
            if (nearbyRigidbody)
            {
                grab = nearbyRigidbody.GetComponent<GrabTwoAttach>();
                if (grab)
                    if (grab is GrabDynamic)
                    {
                        if (!distanceHovering)
                            grab.isHovering = true;
                    }
                    else
                        grab.isHovering = true;
            }
            else
            {
                grab = closestCollider.GetComponent<GrabTwoAttach>() ?? closestCollider.transform.parent.GetComponent<GrabTwoAttach>() ?? closestCollider.transform.root.GetComponent<GrabTwoAttach>() ?? closestCollider.transform.parent.parent.GetComponent<GrabTwoAttach>();
                if (grab)
                    if(grab is GrabDynamic)
                    {
                        if (!distanceHovering)
                            grab.isHovering = true;
                    }
                    else
                        grab.isHovering = true;
            }
            if(grab is GrabDynamic && distanceHovering)
            {
                grab = null;
            }
        }
    }
    void FixedUpdate()
    {
        grabZonePosition = transform.position - (transform.rotation * grabZoneOffset);
        nearbyColliders = Physics.OverlapSphere(grabZonePosition, radius, grabLayer);
        if (!isGrabbing)
        {
            CheckForInteractable(nearbyColliders.ToList());
        }

        CheckGrabInput();
        if (grab)
        {
            if (grab.isHovering && grab is not GrabDynamic)
            {
                HoverIcon();
            }
            if (grab.isGrabbing && spawnedIcon)
            {
                Destroy(spawnedIcon);
            }
            if (nearbyRigidbody && isGrabbing)
            {
                nearbyRigidbody.AddForce(Vector3.down * connectedMass);
            }
        }
        else if (spawnedIcon)
        {
            Destroy(spawnedIcon);
        }
    }
    void HoverIcon()
    {
        if (!spawnedIcon)
        {
            spawnedIcon = Instantiate(hoverIcon);
        }
        if (grab is GrabSword)
        {
            GrabSword grabSword = grab as GrabSword;
            if (handType == handTypeEnum.Left)
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
    IEnumerator DelayExit(GrabTwoAttach oldGrab)
    {
        yield return new WaitForSeconds(0.5f);

        if (!isGrabbing)
        {
            if(oldGrab.gameObject.layer != LayerMask.NameToLayer("Ragdoll"))
            {
                foreach (Collider collider in oldGrab.colliders)
                {
                    foreach (ArmJoint armJoint in armJoints)
                    {
                        foreach (Collider armCollider in armJoint.colliders)
                        {
                            Physics.IgnoreCollision(collider, armCollider, false);
                        }
                    }
                }
            }
            else
            {
                foreach (ArmJoint joint in armJoints)
                {
                    foreach (Collider collider in joint.colliders)
                    {
                        foreach (Collider npcCollider in oldGrab.transform.root.GetComponent<NPC>().colliders)
                        {
                            Physics.IgnoreCollision(collider, npcCollider, false);
                        }
                    }
                }
            }
        }
        else if (oldGrab != grab)
        {
            if (oldGrab.gameObject.layer != LayerMask.NameToLayer("Ragdoll"))
            {
                foreach (Collider collider in oldGrab.colliders)
                {
                    foreach (ArmJoint armJoint in armJoints)
                    {
                        foreach (Collider armCollider in armJoint.colliders)
                        {
                            Physics.IgnoreCollision(collider, armCollider, false);
                        }
                    }
                }
            }
            else
            {
                foreach (ArmJoint joint in armJoints)
                {
                    foreach (Collider collider in joint.colliders)
                    {
                        foreach (Collider npcCollider in oldGrab.transform.root.GetComponent<NPC>().colliders)
                        {
                            Physics.IgnoreCollision(collider, npcCollider, false);
                        }
                    }
                }
            }
        }
    }
    IEnumerator WaitTillGrab()
    {
        //delay until you can regrab dynamic attaches
        yield return new WaitForSeconds(poseSetup.poseTransitionDuration);
        canGrab = true;
    }
    IEnumerator IgnoreCollisionInteractables(Collider collider, Collider[] collidersToIgnore)
    {
        foreach (Collider colliderToIgnore in collidersToIgnore)
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

        Gizmos.color = new Color(1, 0, 0, 1f);

        Gizmos.DrawRay(new Ray(grabZonePosition, Vector3.Lerp(gazeTarget.forward, transform.forward / 2 + transform.up, 0.5f)));
    }
}