using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumDeclaration;
public class GrabVisualAid : MonoBehaviour
{
    public handTypeEnum handType;
    public GrabTwoAttach grab;

    // Update is called once per frame
    void Update()
    {
        if(handType == handTypeEnum.Left)
        {
            transform.position = grab.transform.TransformPoint(grab.leftAttach.leftAttachPosition);
            transform.rotation = grab.transform.rotation * Quaternion.Euler(grab.leftAttach.leftAttachRotation);
        }
        else
        {
            transform.position = grab.transform.TransformPoint(grab.rightAttach.rightAttachPosition);
            transform.rotation = grab.transform.rotation * Quaternion.Euler(grab.rightAttach.rightAttachRotation);
        }
    }
}
