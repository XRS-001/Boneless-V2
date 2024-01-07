using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandData : MonoBehaviour
{
    public enum HandModelType { left, right };

    public HandModelType handType;
    public Transform root;
    public Animator animator;
    public Animator thumbAnimator;
    public Transform[] fingerBones;
    [HideInInspector]
    //for dynamic posing
    public List<bool> hasHit;
    public Transform[] originalBones;
    public IndexFingerBones indexFingerBones;
    public LayerMask interactableLayers;
    private void Start()
    {
        for (int i = 0; i < fingerBones.Length; i++)
        {
            hasHit.Add(false);
        }
    }
    private void Update()
    {
        for (int i = 0; i < fingerBones.Length; i++)
        {
            if (fingerBones[i].name.Contains("Thumb"))
            {
                if (!fingerBones[i].name.Contains("3") && !fingerBones[i].name.Contains("2"))
                {
                    if (!Physics.CheckSphere(fingerBones[i + 1].position, 0.01f, interactableLayers))
                    {
                        hasHit[i] = true;
                    }
                    else
                    {
                        hasHit[i] = false;
                    }
                    if (!Physics.CheckSphere(fingerBones[i + 2].position, 0.01f, interactableLayers))
                    {
                        hasHit[i] = true;
                    }
                    else
                    {
                        hasHit[i] = false;
                    }
                    //check if it isn't setting pose
                    if (animator.enabled)
                    {
                        hasHit[i] = false;
                    }
                }
                else if (fingerBones[i].name.Contains("2"))
                {
                    if (!Physics.CheckSphere(fingerBones[i + 1].position, 0.01f, interactableLayers))
                    {
                        hasHit[i] = true;
                    }
                    else
                    {
                        hasHit[i] = false;
                    }
                    //check if it isn't setting pose
                    if (animator.enabled)
                    {
                        hasHit[i] = false;
                    }
                }
                else
                {
                    if (Physics.CheckSphere(fingerBones[i].position, 0.005f, interactableLayers))
                    {
                        hasHit[i] = true;
                    }
                    //check if it isn't setting pose
                    if (animator.enabled)
                    {
                        hasHit[i] = false;
                    }
                }
            }
            else
            {
                if (!fingerBones[i].name.Contains("3") && !fingerBones[i].name.Contains("2"))
                {
                    if (Physics.CheckSphere(fingerBones[i + 1].position, 0.01f, interactableLayers))
                    {
                        hasHit[i] = true;
                    }
                    if (Physics.CheckSphere(fingerBones[i + 2].position, 0.01f, interactableLayers))
                    {
                        hasHit[i] = true;
                    }
                    //check if it isn't setting pose
                    else if (animator.enabled)
                    {
                        hasHit[i] = false;
                    }
                }
                else if (fingerBones[i].name.Contains("2"))
                {
                    if (Physics.CheckSphere(fingerBones[i + 1].position, 0.01f, interactableLayers))
                    {
                        hasHit[i] = true;
                    }
                    //check if it isn't setting pose
                    else if (animator.enabled)
                    {
                        hasHit[i] = false;
                    }
                }
                else
                {
                    if (Physics.CheckSphere(fingerBones[i].position, 0.01f, 1 << 9))
                    {
                        hasHit[i] = true;
                    }
                    //check if it isn't setting pose
                    else if (animator.enabled)
                    {
                        hasHit[i] = false;
                    }
                }
            }
        }
    }
}
