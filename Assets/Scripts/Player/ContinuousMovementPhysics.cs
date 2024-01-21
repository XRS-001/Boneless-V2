using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ContinuousMovementPhysics : MonoBehaviour
{
    public float speed = 1.5f;
    public float runningSpeed = 2.5f;
    public float turnSpeed = 60;
    private float jumpVelocity;
    public float jumpHeight = 1.5f;
    public float vaultHeight = 1.5f;
    public AudioClip jumpAudio;
    public AudioClip stepAudio;
    public AudioSource audioSource;
    public InputActionProperty moveInputSource;
    public InputActionProperty runInputSource;
    public InputActionProperty turnInputSource;
    public InputActionProperty jumpInputSource;
    public Rigidbody rb;
    public VRIK ik;
    public PhysicsRig rig;
    public Transform directionSource;
    private Vector3 direction;
    private Vector2 inputMoveAxis;
    private float inputTurnAxis;
    private bool isGrounded;
    private bool isClimbing;
    private bool isMoving;
    private bool isRunning;
    private bool isJumping;

    public DetectCollisionFeet[] feetDetection;
    public DetectCollisionJoint[] handDetection;
    private List<GrabPhysics> grabPhysics = new List<GrabPhysics>();
    private void Start()
    {
        rig = GetComponent<PhysicsRig>();
        foreach(DetectCollisionJoint hand in handDetection)
        {
            grabPhysics.Add(hand.GetComponent<GrabPhysics>());
        }
        ik.solver.locomotion.onLeftFootstep.AddListener(OnStep);
        ik.solver.locomotion.onRightFootstep.AddListener(OnStep);
    }
    public void OnStep()
    {
        if (isGrounded)
        {
            if(isRunning)
            {
                audioSource.PlayOneShot(stepAudio, 0.35f);
            }
            else
            {
                audioSource.PlayOneShot(stepAudio, 0.6f);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        inputMoveAxis = moveInputSource.action.ReadValue<Vector2>();
        inputTurnAxis = turnInputSource.action.ReadValue<Vector2>().x;

        bool JumpInput = jumpInputSource.action.WasPressedThisFrame();

        if(JumpInput && isGrounded)
        {
            rb.isKinematic = false;
            isJumping = true;
            jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
            rb.velocity = Vector3.up * jumpVelocity + direction;
            audioSource.PlayOneShot(jumpAudio, 1f);
            StartCoroutine(DelayKinematic());
        }
        if (JumpInput && isClimbing)
        {
            rb.isKinematic = false;
            isJumping = true;
            jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
            rb.velocity = Vector3.up * jumpVelocity * 1.25f + direction + rb.transform.forward;
            audioSource.PlayOneShot(jumpAudio, 1f);
            StartCoroutine(DelayJump());
        }
    }
    IEnumerator DelayJump()
    {
        yield return new WaitForSeconds(0.1f);
        isJumping = false;
    }
    IEnumerator DelayKinematic()
    {
        yield return new WaitForSeconds(1f);
        bool isGrabClimbing = false;
        foreach (GrabPhysics hand in grabPhysics)
        {
            if (hand.isClimbing)
            {
                isGrabClimbing = true;
            }
        }
        if (isGrounded && !isGrabClimbing && !isJumping && !rig.isBodyColliding)
        {
            rb.isKinematic = true;
        }
    }
    private void FixedUpdate()
    {
        isGrounded = CheckIfGrounded();
        isClimbing = CheckIfClimbing();
        bool isGrabClimbing = false;
        bool isGrabbingRagdoll = false;
        foreach (GrabPhysics hand in grabPhysics)
        {
            if (hand.isClimbing)
            {
                isGrabClimbing = true;
            }
            if (hand.isGrabbingRagdoll)
            {
                isGrabbingRagdoll = true;
            }
        }
        if(isGrounded && !isGrabClimbing && !isJumping && !rig.isBodyColliding)
        {
            if(!rb.isKinematic)
            {
                StartCoroutine(DelayKinematic());
            }
        }
        else
        {
            if(!rig.delayStart)
            {
                rb.isKinematic = false;
            }
        }
        if (isGrabbingRagdoll)
        {
            rb.isKinematic = true;
        }
        if (isGrounded)
        {
            if(inputMoveAxis != Vector2.zero) 
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
            Quaternion yaw = Quaternion.Euler(0, directionSource.eulerAngles.y, 0);
            direction = yaw * new Vector3(inputMoveAxis.x, 0, inputMoveAxis.y);

            Vector3 targetMovePosition;
            if(runInputSource.action.ReadValue<float>() > 0f)
            {
                isRunning = true;
                targetMovePosition = rb.position + direction * Time.fixedDeltaTime * runningSpeed;
            }
            else
            {
                isRunning = false;
                targetMovePosition = rb.position + direction * Time.fixedDeltaTime * speed;
            }

            Vector3 axis = Vector3.up;
            float angle = turnSpeed * Time.fixedDeltaTime * inputTurnAxis;

            Quaternion q = Quaternion.AngleAxis(angle, axis);
            rb.MoveRotation(rb.rotation * q);
            Vector3 newPosition = q * (targetMovePosition - directionSource.position) + directionSource.position;
            rb.MovePosition(newPosition);
        }
    }
    public void LimitPositionLeft()
    {
        if (isMoving)
        {
            Vector3 direction = rb.position - (handDetection[1].transform.position - new Vector3(directionSource.localPosition.x, 0, directionSource.localPosition.z));

            if (direction.magnitude > 1.3f)
            {
                direction = direction.normalized * 1.3f;
                Vector3 newVector = handDetection[1].transform.position - new Vector3(directionSource.localPosition.x, 0, directionSource.localPosition.z) + direction;
                rb.position = new Vector3(newVector.x, rb.position.y, newVector.z);
            }
        }
    }
    public void LimitPositionRight()
    {
        if (isMoving)
        {
            Vector3 direction = rb.position - (handDetection[0].transform.position - new Vector3(directionSource.localPosition.x, 0, directionSource.localPosition.z));

            if (direction.magnitude > 1.3f)
            {
                direction = direction.normalized * 1.3f;
                Vector3 newVector = handDetection[0].transform.position - new Vector3(directionSource.localPosition.x, 0, directionSource.localPosition.z) + direction;
                rb.position = new Vector3(newVector.x, rb.position.y, newVector.z);
            }
        }
    }
    public bool CheckIfGrounded()
    {
        bool isGrounded = false;
        foreach (DetectCollisionFeet detectCollision  in feetDetection) 
        {
            if(detectCollision.isColliding)
            {
                isGrounded = true;
            }
        }
        return isGrounded;
    }
    public bool CheckIfClimbing()
    {
        bool isClimbing = false;
        foreach (DetectCollisionJoint detectCollision in handDetection)
        {
            if (detectCollision.isColliding)
            {
                isClimbing = true;
            }
        }
        return isClimbing;
    }
}
