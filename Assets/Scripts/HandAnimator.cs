using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimator : MonoBehaviour
{
    public InputActionProperty trigger;
    public InputActionProperty triggerTouch;
    public InputActionProperty grip;
    public InputActionProperty gripTouch;
    public InputActionProperty thumb;
    public Animator handAnimator;

    // Update is called once per frame
    void Update()
    {
        if (false)
        {
            //trigger
            float triggerValue = trigger.action.ReadValue<float>();
            float triggerTouchValue = triggerTouch.action.ReadValue<float>();
            if (triggerTouchValue > 0)
            {
                handAnimator.SetFloat("Trigger", 0.3f);
            }
            if (triggerValue > 0.3f)
            {
                handAnimator.SetFloat("Trigger", triggerValue);
            }

            //grip
            float gripValue = grip.action.ReadValue<float>();
            float gripTouchValue = gripTouch.action.ReadValue<float>();
            if (gripTouchValue > 0)
            {
                handAnimator.SetFloat("Grip", 0.3f);
            }
            if (gripValue > 0.3f)
            {
                handAnimator.SetFloat("Grip", gripValue);
            }

            //thumb
            float thumbTouch = thumb.action.ReadValue<float>();
            if (thumbTouch > 0)
            {
                handAnimator.SetBool("Thumb", true);
            }
            else
            {
                handAnimator.SetBool("Thumb", false);
            }
        }
    }
}
