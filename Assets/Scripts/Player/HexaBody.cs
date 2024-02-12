using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using RootMotion.FinalIK;
using Unity.VisualScripting;
using UnityEngine.Timeline;
using System.Collections;
using RootMotion;
using static RootMotion.Demos.FBBIKSettings;

public class HexaBody : MonoBehaviour
{
    [Header("XR Toolkit Parts")]
    public XROrigin XRRig;
    public GameObject XRCamera;
    public Transform head;
    public Transform chest;
    public Transform hip;
    public Transform trackedOffset;
    public GrabPhysics[] grabbing;
    public VRIK finalSolver;
    [Header("Actionbased Controller")]
    public Transform CameraController;
    public ActionBasedController RightHandController;
    public Transform trackedSolverRightTarget;
    public ActionBasedController LeftHandController;
    public Transform trackedSolverLeftTarget;

    public InputActionReference LeftTrackPadClicked;
    public InputActionReference LeftTrackPadTouch;

    public InputActionReference RightTrackPadTouched;

    public InputActionReference RightTrackPad;
    public InputActionReference LeftTrackPad;

    [Header("Hexabody Parts")]
    public GameObject Head;
    public GameObject Chest;
    public GameObject Fender;
    public GameObject Monoball;

    public ConfigurableJoint Spine;
    public DetectLocoSphereGrounded detectGrounded;
    public TargetLimb[] limbs;

    [Header("Hexabody Movespeed")]
    public float moveForceCrouch;
    public float moveForceWalk;
    public float moveForceSprint;

    [Header("Hexabody Drag")]
    public float angularDragOnMove;
    public float angularBreakDrag;

    [Header("Hexabody Crouch & Jump")]
    bool jumping = false;
    bool vaulting = false;

    public float crouchSpeed;
    public float lowestCrouch;
    public float highestCrouch;
    private float additionalHeight;

    Vector3 CrouchTarget;

    //---------Input Values---------------------------------------------------------------------------------------------------------------//

    private Quaternion headYaw;
    private Vector3 moveDirection;
    private Vector3 monoballTorque;

    private Vector3 previousHeadPosition;
    private Vector3 currentHeadVelocity;

    private Vector2 LeftTrackPadVector;
    private Vector2 RightTrackPadVector;

    private float leftTrackPadPressed;
    private float rightTrackPadTouched;

    private float leftTrackPadTouched;

    void Start()
    {
        additionalHeight = (0.5f * Monoball.transform.lossyScale.y) + (0.5f * Fender.transform.lossyScale.y) + (Head.transform.position.y - Chest.transform.position.y);
        previousHeadPosition = CameraController.localPosition;
    }
    void Update()
    {
        CameraToPlayer();
        XRRigToPlayer();
        GetContollerInputValues();
    }

