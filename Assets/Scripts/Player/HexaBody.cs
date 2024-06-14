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
using Valve.VR;
using static EnumDeclaration;
public class HexaBody : MonoBehaviour
{
    [Header("XR Toolkit Parts")]
    public XROrigin XRRig;
    public GameObject XRCamera;
    public Transform headTarget;
    public Transform IKChest;
    public Transform hip;
    public Transform trackedOffset;
    public GrabPhysics[] grabbing;
    public VRIK finalSolver;
    public AudioClip footstepSound;
    public AudioClip jumpSound;
    [Header("Virtual Turning")]
    public turnType turnType;
    public float snapTurnDegree = 60;
    public float smoothTurnSpeed = 90;
    private bool canSnapTurn = true;
    [Header("Actionbased Controller")]
    public Transform CameraController;
    public ActionBasedController RightHandController;
    public Transform trackedSolverRightTarget;
    public ActionBasedController LeftHandController;
    public Transform trackedSolverLeftTarget;

    public InputActionReference RightTrackPad;
    public InputActionReference LeftTrackPad;
    public InputActionReference LeftTrackPadPressed;
    public InputActionReference LeftTrackPadTouch;
    public InputActionReference jump;

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

    private bool leftTrackPadTouched;
    private bool leftTrackPadPressed;
    public bool jumpPressed;

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
        VirtualTurn();
    }
    void VirtualTurn()
    {
        switch(turnType)
        {
            case turnType.smooth:
                if (RightTrackPadVector.x < 0)
                {
                    RightHandController.transform.parent.RotateAround(Fender.transform.position, Vector3.up, Time.deltaTime * -smoothTurnSpeed);
                    LeftHandController.transform.parent.RotateAround(Fender.transform.position, Vector3.up, Time.deltaTime * -smoothTurnSpeed);
                    XRCamera.transform.parent.RotateAround(Fender.transform.position, Vector3.up, Time.deltaTime * -smoothTurnSpeed);
                }
                else if (RightTrackPadVector.x > 0)
                {
                    RightHandController.transform.parent.RotateAround(Fender.transform.position, Vector3.up, Time.deltaTime * smoothTurnSpeed);
                    LeftHandController.transform.parent.RotateAround(Fender.transform.position, Vector3.up, Time.deltaTime * smoothTurnSpeed);
                    XRCamera.transform.parent.RotateAround(Fender.transform.position, Vector3.up, Time.deltaTime * smoothTurnSpeed);
                }
                break;

            case turnType.snap:
                if (RightTrackPadVector.x < 0 && canSnapTurn)
                {
                    RightHandController.transform.parent.rotation = Quaternion.Euler(new Vector3(0, RightHandController.transform.parent.eulerAngles.y - snapTurnDegree, 0));
                    LeftHandController.transform.parent.rotation = Quaternion.Euler(new Vector3(0, LeftHandController.transform.parent.eulerAngles.y - snapTurnDegree, 0));
                    XRCamera.transform.parent.rotation = Quaternion.Euler(new Vector3(0, XRCamera.transform.parent.eulerAngles.y - snapTurnDegree, 0));
                    canSnapTurn = false;
                    Invoke(nameof(DelayCanSnapTurn), 0.3f);
                }
                else if (RightTrackPadVector.x > 0 && canSnapTurn)
                {
                    RightHandController.transform.parent.rotation = Quaternion.Euler(new Vector3(0, RightHandController.transform.parent.eulerAngles.y + snapTurnDegree, 0));
                    LeftHandController.transform.parent.rotation = Quaternion.Euler(new Vector3(0, LeftHandController.transform.parent.eulerAngles.y + snapTurnDegree, 0));
                    XRCamera.transform.parent.rotation = Quaternion.Euler(new Vector3(0, XRCamera.transform.parent.eulerAngles.y + snapTurnDegree, 0));
                    canSnapTurn = false;
                    Invoke(nameof(DelayCanSnapTurn), 0.3f);
                }
                break;
        }
    }
    void DelayCanSnapTurn()
    {
        canSnapTurn = true;
    }
    private void LateUpdate()
    {
        RotatePlayer();
    }
    public void Zipline()
    {
        if (!detectGrounded.collided)
        {
            Monoball.GetComponent<Rigidbody>().AddForce(Physics.gravity * 250);
            zipLining = true;
        }
    }
    void SetHandTargets()
    {
        trackedSolverLeftTarget.transform.position = LeftHandController.transform.position - XRCamera.transform.parent.localRotation * new Vector3(CameraController.localPosition.x, 0, CameraController.localPosition.z);
        trackedSolverLeftTarget.transform.rotation = LeftHandController.transform.rotation;

        trackedSolverRightTarget.transform.position = RightHandController.transform.position - XRCamera.transform.parent.localRotation * new Vector3(CameraController.localPosition.x, 0, CameraController.localPosition.z);
        trackedSolverRightTarget.transform.rotation = RightHandController.transform.rotation * Quaternion.Euler(0, 0, 180);
    }
    void TransformSpineCollider()
    {
        Chest.GetComponent<CapsuleCollider>().center = new Vector3(Chest.transform.InverseTransformPoint(IKChest.position).x, Chest.GetComponent<CapsuleCollider>().center.y, Chest.transform.InverseTransformPoint(IKChest.position).z - 0.1f);
        Chest.GetComponent<CapsuleCollider>().height = Vector3.Distance(new Vector3(0, Head.transform.position.y, 0), new Vector3(0, Fender.transform.position.y, 0)) * 2.5f;
    }
    void Climbing()
    {
        if (detectGrounded.collided)
        {
            finalSolver.solver.locomotion.weight = Mathf.Lerp(finalSolver.solver.locomotion.weight, 1, 0.1f);
        }
        else
        {
            finalSolver.solver.locomotion.weight = Mathf.Lerp(finalSolver.solver.locomotion.weight, 0.25f, 0.1f);
        }
        bool onSurface = false;
        foreach (TargetLimb limb in limbs)
        {
            if (limb.isColliding && limb.colliderColliding)
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
                            if(!vaulting && limb.transform.position.y > hip.transform.position.y)
                            {
                                StartCoroutine(Vault());
                            }
                        }
                    }
                }
            }
        }
        bool climbing = false;
        foreach (GrabPhysics grab in grabbing)
        {
            if (grab.isClimbing)
            {
                climbing = true;
            }
        }
        if (detectGrounded.collided && !climbing)
        {
            Chest.GetComponent<Rigidbody>().drag = 3;
            Monoball.GetComponent<Rigidbody>().drag = 3;
            Fender.GetComponent<Rigidbody>().drag = 3;
            Head.GetComponent<Rigidbody>().drag = 3;
        }
        else
        {
            Chest.GetComponent<Rigidbody>().drag = 1;
            Monoball.GetComponent<Rigidbody>().drag = 1;
            Fender.GetComponent<Rigidbody>().drag = 1;
            Head.GetComponent<Rigidbody>().drag = 1;
        }
    }
    IEnumerator Vault()
    {
        vaulting = true;
        yield return new WaitForSeconds(0.35f);

        AudioSource.PlayClipAtPoint(jumpSound, finalSolver.transform.position, 0.2f);
        Chest.GetComponent<Rigidbody>().useGravity = false;
        Monoball.GetComponent<Rigidbody>().useGravity = false;
        Fender.GetComponent<Rigidbody>().useGravity = false;
        Head.GetComponent<Rigidbody>().useGravity = false;

        Monoball.GetComponent<Rigidbody>().AddForce(Vector3.up * 2.5f, ForceMode.VelocityChange);
        Chest.GetComponent<Rigidbody>().AddForce(Vector3.up * 2.5f, ForceMode.VelocityChange);
        Head.GetComponent<Rigidbody>().AddForce(Vector3.up * 2.5f, ForceMode.VelocityChange);
        Spine.GetComponent<Rigidbody>().AddForce(Vector3.up * 2.5f, ForceMode.VelocityChange);

        yield return new WaitForSeconds(0.5f);

        Monoball.GetComponent<Rigidbody>().AddForce(IKChest.transform.forward / 1, ForceMode.VelocityChange);
        Chest.GetComponent<Rigidbody>().AddForce(IKChest.transform.forward / 1, ForceMode.VelocityChange);
        Head.GetComponent<Rigidbody>().AddForce(IKChest.transform.forward / 1f, ForceMode.VelocityChange);
        Spine.GetComponent<Rigidbody>().AddForce(IKChest.transform.forward / 1f, ForceMode.VelocityChange);

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

        Monoball.GetComponent<Rigidbody>().MovePosition(Monoball.transform.position + (XRCamera.transform.parent.localRotation * roomscaleMove));
        Fender.GetComponent<Rigidbody>().MovePosition(Fender.transform.position + (XRCamera.transform.parent.localRotation * roomscaleMove));
        Chest.GetComponent<Rigidbody>().MovePosition(Chest.transform.position + (XRCamera.transform.parent.localRotation * roomscaleMove));
        Head.GetComponent<Rigidbody>().MovePosition(Head.transform.position + (XRCamera.transform.parent.localRotation * roomscaleMove));
    }
    private void GetContollerInputValues()
    {
        //Trackpad
        LeftTrackPadVector = LeftTrackPad.action.ReadValue<Vector2>();
        leftTrackPadTouched = LeftTrackPadTouch.action.IsPressed();
        leftTrackPadPressed = LeftTrackPadPressed.action.IsPressed();

        //Trackpad
        RightTrackPadVector = RightTrackPad.action.ReadValue<Vector2>();

        headYaw = Quaternion.Euler(0, XRCamera.transform.eulerAngles.y, 0);
        moveDirection = headYaw * new Vector3(LeftTrackPadVector.x, 0, LeftTrackPadVector.y);
        monoballTorque = new Vector3(moveDirection.z, 0, -moveDirection.x);

        jumpPressed = jump.action.IsPressed();
    }

    //------Transforms---------------------------------------------------------------------------------------
    private void CameraToPlayer()
    {
        XRCamera.transform.position = Head.transform.position;
    }
    private void XRRigToPlayer()
    {
        XRRig.transform.position = new Vector3(Fender.transform.position.x, Fender.transform.position.y - (0.5f * Fender.transform.localScale.y + 0.5f * Monoball.transform.localScale.y), Fender.transform.position.z);
        headTarget.transform.position = Head.transform.position;
        headTarget.transform.rotation = XRCamera.transform.rotation;
    }
    private void RotatePlayer()
    {
        Chest.transform.rotation = Quaternion.Euler(0, IKChest.eulerAngles.y, 0);
    }
    //-----HexaBody Movement---------------------------------------------------------------------------------
    private void MovePlayerViaController()
    {
        if (!jumping)
        {
            if (!leftTrackPadTouched)
            {
                StopMonoball();
            }

            else if (!leftTrackPadPressed && leftTrackPadTouched)
            {
                MoveMonoball(moveForceWalk);
            }

            else if (leftTrackPadPressed)
            {
                MoveMonoball(moveForceSprint);
            }
        }

        else if (jumping)
        {
            if (!leftTrackPadTouched)
            {
                StopMonoball();
            }

            else if (leftTrackPadTouched)
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
        if (jumpPressed)
        {
            jumping = true;
            JumpSitDown();
        }

        else if (!jumpPressed && jumping)
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
            trackedOffset.transform.localPosition = Vector3.Lerp(trackedOffset.transform.localPosition, new Vector3(0, 0 - 2.4f, 0), 0.005f);
        }
    }
    private void JumpSitUp()
    {
        CrouchTarget = new Vector3(0, highestCrouch - additionalHeight, 0);
        Spine.targetPosition = CrouchTarget;
        AudioSource.PlayClipAtPoint(jumpSound, finalSolver.transform.position, 0.2f);

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
            Monoball.GetComponent<Rigidbody>().AddForce(IKChest.transform.forward / 2f, ForceMode.VelocityChange);
            Chest.GetComponent<Rigidbody>().AddForce(IKChest.transform.forward / 2f, ForceMode.VelocityChange);
            Fender.GetComponent<Rigidbody>().AddForce(IKChest.transform.forward / 2f, ForceMode.VelocityChange);
            Head.GetComponent<Rigidbody>().AddForce(IKChest.transform.forward / 2f, ForceMode.VelocityChange);
        }
    }

    //------Joint Controll-----------------------------------------------------------------------------------
    private void SpineContractionOnRealWorldCrouch()
    {
        CrouchTarget.y = Mathf.Clamp(CameraController.transform.localPosition.y - additionalHeight, -0.1f, highestCrouch - additionalHeight);
        Spine.targetPosition = new Vector3(0, CrouchTarget.y, 0);
    }
}
