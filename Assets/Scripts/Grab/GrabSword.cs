using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumDeclaration;
public class GrabSword : GrabTwoAttach
{
    [Tooltip("Defines the Y axis the hand will grab along")]
    public upDirection handleDirection;
    [System.Serializable]
    public class DynamicSettings
    {
        public Transform leftHand;
        public Transform rightHand;
        public Vector3 handlePosition;
        public float handleLength;
    }
    public DynamicSettings dynamicSettings;
    private void Start()
    {
        if (!dynamicSettings.rightHand && !dynamicSettings.leftHand)
        {
            dynamicSettings.rightHand = GameObject.Find("GameManager").GetComponent<GameManager>().defaultRightHandTarget;
            dynamicSettings.leftHand = GameObject.Find("GameManager").GetComponent<GameManager>().defaultLeftHandTarget;
        }
    }
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (isHovering)
        {
            Vector3 positionLeft = transform.InverseTransformPoint(dynamicSettings.leftHand.position);
            switch (handleDirection)
            {
                case upDirection.up:
                    positionLeft.x = dynamicSettings.handlePosition.x;
                    positionLeft.y = Mathf.Clamp(positionLeft.y, -dynamicSettings.handleLength + dynamicSettings.handlePosition.y, dynamicSettings.handleLength + dynamicSettings.handlePosition.y);
                    positionLeft.z = dynamicSettings.handlePosition.z;
                    break;
                case upDirection.right:
                    positionLeft.y = dynamicSettings.handlePosition.y;
                    positionLeft.x = Mathf.Clamp(positionLeft.x, -dynamicSettings.handleLength + dynamicSettings.handlePosition.x, dynamicSettings.handleLength + dynamicSettings.handlePosition.x);
                    positionLeft.z = dynamicSettings.handlePosition.z;
                    break;
                case upDirection.forward:
                    positionLeft.x = dynamicSettings.handlePosition.x;
                    positionLeft.z = Mathf.Clamp(positionLeft.z, -dynamicSettings.handleLength + dynamicSettings.handlePosition.z, dynamicSettings.handleLength + dynamicSettings.handlePosition.z);
                    positionLeft.y = dynamicSettings.handlePosition.y;
                    break;
            }
            leftAttach.leftAttachPosition = positionLeft;
        }
        else
        {
            leftAttach.leftAttachPosition = dynamicSettings.handlePosition;
        }
        if (isHovering)
        {
            Vector3 positionRight = transform.InverseTransformPoint(dynamicSettings.rightHand.position);
            switch (handleDirection)
            {
                case upDirection.up:
                    positionRight.x = -dynamicSettings.handlePosition.x;
                    positionRight.y = Mathf.Clamp(positionRight.y, -dynamicSettings.handleLength + dynamicSettings.handlePosition.y, dynamicSettings.handleLength + dynamicSettings.handlePosition.y);
                    positionRight.z = dynamicSettings.handlePosition.z;
                    break;
                case upDirection.right:
                    positionRight.y = dynamicSettings.handlePosition.y;
                    positionRight.x = Mathf.Clamp(positionRight.x, -dynamicSettings.handleLength + dynamicSettings.handlePosition.x, dynamicSettings.handleLength + dynamicSettings.handlePosition.x);
                    positionRight.z = -dynamicSettings.handlePosition.z;
                    break;
                case upDirection.forward:
                    positionRight.x = -dynamicSettings.handlePosition.x;
                    positionRight.z = Mathf.Clamp(positionRight.z, -dynamicSettings.handleLength + dynamicSettings.handlePosition.z, dynamicSettings.handleLength + dynamicSettings.handlePosition.z);
                    positionRight.y = dynamicSettings.handlePosition.y;
                    break;
            }
            rightAttach.rightAttachPosition = positionRight;
        }
        else
        {
            rightAttach.rightAttachPosition = new Vector3(-dynamicSettings.handlePosition.x, dynamicSettings.handlePosition.y, dynamicSettings.handlePosition.z);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        switch (handleDirection)
        {
            case upDirection.forward:
                Gizmos.DrawLine(transform.TransformPoint(dynamicSettings.handlePosition - (Vector3.forward * dynamicSettings.handleLength)), transform.TransformPoint(dynamicSettings.handlePosition + (Vector3.forward * dynamicSettings.handleLength)));
                break;
            case upDirection.up:
                Gizmos.DrawLine(transform.TransformPoint(dynamicSettings.handlePosition - (Vector3.up * dynamicSettings.handleLength)), transform.TransformPoint(dynamicSettings.handlePosition + (Vector3.up * dynamicSettings.handleLength)));
                break;
            case upDirection.right:
                Gizmos.DrawLine(transform.TransformPoint(dynamicSettings.handlePosition - (Vector3.right * dynamicSettings.handleLength)), transform.TransformPoint(dynamicSettings.handlePosition + (Vector3.right * dynamicSettings.handleLength)));
                break;
        }
    }
}