    private void FixedUpdate() 
    {
        SetHandTargets();
        TransformSpineCollider();
        MovePlayerViaController();
        Jump();

        if (!jumping)
        {
            SpineContractionOnRealWorldCrouch();
        }

        RotatePlayer();
        RoomScaleMove();
        Climbing();
    } 
    void SetHandTargets()
    {
        trackedSolverLeftTarget.transform.position = LeftHandController.transform.position;
        trackedSolverLeftTarget.transform.rotation = LeftHandController.transform.rotation;

        trackedSolverRightTarget.transform.position = RightHandController.transform.position;
        trackedSolverRightTarget.transform.rotation = RightHandController.transform.rotation * Quaternion.Euler(0, 0, 180);
    }
    void TransformSpineCollider()
    {
        Chest.GetComponent<CapsuleCollider>().center = new Vector3(Chest.transform.InverseTransformPoint(chest.position).x, Chest.GetComponent<CapsuleCollider>().center.y, Chest.transform.InverseTransformPoint(chest.position).z);
        Chest.GetComponent<CapsuleCollider>().height = Vector3.Distance(new Vector3(0, Head.transform.position.y, 0), new Vector3(0, Fender.transform.position.y, 0)) * 3f;
    }
    void Climbing()
    {
        bool isClimbing = false;
        foreach (GrabPhysics grab in grabbing)
        {
            if (grab.isGrabbing)
            {
                isClimbing = true;
            }
        }
        if (detectGrounded.isGrounded)
        {
            isClimbing = false;
            finalSolver.solver.locomotion.weight = Mathf.Lerp(finalSolver.solver.locomotion.weight, 1, 0.1f);
        }
        else
        {
            finalSolver.solver.locomotion.weight = Mathf.Lerp(finalSolver.solver.locomotion.weight, 0.25f, 0.1f);
        }
        bool onSurface = false;
        foreach (TargetLimb limb in limbs)
        {
            if (limb.isColliding)
            {
                onSurface = true;
            }
        }
        if (onSurface)
        {
            if (!detectGrounded.isGrounded)
            {
                foreach(TargetLimb limb in limbs)
                {
                    if (limb.isColliding)
                    {
                        limb.colliderColliding.Raycast(new Ray(new Vector3(Chest.transform.position.x, Head.transform.position.y, Chest.transform.position.z) + Chest.transform.forward / 32, Vector3.down), out RaycastHit hit, float.PositiveInfinity);
                        if (hit.collider && hit.point.y > hip.position.y)
                        {
                            StartCoroutine(Vault());
                        }
                    }
                }
            }
            if (!vaulting && !isClimbing)
            {
                float drag = Mathf.Clamp(1 / Monoball.GetComponent<Rigidbody>().velocity.magnitude, 0, float.PositiveInfinity);

                Head.GetComponent<Rigidbody>().drag = drag;
                Monoball.GetComponent<Rigidbody>().drag = drag;
                Spine.GetComponent<Rigidbody>().drag = drag;
                Fender.GetComponent<Rigidbody>().drag = drag;
            }
            else if (isClimbing)
            {
                Head.GetComponent<Rigidbody>().drag = 25;
                Monoball.GetComponent<Rigidbody>().drag = 25;
                Spine.GetComponent<Rigidbody>().drag = 25;
                Fender.GetComponent<Rigidbody>().drag = 25;
            }
            else
            {
                Head.GetComponent<Rigidbody>().drag = 0;
                Monoball.GetComponent<Rigidbody>().drag = 0;
                Spine.GetComponent<Rigidbody>().drag = 0;
                Fender.GetComponent<Rigidbody>().drag = 0;
            }
        }
        else if (detectGrounded.isGrounded)
        {
            Head.GetComponent<Rigidbody>().drag = 5;
            Monoball.GetComponent<Rigidbody>().drag = 5;
            Spine.GetComponent<Rigidbody>().drag = 5;
            Fender.GetComponent<Rigidbody>().drag = 5;
        }
        else if (isClimbing)
        {
            Head.GetComponent<Rigidbody>().drag = 25;
            Monoball.GetComponent<Rigidbody>().drag = 25;
            Spine.GetComponent<Rigidbody>().drag = 25;
            Fender.GetComponent<Rigidbody>().drag = 25;
        }
        else
        {
            Head.GetComponent<Rigidbody>().drag = 0;
            Monoball.GetComponent<Rigidbody>().drag = 0;
            Spine.GetComponent<Rigidbody>().drag = 0;
            Fender.GetComponent<Rigidbody>().drag = 0;
        }
    }
    IEnumerator Vault()
    {
        vaulting = true;
        Monoball.GetComponent<Collider>().enabled = false;
        Fender.GetComponent<Collider>().enabled = false;
        Chest.GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(0.25f);

        Chest.GetComponent<Rigidbody>().useGravity = false;
        Monoball.GetComponent<Rigidbody>().useGravity = false;
        Fender.GetComponent<Rigidbody>().useGravity = false;
        Head.GetComponent<Rigidbody>().useGravity = false;

        Chest.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(Chest.transform.position, Chest.transform.position + (Vector3.up / 7.5f + Chest.transform.forward / 70), 0.35f));
        Monoball.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(Monoball.transform.position, Monoball.transform.position + (Vector3.up / 7.5f + Chest.transform.forward / 70), 0.35f));
        Fender.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(Fender.transform.position, Fender.transform.position + (Vector3.up / 7.5f + Chest.transform.forward / 70), 0.35f));
        Head.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(Head.transform.position, Head.transform.position + (Vector3.up / 7.5f + Chest.transform.forward / 70), 0.35f));

        yield return new WaitForSeconds(0.35f);

        Chest.GetComponent<Rigidbody>().useGravity = true;
        Monoball.GetComponent<Rigidbody>().useGravity = true;
        Fender.GetComponent<Rigidbody>().useGravity = true;
        Head.GetComponent<Rigidbody>().useGravity = true;

        Monoball.GetComponent<Collider>().enabled = true;
        Fender.GetComponent<Collider>().enabled = true;
        Chest.GetComponent<Collider>().enabled = true;

        vaulting = false;
    }
    void RoomScaleMove()
    {
        currentHeadVelocity = (CameraController.localPosition - previousHeadPosition) / Time.deltaTime;
        previousHeadPosition = CameraController.localPosition;
        Vector3 roomscaleMove = new Vector3(currentHeadVelocity.x, 0, currentHeadVelocity.z) * Time.deltaTime;

        Monoball.GetComponent<Rigidbody>().MovePosition(Monoball.transform.position + roomscaleMove);
        Fender.GetComponent<Rigidbody>().MovePosition(Fender.transform.position + roomscaleMove);
        Chest.GetComponent<Rigidbody>().MovePosition(Chest.transform.position + roomscaleMove);
        Head.GetComponent<Rigidbody>().MovePosition(Head.transform.position + roomscaleMove);
    }
    private void GetContollerInputValues()
    {
        //Trackpad
        LeftTrackPadVector = LeftTrackPad.action.ReadValue<Vector2>();
        leftTrackPadPressed = LeftTrackPadClicked.action.ReadValue<float>();
        leftTrackPadTouched = LeftTrackPadTouch.action.ReadValue<float>();

        //Trackpad
        RightTrackPadVector = RightTrackPad.action.ReadValue<Vector2>();
        rightTrackPadTouched = RightTrackPadTouched.action.ReadValue<float>();

        headYaw = Quaternion.Euler(0, XRRig.Camera.transform.eulerAngles.y, 0);
        moveDirection = headYaw * new Vector3(LeftTrackPadVector.x, 0, LeftTrackPadVector.y);
        monoballTorque = new Vector3(moveDirection.z, 0, -moveDirection.x);
    }

    //------Transforms---------------------------------------------------------------------------------------
    private void CameraToPlayer()
    {
        XRCamera.transform.position = Head.transform.position;
    }
    private void XRRigToPlayer()
    {
        XRRig.transform.position = new Vector3(Fender.transform.position.x - CameraController.localPosition.x, Fender.transform.position.y - (0.5f * Fender.transform.localScale.y + 0.5f * Monoball.transform.localScale.y), Fender.transform.position.z - CameraController.localPosition.z);
        head.transform.position = Head.transform.position;
        head.transform.rotation = XRCamera.transform.rotation;
    }
    private void RotatePlayer()
    {
        Chest.transform.rotation = headYaw;
    }
    //-----HexaBody Movement---------------------------------------------------------------------------------
    private void MovePlayerViaController()
    {
        if (!jumping)
        {
            if (leftTrackPadTouched == 0)
            {
                StopMonoball();
            }

            else if (leftTrackPadPressed == 0 && leftTrackPadTouched == 1)
            {
                MoveMonoball(moveForceWalk);
            }

            else if (leftTrackPadPressed == 1)
            {
                MoveMonoball(moveForceSprint);
            }
        }

        else if (jumping)
        {
            if (leftTrackPadTouched == 0)
            {
                StopMonoball();
            }

            else if (leftTrackPadTouched == 1)
            {
                MoveMonoball(moveForceCrouch);
            }
        }

    }
    private void MoveMonoball(float force)
    {
        Monoball.GetComponent<Rigidbody>().freezeRotation = false;
        Monoball.GetComponent<Rigidbody>().angularDrag = angularDragOnMove;
        Monoball.GetComponent<Rigidbody>().AddTorque(monoballTorque.normalized * force, ForceMode.Force);
    }
    private void StopMonoball()
    {
       Monoball.GetComponent<Rigidbody>().freezeRotation = true;
       Monoball.GetComponent<Rigidbody>().angularDrag = angularBreakDrag;
    }

    //------Jumping------------------------------------------------------------------------------------------
    private void Jump()
    {
        if (rightTrackPadTouched == 1 && RightTrackPadVector.y < 0)
        {
            jumping = true;
            JumpSitDown();
        }

        else if ((rightTrackPadTouched == 0) && jumping == true)
        {
            jumping = false;
            JumpSitUp();
        }
    }
    private void JumpSitDown()
    {
        if (CrouchTarget.y >= lowestCrouch)
        {
            CrouchTarget.y -= crouchSpeed * Time.fixedDeltaTime;
            Spine.targetPosition = new Vector3(0, CrouchTarget.y, 0);
            trackedOffset.transform.localPosition = Vector3.Lerp(trackedOffset.transform.localPosition, new Vector3(0, 0 - Spine.transform.localPosition.y - 0.2f, 0), 0.05f);
        }
    }
    private void JumpSitUp()
    {
        CrouchTarget = new Vector3(0, highestCrouch - additionalHeight, 0);
        Spine.targetPosition = CrouchTarget;
        StartCoroutine(SitUpRoutine());
    }
    IEnumerator SitUpRoutine()
    {
        float timer = 0;
        while (timer < 0.5f)
        {
            trackedOffset.transform.localPosition = Vector3.Lerp(trackedOffset.transform.localPosition, Vector3.zero, timer / 0.4f);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    //------Joint Controll-----------------------------------------------------------------------------------
    private void SpineContractionOnRealWorldCrouch()
    {
        CrouchTarget.y = Mathf.Clamp(CameraController.transform.localPosition.y - additionalHeight, lowestCrouch, highestCrouch - additionalHeight);
        Spine.targetPosition = new Vector3(0, CrouchTarget.y, 0);
    }
}
