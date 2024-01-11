using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
public class FootstepsAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isMoving;
    private bool hasPlayed = false;
    private Vector2 inputMoveAxis;
    private InputActionProperty moveInputSource;
    private ContinuousMovementPhysics movement;
    public AudioClip footstepAudio;
    private void Start()
    {
        movement = GetComponent<ContinuousMovementPhysics>();
        moveInputSource = movement.moveInputSource;
        audioSource = movement.audioSource;
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
            if (!hasPlayed)
            {
                StartCoroutine(PlayAudio());
                hasPlayed = true;
            }
        }
    }
    IEnumerator PlayAudio()
    {
        if (movement.isRunning)
        {
            yield return new WaitForSeconds(0.5f / movement.runningSpeed);
            audioSource.PlayOneShot(footstepAudio);
        }
        else
        {
            yield return new WaitForSeconds(0.5f / movement.speed);
            audioSource.PlayOneShot(footstepAudio);
        }
        hasPlayed = false;
    }
}
