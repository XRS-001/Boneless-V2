using RootMotion.FinalIK;
using System.Collections;
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
    public Transform directionSource;
    private Vector3 direction;
    private Vector2 inputMoveAxis;
    private float inputTurnAxis;
    private bool isGrounded;
    private bool isClimbing;
    private bool isRunning;

    public DetectCollisionFeet[] feetDetection;
    public DetectCollisionJoint[] handDetection;
    private void Start()
    {
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
            jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
            rb.velocity = Vector3.up * jumpVelocity + direction;
            audioSource.PlayOneShot(jumpAudio, 1f);
        }
        if(JumpInput && isClimbing)
        {
            jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
            rb.velocity = Vector3.up * jumpVelocity + direction + rb.transform.forward;
            audioSource.PlayOneShot(jumpAudio, 1f);
        }
    }
    private void FixedUpdate()
    {
        isGrounded = CheckIfGrounded();
        isClimbing = CheckIfClimbing();

        if (isGrounded)
        {
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
