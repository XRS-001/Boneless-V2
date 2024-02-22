using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMapping : MonoBehaviour
{
    public Transform[] bones;
    public Transform[] targets;
    public Vector3[] targetsOffsetRot;
    public Vector3[] targetsOffsetPos;

    public Transform[] twists;
    public Transform[] twistTargets;
    public Vector3[] twistsOffsetRot;
    public Vector3[] twistsOffsetPos;

    // Update is called once per frame
    void LateUpdate()
    {
        for(int i = 0; i < bones.Length; i++)
        {
            bones[i].position = targets[i].position + targetsOffsetPos[i];
            bones[i].rotation = targets[i].rotation * Quaternion.Euler(targetsOffsetRot[i]);
        }
        for (int i = 0; i < twists.Length; i++)
        {
            twists[i].position = twistTargets[i].position + twistsOffsetPos[i];
            twists[i].rotation = twistTargets[i].rotation * Quaternion.Euler(targetsOffsetPos[i]);
        }
    }
}
