using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using RootMotion.FinalIK;
using Unity.VisualScripting;

public class HexaBody : MonoBehaviour
{
    [Header("XR Toolkit Parts")]
    public XROrigin XRRig;
    public GameObject XRCamera;
    public Transform head;
    public Transform chest;
    public Transform trackedOffset;
    public VRIK physicsIK;

    [Header("Actionbased Controller")]
    public ActionBasedController CameraController;
    public ActionBasedController RightHandController;
    public ActionBasedController LeftHandController;

    public InputActionReference LeftTrackPadClicked;
    public InputActionReference LeftTrackPadTouch;

    public InputActionReference RightTrackPadClicked;

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
    bool isMoving = false;

    public float crouchSpeed;
    public float lowestCrouch;
    public float highestCrouch;
    private float additionalHeight;

    Vector3 CrouchTarget;

    //---------Input Values---------------------------------------------------------------------------------------------------------------//

    private Quaternion headYaw;
    private Vector3 moveDirection;
    private Vector3 monoballTorque;

    private Vector3 CameraControllerPos;

    private Vector3 previousHeadPosition;
    private Vector3 currentHeadVelocity;

    private Vector2 LeftTrackPadVector;
    private Vector2 RightTrackPadVector;

    private float leftTrackPadPressed;
    private float rightTrackPadPressed;

    private float leftTrackPadTouched;

    void Start()
    {
        additionalHeight = (0.5f * Monoball.transform.lossyScale.y) + (0.5f * Fender.transform.lossyScale.y) + (Head.transform.position.y - Chest.transform.position.y);
        previousHeadPosition = CameraController.positionAction.action.ReadValue<Vector3>();
    }
    void Update()
    {
        CameraToPlayer();
        XRRigToPlayer();
        GetContollerInputValues();
        SetHeadTarget();
    }

    private void FixedUpdate() 
    {
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
    void TransformSpineCollider()
    {
        Chest.GetComponent<CapsuleCollider>().center = new Vector3(Chest.transform.InverseTransformPoint(chest.position).x, Chest.GetComponent<CapsuleCollider>().center.y, Chest.transform.InverseTransformPoint(chest.position).z);
        Chest.GetComponent<CapsuleCollider>().height = Vector3.Distance(new Vector3(0, Head.transform.position.y, 0), new Vector3(0, Fender.transform.position.y, 0)) * 2;
    }
    void Climbing()
    {
        bool isClimbing = false;
        foreach (TargetLimb limb in limbs)
        {
            if (limb.isColliding)
            {
                isClimbing = true;
            }
        }
        if (isClimbing)
        {
            float drag = 1 / Monoball.GetComponent<Rigidbody>().velocity.magnitude * 10;

            Head.GetComponent<Rigidbody>().drag = drag;
            Monoball.GetComponent<Rigidbody>().drag = drag;
            Spine.GetComponent<Rigidbody>().drag = drag;
            Fender.GetComponent<Rigidbody>().drag = drag;
        }
        else
        {
            Head.GetComponent<Rigidbody>().drag = 0;
            Monoball.GetComponent<Rigidbody>().drag = 0;
            Spine.GetComponent<Rigidbody>().drag = 0;
            Fender.GetComponent<Rigidbody>().drag = 0;
        }
    }
    void SetHeadTarget()
    {
        if (physicsIK.solver.spine.headTarget == null && GameObject.Find("Head Target"))
        {
            physicsIK.solver.spine.headTarget = GameObject.Find("Head Target").transform;
        }
    }
    void RoomScaleMove()
    {
        currentHeadVelocity = (CameraControllerPos - previousHeadPosition) / Time.deltaTime;
        previousHeadPosition = CameraControllerPos;
        currentHeadVelocity.y = 0;
        if (!isMoving)
        {
            Monoball.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(Monoball.transform.position, Monoball.transform.position + currentHeadVelocity, 0.015f));
        }
        else
        {
            Chest.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(Chest.transform.position, Chest.transform.position + currentHeadVelocity, 0.015f));
        }
    }
    private void GetContollerInputValues()
    {
        //Trackpad
        LeftTrackPadVector = LeftTrackPad.action.ReadValue<Vector2>();
        leftTrackPadPressed = LeftTrackPadClicked.action.ReadValue<float>();
        leftTrackPadTouched = LeftTrackPadTouch.action.ReadValue<float>();

        //Trackpad
        RightTrackPadVector = RightTrackPad.action.ReadValue<Vector2>();
        rightTrackPadPressed = RightTrackPadClicked.action.ReadValue<float>();

        //Camera Inputs
        CameraControllerPos = CameraController.positionAction.action.ReadValue<Vector3>();

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
        XRRig.transform.position = Vector3.Lerp(XRRig.transform.position, new Vector3(Fender.transform.position.x, Fender.transform.position.y - (0.5f * Fender.transform.localScale.y + 0.5f * Monoball.transform.localScale.y), Fender.transform.position.z) - new Vector3(CameraControllerPos.x, 0, CameraControllerPos.z), 0.25f);
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
        isMoving = true;
        Monoball.GetComponent<Rigidbody>().freezeRotation = false;
        Monoball.GetComponent<Rigidbody>().angularDrag = angularDragOnMove;
        Monoball.GetComponent<Rigidbody>().AddTorque(monoballTorque.normalized * force, ForceMode.Force);
    }
    private void StopMonoball()
    {
       isMoving = false;
       Monoball.GetComponent<Rigidbody>().freezeRotation = true;
       Monoball.GetComponent<Rigidbody>().angularDrag = angularBreakDrag;
    }

    //------Jumping------------------------------------------------------------------------------------------
    private void Jump()
    {
        if (rightTrackPadPressed == 1 && RightTrackPadVector.y < 0)
        {
            if(!jumping)
            {
                trackedOffset.transform.localPosition = new Vector3(0, -(CrouchTarget.y - lowestCrouch), 0);
            }
            jumping = true;
            JumpSitDown();
        }

        else if ((rightTrackPadPressed == 0) && jumping == true)
        {
            trackedOffset.transform.localPosition = Vector3.zero;
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
        }
    }
    private void JumpSitUp()
    {
        CrouchTarget = new Vector3(0, highestCrouch - additionalHeight, 0);
        Spine.targetPosition = CrouchTarget;
    }

    //------Joint Controll-----------------------------------------------------------------------------------
    private void SpineContractionOnRealWorldCrouch()
    {
        CrouchTarget.y = Mathf.Clamp(CameraControllerPos.y - additionalHeight, lowestCrouch, highestCrouch - additionalHeight);
        Spine.targetPosition = new Vector3(0, CrouchTarget.y, 0);
    }
}
