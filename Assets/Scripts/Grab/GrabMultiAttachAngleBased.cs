using UnityEngine;
using static EnumDeclaration;
public class GrabMultiAttachAngleBased : GrabTwoAttach
{
    [Tooltip("The angle difference needed to switch attaches")]
    public float angleThreshold;
    [Tooltip("The up axis of the interactable")]
    public upDirection upAxis;
    public LeftAttach altLeftAttach;
    public RightAttach altRightAttach;
    private LeftAttach primaryLeftAttach;
    private RightAttach primaryRightAttach;
    public Transform leftHand;
    public Transform rightHand;
    private void Start()
    {
        primaryLeftAttach = new LeftAttach();
        primaryLeftAttach.leftAttachPosition = leftAttach.leftAttachPosition; primaryLeftAttach.leftAttachRotation = leftAttach.leftAttachRotation;
        primaryRightAttach = new RightAttach();
        primaryRightAttach.rightAttachPosition = rightAttach.rightAttachPosition; primaryRightAttach.rightAttachRotation = rightAttach.rightAttachRotation;
    }
    // Update is called once per frame
    void Update()
    {
        if (isHovering)
        {
            float dotLeft = 0;
            if (upAxis == upDirection.up)
            {
                dotLeft = Vector3.Dot(leftHand.up, transform.up);
            }
            else if (upAxis == upDirection.forward)
            {
                dotLeft = Vector3.Dot(leftHand.up, transform.forward);
            }
            else
            {
                dotLeft = Vector3.Dot(leftHand.up, transform.right);
            }
            if(dotLeft < (angleThreshold / 360))
            {
                leftAttach.leftAttachPosition = altLeftAttach.leftAttachPosition;
                leftAttach.leftAttachRotation = altLeftAttach.leftAttachRotation;
            }
            else
            {
                leftAttach.leftAttachPosition = primaryLeftAttach.leftAttachPosition;
                leftAttach.leftAttachRotation = primaryLeftAttach.leftAttachRotation;
            }
            float dotRight = 0;
            if (upAxis == upDirection.up)
            {
                dotRight = Vector3.Dot(rightHand.up, transform.up);
            }
            else if (upAxis == upDirection.forward)
            {
                dotRight = Vector3.Dot(rightHand.up, transform.forward);
            }
            else
            {
                dotRight = Vector3.Dot(rightHand.up, transform.right);
            }
            if (dotRight < (angleThreshold / 360))
            {
                rightAttach.rightAttachPosition = altRightAttach.rightAttachPosition;
                rightAttach.rightAttachRotation = altRightAttach.rightAttachRotation;
            }
            else
            {
                rightAttach.rightAttachPosition = primaryRightAttach.rightAttachPosition;
                rightAttach.rightAttachRotation = primaryRightAttach.rightAttachRotation;
            }
        }
    }
}
