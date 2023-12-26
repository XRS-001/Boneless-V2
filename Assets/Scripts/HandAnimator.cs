using System.Collections;
using System.Collections.Generic;
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
    private bool thumbTouched = false;

    // Update is called once per frame
    void Update()
    {
        //trigger
        float triggerValue = trigger.action.ReadValue<float>();
        float triggerTouchValue = triggerTouch.action.ReadValue<float>();
        if (triggerTouchValue > 0)
        {
            handAnimator.SetFloat("Trigger", 0.3f);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0f);
        }
        if (triggerValue > 0.3f)
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }

        //grip
        float gripValue = grip.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);
        //thumb
        float thumbTouch = thumb.action.ReadValue<float>();
        if (thumbTouch > 0)
        {
            thumbTouched = true;
            thumbAnimator.Play("Thumb", 0);
        }
        else if (thumbTouched)
        {
            thumbTouched = false;
            thumbAnimator.Play("ThumbReverse", 0);
        }
    }
}
