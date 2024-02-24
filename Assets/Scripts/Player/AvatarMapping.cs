using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMapping : MonoBehaviour
{
    public Transform[] bones;
    public Transform[] targets;

    public Transform[] twists;
    public Transform[] twistTargets;

    // Update is called once per frame
    void LateUpdate()
    {
        for(int i = 0; i < bones.Length; i++)
        {
            bones[i].position = targets[i].position;
            bones[i].rotation = targets[i].rotation;
        }
        for (int i = 0; i < twists.Length; i++)
        {
            twists[i].position = twistTargets[i].position;
            twists[i].rotation = twistTargets[i].rotation;
        }
    }
}
