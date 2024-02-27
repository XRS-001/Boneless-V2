using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMapping : MonoBehaviour
{
    public Transform[] bones;
    public Transform[] targets;

    // Update is called once per frame
    void LateUpdate()
    {
        for(int i = 0; i < bones.Length; i++)
        {
            bones[i].position = targets[i].position;
            bones[i].rotation = targets[i].rotation;
        }
    }
}
