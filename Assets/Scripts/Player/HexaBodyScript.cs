using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using RootMotion.FinalIK;

public class HexaBodyScript : MonoBehaviour
{
    [Header("XR Toolkit Parts")]
    public XROrigin XRRig;
    public GameObject XRCamera;
    public Transform head;
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
        previousHeadPosition = CameraController.transform.position;
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
        MovePlayerViaController();
        Jump();

        if (!jumping)
        {
            SpineContractionOnRealWorldCrouch();
        }

        RotatePlayer();
        RoomScaleMove();
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
        Vector3 offset = CameraControllerPos;
        offset.y = 0;
        XRRig.transform.position = new Vector3(Fender.transform.position.x, Fender.transform.position.y - (0.5f * Fender.transform.localScale.y + 0.5f * Monoball.transform.localScale.y), Fender.transform.position.z) - offset;
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
            jumping = true;
            JumpSitDown();
        }

        else if ((rightTrackPadPressed == 0) && jumping == true)
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
