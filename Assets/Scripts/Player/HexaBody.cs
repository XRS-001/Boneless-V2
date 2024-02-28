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
using RootMotion.Demos;

public class HexaBody : MonoBehaviour
{
    [Header("XR Toolkit Parts")]
    public XROrigin XRRig;
    public GameObject XRCamera;
    public Transform headTarget;
    public Transform chest;
    public Transform hip;
    public Transform trackedOffset;
    public GrabPhysics[] grabbing;
    public VRIK finalSolver;
    public AudioClip footstepSound;
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
    public CheckColliding detectGrounded;
    public CheckColliding fenderCollision;
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
    [HideInInspector]
    public bool zipLining;

    public float crouchSpeed;
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
        finalSolver.solver.locomotion.onLeftFootstep.AddListener(PlayStepAudio);
        finalSolver.solver.locomotion.onRightFootstep.AddListener(PlayStepAudio);
        additionalHeight = (0.5f * Monoball.transform.lossyScale.y) + (0.5f * Fender.transform.lossyScale.y) + (Head.transform.position.y - Chest.transform.position.y);
        previousHeadPosition = CameraController.localPosition;
    }
    void PlayStepAudio()
    {
        AudioSource.PlayClipAtPoint(footstepSound, finalSolver.transform.position, 0.2f);
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
    
    public void Zipline()
    {
        if (!detectGrounded.collided)
        {
            Monoball.GetComponent<Rigidbody>().AddForce(Physics.gravity * 50);
            zipLining = true;
        }
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
            if (grab.isClimbing)
            {
                isClimbing = true;
            }
        }
        if (detectGrounded.collided)
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
                GrabTwoAttach grab = limb.colliderColliding.GetComponent<GrabTwoAttach>();
                if (!grab && limb.colliderColliding.transform.parent)
                {
                    grab = limb.colliderColliding.transform.parent.GetComponent<GrabTwoAttach>();

                    if (!grab && limb.colliderColliding.transform.parent.parent)
                        grab = limb.colliderColliding.transform.parent.parent.GetComponent<GrabTwoAttach>();
                }

                if(!grab && limb.colliderColliding.transform.root)
                    grab = limb.colliderColliding.transform.root.GetComponent<GrabTwoAttach>();

                if (grab)
                {
                    if (!grab.isGrabbing)
                        onSurface = true;
                }
                else
                    onSurface = true;
            }
        }
        foreach (GrabPhysics grab in grabbing)
        {
            if (grab.grabColliding)
                if (grab.grabColliding.collided)
                    onSurface = true;

        }
        if (onSurface)
        {
            if (!detectGrounded.collided)
            {
                foreach(TargetLimb limb in limbs)
                {
                    if (limb.isColliding)
                    {
                        if (fenderCollision.collided)
                        {
                            if(fenderCollision.colliderColliding == limb.colliderColliding && !vaulting && limb.transform.position.y > hip.transform.position.y)
                            {
                                StartCoroutine(Vault());
                            }
                        }
                    }
                }
            }
            if (!vaulting && !isClimbing && !zipLining)
            {
                float drag = Mathf.Clamp(1 / Monoball.GetComponent<Rigidbody>().velocity.magnitude, 10, float.PositiveInfinity);

                Head.GetComponent<Rigidbody>().drag = drag;
                Monoball.GetComponent<Rigidbody>().drag = drag;
                Spine.GetComponent<Rigidbody>().drag = drag;
                Fender.GetComponent<Rigidbody>().drag = drag;
            }
            else if (isClimbing)
            {
                Head.GetComponent<Rigidbody>().drag = 5;
                Monoball.GetComponent<Rigidbody>().drag = 5;
                Spine.GetComponent<Rigidbody>().drag = 5;
                Fender.GetComponent<Rigidbody>().drag = 5;
            }
            else
            {
                Head.GetComponent<Rigidbody>().drag = 0;
                Monoball.GetComponent<Rigidbody>().drag = 0;
                Spine.GetComponent<Rigidbody>().drag = 0;
                Fender.GetComponent<Rigidbody>().drag = 0;
            }
        }
        else if (detectGrounded.collided)
        {
            Head.GetComponent<Rigidbody>().drag = 5;
            Monoball.GetComponent<Rigidbody>().drag = 5;
            Spine.GetComponent<Rigidbody>().drag = 5;
            Fender.GetComponent<Rigidbody>().drag = 5;
        }
        else if (isClimbing)
        {
            Head.GetComponent<Rigidbody>().drag = 5;
            Monoball.GetComponent<Rigidbody>().drag = 5;
            Spine.GetComponent<Rigidbody>().drag = 5;
            Fender.GetComponent<Rigidbody>().drag = 5;
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
        yield return new WaitForSeconds(0.35f);

        Chest.GetComponent<Rigidbody>().useGravity = false;
        Monoball.GetComponent<Rigidbody>().useGravity = false;
        Fender.GetComponent<Rigidbody>().useGravity = false;
        Head.GetComponent<Rigidbody>().useGravity = false;

        Monoball.GetComponent<Rigidbody>().AddForce(Vector3.up * 2, ForceMode.VelocityChange);
        Chest.GetComponent<Rigidbody>().AddForce(Vector3.up * 2, ForceMode.VelocityChange);
        Head.GetComponent<Rigidbody>().AddForce(Vector3.up * 2, ForceMode.VelocityChange);
        Spine.GetComponent<Rigidbody>().AddForce(Vector3.up * 2, ForceMode.VelocityChange);

        yield return new WaitForSeconds(0.5f);

        Monoball.GetComponent<Rigidbody>().AddForce(chest.transform.forward / 1, ForceMode.VelocityChange);
        Chest.GetComponent<Rigidbody>().AddForce(chest.transform.forward / 1, ForceMode.VelocityChange);
        Head.GetComponent<Rigidbody>().AddForce(chest.transform.forward / 1f, ForceMode.VelocityChange);
        Spine.GetComponent<Rigidbody>().AddForce(chest.transform.forward / 1f, ForceMode.VelocityChange);

        Chest.GetComponent<Rigidbody>().useGravity = true;
        Monoball.GetComponent<Rigidbody>().useGravity = true;
        Fender.GetComponent<Rigidbody>().useGravity = true;
        Head.GetComponent<Rigidbody>().useGravity = true;

        yield return new WaitForSeconds(1f);
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

        headYaw = Quaternion.Euler(0, XRCamera.transform.eulerAngles.y, 0);
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
        headTarget.transform.position = Head.transform.position;
        headTarget.transform.rotation = XRCamera.transform.rotation;
    }
    private void RotatePlayer()
    {
        Chest.transform.rotation = Quaternion.Euler(0, chest.eulerAngles.y, 0);
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
        if (CrouchTarget.y >= 0.1f)
        {
            CrouchTarget.y -= crouchSpeed * Time.fixedDeltaTime;
            Spine.targetPosition = new Vector3(0, CrouchTarget.y, 0);
            trackedOffset.transform.localPosition = Vector3.Lerp(trackedOffset.transform.localPosition, new Vector3(0, 0 - 2.5f, 0), 0.005f);
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
        while (timer < 0.25f)
        {
            trackedOffset.transform.localPosition = Vector3.Lerp(trackedOffset.transform.localPosition, Vector3.zero, timer / 0.25f);
            timer += Time.deltaTime;
            yield return null;
        }

        if (Monoball.GetComponent<Rigidbody>().angularVelocity.magnitude > 0.5f)
        {
            Monoball.GetComponent<Rigidbody>().AddForce(chest.transform.forward / 2f, ForceMode.VelocityChange);
            Chest.GetComponent<Rigidbody>().AddForce(chest.transform.forward / 2f, ForceMode.VelocityChange);
            Fender.GetComponent<Rigidbody>().AddForce(chest.transform.forward / 2f, ForceMode.VelocityChange);
            Head.GetComponent<Rigidbody>().AddForce(chest.transform.forward / 2f, ForceMode.VelocityChange);
        }
    }

    //------Joint Controll-----------------------------------------------------------------------------------
    private void SpineContractionOnRealWorldCrouch()
    {
        CrouchTarget.y = Mathf.Clamp(CameraController.transform.localPosition.y - additionalHeight, -0.075f, highestCrouch - additionalHeight);
        Spine.targetPosition = new Vector3(0, CrouchTarget.y, 0);
    }
}
