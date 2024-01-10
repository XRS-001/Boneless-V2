using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
public class FootstepsAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isMoving;
    private Vector2 inputMoveAxis;
    private InputActionProperty moveInputSource;
    private ContinuousMovementPhysics movement;
    private void Start()
    {
        movement = GetComponent<ContinuousMovementPhysics>();
        moveInputSource = movement.moveInputSource;
        audioSource = GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        inputMoveAxis = moveInputSource.action.ReadValue<Vector2>();

        // Check if the player is moving.
        isMoving = inputMoveAxis.magnitude > 0.1f;
        // Play footstep sound when moving, stop it when not moving.
        if (isMoving)
        {
            if (!audioSource.isPlaying)
            {
                if (movement.isRunning)
                {
                    audioSource.pitch = 0.75f * movement.runningSpeed;
                }
                else
                {
                    audioSource.pitch = 0.85f * movement.speed;
                }

                audioSource.Play();
            }
        }
    }
}
