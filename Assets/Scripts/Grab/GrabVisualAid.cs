using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumDeclaration;
[ExecuteInEditMode]
public class GrabVisualAid : MonoBehaviour
{
    public handTypeEnum handType;
    public GrabTwoAttach grab;

    // Update is called once per frame
    void Update()
    {
        if(grab)
            if (handType == handTypeEnum.Left)
            {
                transform.position = grab.transform.TransformPoint(grab.leftAttach.leftAttachPosition);
                if (!(grab is GrabDynamic))
                {
                    transform.rotation = grab.transform.rotation * Quaternion.Euler(grab.leftAttach.leftAttachRotation);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(grab.leftAttach.leftAttachRotation);
                }
            }
            else
            {
                transform.position = grab.transform.TransformPoint(grab.rightAttach.rightAttachPosition);
                if (!(grab is GrabDynamic))
                {
                    transform.rotation = grab.transform.rotation * Quaternion.Euler(grab.rightAttach.rightAttachRotation);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(grab.rightAttach.rightAttachRotation);
                }
            }
    }
}
