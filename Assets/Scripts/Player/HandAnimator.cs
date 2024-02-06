using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
[System.Serializable]
public class IndexFingerBones
{
    public Transform index1;
    public Transform index2;
    public Transform index3;
}
public class HandAnimator : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty trigger;
    public InputActionProperty triggerTouch;
    public InputActionProperty grip;
    public InputActionProperty thumb;

    [Header("Animation")]
    public Animator handAnimator;
    public Animator thumbAnimator;

    [Header("Collider Animation:")]
    public Animator handAnimatorPhysics;
    public Animator thumbAnimatorPhysics;

    private float triggeredValue;
    private bool thumbTouched = false;
    private bool triggerTouched;

    private GrabPhysics grabPhysics;
    [Header("Index Finger")]
    public IndexFingerBones indexFingerBones;
    private Quaternion originalIndex1Rotation;
    private Quaternion originalIndex2Rotation;
    private Quaternion originalIndex3Rotation;
    private void Start()
    {
        originalIndex1Rotation = indexFingerBones.index1.localRotation;
        originalIndex2Rotation = indexFingerBones.index2.localRotation;
        originalIndex3Rotation = indexFingerBones.index3.localRotation;
        grabPhysics = GetComponent<GrabPhysics>();
    }
    // Update is called once per frame
    void Update()
    {
        //giving freedom to the index finger to rotate during grabbing
        FreeIndexFinger();
        FingerInputs();
    }
    void FingerInputs()
    {
        //trigger
        float triggerValue = trigger.action.ReadValue<float>();
        float triggerTouchValue = triggerTouch.action.ReadValue<float>();

        if (triggerTouchValue > 0f && !triggerTouched) { StartCoroutine(TriggerTouch()); }

        else if (triggerTouchValue > 0f && triggerTouched) { triggeredValue = 0.5f; }

        else if (triggerTouched) { StartCoroutine(TriggerUnTouch()); }

        if (!triggerTouched && triggerTouchValue == 0f) { triggeredValue = 0f; }

        if (triggerValue > 0f) { triggeredValue = Mathf.Lerp(0.5f, 1f, triggerValue); }

        handAnimator.SetFloat("Trigger", triggeredValue);
        handAnimatorPhysics.SetFloat("Trigger", triggeredValue);

        //grip
        float gripValue = grip.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);
        handAnimatorPhysics.SetFloat("Grip", gripValue);

        //thumb
        float thumbTouch = thumb.action.ReadValue<float>();
        if (thumbTouch > 0f)
        {
            thumbTouched = true;
            thumbAnimator.Play("Thumb", 0);
            thumbAnimatorPhysics.Play("Thumb", 0);
        }
        else if (thumbTouched)
        {
            thumbTouched = false;
            thumbAnimator.Play("ThumbReverse", 0);
            thumbAnimatorPhysics.Play("ThumbReverse", 0);
        }
    }
    void FreeIndexFinger()
    {
        if (grabPhysics.grab)
        {
            if (!(grabPhysics.grab is GrabDynamic))
            {
                HandData h = grabPhysics.poseSetup.pose;
                if (h)
                {
                    indexFingerBones.index1.localRotation = Quaternion.Slerp(originalIndex1Rotation, h.indexFingerBones.index1.localRotation, triggeredValue);
                    indexFingerBones.index2.localRotation = Quaternion.Slerp(originalIndex2Rotation, h.indexFingerBones.index2.localRotation, triggeredValue);
                    indexFingerBones.index3.localRotation = Quaternion.Slerp(originalIndex3Rotation, h.indexFingerBones.index3.localRotation, triggeredValue);
                }
            }
        }
    }
    //smoothly transition the blend value of the trigger
    public IEnumerator TriggerTouch()
    {
        float timer = 0;

        while (timer < 0.1f)
        {
            float triggerTouchValue = triggerTouch.action.ReadValue<float>();
            if (triggerTouchValue > 0f)
            {
                triggeredValue = Mathf.Lerp(0, 0.5f, timer / 0.07f);
                handAnimator.SetFloat("Trigger", triggeredValue);
                handAnimatorPhysics.SetFloat("Trigger", triggeredValue);
            }
            else
            {
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        triggerTouched = true;
    }
    public IEnumerator TriggerUnTouch()
    {
        float timer = 0;

        while (timer < 0.1f)
        {
            float triggerTouchValue = triggerTouch.action.ReadValue<float>();
            if (triggerTouchValue == 0f)
            {
                triggeredValue = Mathf.Lerp(0.5f, 0, timer / 0.07f);
                handAnimator.SetFloat("Trigger", triggeredValue);
                handAnimatorPhysics.SetFloat("Trigger", triggeredValue);
            }
            else
            {
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        triggerTouched = false;
    }
}
