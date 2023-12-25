using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ContinuousMovementPhysics : MonoBehaviour
{
    public float speed = 1;
    public float turnSpeed = 60;
    private float jumpVelocity;
    public float jumpHeight = 1.5f;
    public InputActionProperty moveInputSource;
    public InputActionProperty turnInputSource;
    public InputActionProperty jumpInputSource;
    public Rigidbody rb;
    public Transform directionSource;
    private Vector3 direction;
    private Vector2 inputMoveAxis;
    private float inputTurnAxis;
    private bool isGrounded;
    private bool isClimbing;
    private bool isJumping;

    public DetectCollisionNoRb[] feetDetection;
    public DetectCollisionRb[] handDetection;

    // Update is called once per frame
    void Update()
    {
        inputMoveAxis = moveInputSource.action.ReadValue<Vector2>();
        inputTurnAxis = turnInputSource.action.ReadValue<Vector2>().x;

        bool JumpInput = jumpInputSource.action.WasPressedThisFrame();
        float jumpValue = jumpInputSource.action.ReadValue<float>();

        if(JumpInput && !isJumping)
        {
            StartCoroutine(JumpRoutineCrouch());
        }
        if(jumpValue == 0 && isJumping)
        {
            StartCoroutine(JumpRoutine());
        }
    }
    IEnumerator JumpRoutine()
    {
        float timer = 0;
        Vector3 newPosition = directionSource.parent.transform.localPosition;
        while (timer < 0.1f)
        {
            directionSource.parent.transform.localPosition = Vector3.Lerp(newPosition, new Vector3(0,0,0), timer / 0.1f);
            timer += Time.deltaTime;
            yield return null;
        }
        directionSource.parent.transform.localPosition = new Vector3(0, 0, 0);

        jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
        rb.velocity = Vector3.up * jumpVelocity + direction;
        isJumping = false;
    }
    IEnumerator JumpRoutineCrouch()
    {
        if(!isClimbing && isGrounded)
        {
            isJumping = true;
            //bouncing the parent of the tracked objects to simulate jumping
            float timer = 0;
            while (timer < 0.1f)
            {
                directionSource.parent.transform.localPosition = Vector3.Lerp(new Vector3(0, 0, 0), (new Vector3(0, 0, 0) - Vector3.up / 3f) * directionSource.localPosition.y, timer / 0.1f);
                timer += Time.deltaTime;
                yield return null;
            }
        }
        else if (isClimbing)
        {
            isJumping = true;
            jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
            rb.velocity = Vector3.up * jumpVelocity + (Vector3.up * 3) + directionSource.forward;
            isJumping = false;
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

            Vector3 targetMovePosition = rb.position + direction * Time.fixedDeltaTime * speed;

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
        foreach (DetectCollisionNoRb detectCollision  in feetDetection) 
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
        foreach (DetectCollisionRb detectCollision in handDetection)
        {
            if (detectCollision.isColliding)
            {
                isClimbing = true;
            }
        }
        return isClimbing;
    }
}
