using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimator : MonoBehaviour
{
    public InputActionProperty trigger;
    public InputActionProperty triggerTouch;
    public InputActionProperty grip;
    public InputActionProperty thumb;
    public Animator handAnimator;
    public Animator thumbAnimator;
    [Header("Collider Animation:")]
    public Animator handAnimatorPhysics;
    public Animator thumbAnimatorPhysics;
    private bool thumbTouched = false;
    private bool triggerTouched;

    // Update is called once per frame
    void Update()
    {
        //trigger
        float triggerValue = trigger.action.ReadValue<float>();
        float triggerTouchValue = triggerTouch.action.ReadValue<float>();
        if (triggerTouchValue > 0f && !triggerTouched)
        {
            StartCoroutine(TriggerTouch());
        }
        else if (triggerTouchValue > 0f && triggerTouched)
        {
            handAnimator.SetFloat("Trigger", 0.3f);
            handAnimatorPhysics.SetFloat("Trigger", 0.3f);
        }
        else if (triggerTouched) 
        {
            StartCoroutine(TriggerUnTouch());
        }
        if(!triggerTouched && triggerTouchValue == 0f)
        {
            handAnimator.SetFloat("Trigger", 0);
            handAnimatorPhysics.SetFloat("Trigger", 0);
        }
        if (triggerValue > 0.3f)
        {
            handAnimator.SetFloat("Trigger", Mathf.Lerp(0.3f, 1f, triggerValue));
            handAnimatorPhysics.SetFloat("Trigger", Mathf.Lerp(0.3f, 1f, triggerValue));
        }

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
    //smoothly transition the blend value of the trigger
    public IEnumerator TriggerTouch()
    {
        float timer = 0;

        while (timer < 0.1f)
        {
            float triggerTouchValue = triggerTouch.action.ReadValue<float>();
            if (triggerTouchValue > 0f)
            {
                handAnimator.SetFloat("Trigger", Mathf.Lerp(0, 0.3f, timer / 0.05f));
                handAnimatorPhysics.SetFloat("Trigger", Mathf.Lerp(0, 0.3f, timer / 0.05f));
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
                handAnimator.SetFloat("Trigger", Mathf.Lerp(0.3f, 0, timer / 0.05f));
                handAnimatorPhysics.SetFloat("Trigger", Mathf.Lerp(0.3f, 0, timer / 0.05f));

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
