using System.Collections;
using UnityEngine;
using System;

public class SetPose : MonoBehaviour
{
    public float poseTransitionDuration = 0.2f;

    [NonSerialized]
    public HandData pose;
    [NonSerialized]
    public bool setDynamicPose;
    [NonSerialized]
    public bool exitingDynamicPose;
    public HandData handData;

    private Quaternion startingHandRotation;
    private Quaternion finalHandRotation;

    private Quaternion[] startingFingerRotations;
    private Quaternion[] finalFingerRotations;
    private Vector3[] finalFingerPositions;
    private GrabPhysics grabPhysics;
    private void Start()
    {
        grabPhysics = GetComponent<GrabPhysics>();
    }
    public void SetupPose(HandData resettedHandData)
    {
        handData.thumbAnimator.enabled = false;
        handData.animator.enabled = false;

        SetHandDataValues(handData, pose);
        if (resettedHandData)
            handData = resettedHandData;
        StartCoroutine(SetHandDataRoutine(handData, finalHandRotation, finalFingerRotations, startingHandRotation, startingFingerRotations, false));
    }
    public void UnSetPose()
    {
        StartCoroutine(SetHandDataRoutine(handData, startingHandRotation, startingFingerRotations, finalHandRotation, finalFingerRotations, true));
    }
    public void SetHandDataValues(HandData h1, HandData h2)
    {
        startingHandRotation = h1.root.localRotation;
        finalHandRotation = h2.root.localRotation;

        startingFingerRotations = new Quaternion[h1.originalBones.Length];
        finalFingerRotations = new Quaternion[h2.fingerBones.Length];
        finalFingerPositions = new Vector3[h2.fingerBones.Length];

        for (int i = 0; i < h1.fingerBones.Length; i++)
        {
            startingFingerRotations[i] = h1.originalBones[i].localRotation;
            finalFingerRotations[i] = h2.fingerBones[i].localRotation;
            finalFingerPositions[i] = h2.fingerBones[i].position;
        }
    }

    public IEnumerator SetHandDataRoutine(HandData h, Quaternion newRotation, Quaternion[] newBonesRotation, Quaternion startingRotation, Quaternion[] startingBonesRotation, bool enableAnimators)
    {
        float timer = 0;

        while (timer < poseTransitionDuration)
        {
            Quaternion r = Quaternion.Lerp(startingRotation, newRotation, timer / poseTransitionDuration);

            h.root.localRotation = r;

            for (int i = 0; i < newBonesRotation.Length; i++)
            {
                if (setDynamicPose)
                {
                    if (!h.hasHit[i])
                    {
                        h.fingerBones[i].localRotation = Quaternion.Lerp(startingBonesRotation[i], newBonesRotation[i], timer / poseTransitionDuration);
                    }
                }
                else
                {
                    if (!exitingDynamicPose)
                    {
                        if (!h.fingerBones[i].name.Contains("Index"))
                        {
                            h.fingerBones[i].localRotation = Quaternion.Lerp(startingBonesRotation[i], newBonesRotation[i], timer / poseTransitionDuration);
                        }
                    }
                    else
                    {
                        h.fingerBones[i].localRotation = Quaternion.Lerp(startingBonesRotation[i], newBonesRotation[i], timer / poseTransitionDuration);
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
        if (exitingDynamicPose)
            exitingDynamicPose = false;
        if (setDynamicPose)
        {
            setDynamicPose = false;
            for (int i = 0; i < newBonesRotation.Length; i++)
            {
                newBonesRotation[i] = h.fingerBones[i].localRotation;
            }
        }
        if (enableAnimators && !grabPhysics.isGrabbing)
        {
            handData.thumbAnimator.enabled = true;
            handData.animator.enabled = true;
        }
    }
}
