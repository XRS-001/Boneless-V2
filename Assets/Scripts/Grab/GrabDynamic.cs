using System.Collections.Generic;
using UnityEngine;
using static GrabTwoAttach;
using static EnumDeclaration;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class GrabDynamic : GrabTwoAttach
{
    [System.Serializable]
    public class DynamicSettings
    {
        [HideInInspector]
        public Transform leftHand;
        [HideInInspector]
        public GrabPhysics leftGrab;
        [HideInInspector]
        public Transform rightHand;
        [HideInInspector]
        public GrabPhysics rightGrab;
        [Tooltip("The offset from the surface of the interactable")]
        public float offset;
        [Tooltip("The weight the hand is made to look at the normal (1 being 100%)")]
        public float angleWeight;
        //verify that the object is valid to be grabbed
        [HideInInspector]
        public bool isGrabbable = true;
    }
    public DynamicSettings dynamicSettings;
    private void Start()
    {
        if (!dynamicSettings.rightHand && !dynamicSettings.leftHand)
        {
            dynamicSettings.rightHand = GameObject.Find("GameManager").GetComponent<GameManager>().defaultRightHandTarget;
            dynamicSettings.leftHand = GameObject.Find("GameManager").GetComponent<GameManager>().defaultLeftHandTarget;
        }
        leftAttach.leftPose = GameObject.Find("GameManager").GetComponent<GameManager>().leftDynamicPose;
        rightAttach.rightPose = GameObject.Find("GameManager").GetComponent<GameManager>().rightDynamicPose;

        dynamicSettings.leftGrab = dynamicSettings.leftHand.GetComponent<GrabPhysics>(); dynamicSettings.rightGrab = dynamicSettings.rightHand.GetComponent<GrabPhysics>();
    }
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (isHovering && !dynamicSettings.leftGrab.isGrabbing)
        {
            if (colliders.Length > 1 || !GetComponent<Collider>())
            {
                Collider closestCollider = ClosestCollider(handTypeEnum.Left);
                if ((closestCollider.ClosestPoint(dynamicSettings.leftHand.position) - dynamicSettings.leftHand.position).normalized.magnitude > float.Epsilon && !float.IsNaN((closestCollider.ClosestPoint(dynamicSettings.leftHand.position) - dynamicSettings.leftHand.position).normalized.magnitude))
                {
                    closestCollider.Raycast(new Ray(dynamicSettings.leftHand.position, (closestCollider.ClosestPoint(dynamicSettings.leftHand.position) - dynamicSettings.leftHand.position).normalized), out RaycastHit hitInfo, float.PositiveInfinity);

                    if (hitInfo.collider)
                    {
                        dynamicSettings.isGrabbable = true;
                        leftAttach.leftAttachPosition = transform.InverseTransformPoint(closestCollider.ClosestPoint(dynamicSettings.leftHand.position)

                            - Quaternion.Lerp(dynamicSettings.leftHand.rotation, Quaternion.LookRotation(hitInfo.normal, -dynamicSettings.leftHand.right)
                            * Quaternion.Euler(180, 0, 90), dynamicSettings.angleWeight)
                            * -Vector3.down / 15f

                            + (Quaternion.Lerp(dynamicSettings.leftHand.rotation, Quaternion.LookRotation(hitInfo.normal, -dynamicSettings.leftHand.right)
                            * Quaternion.Euler(180, 0, 90), dynamicSettings.angleWeight)
                            * -Vector3.forward / 15f
                            * dynamicSettings.offset));

                        leftAttach.leftAttachRotation = Quaternion.Lerp(dynamicSettings.leftHand.rotation, Quaternion.LookRotation(hitInfo.normal, dynamicSettings.leftHand.right) * Quaternion.Euler(0, 180, 90), dynamicSettings.angleWeight).eulerAngles;
                    }
                    else
                    {
                        dynamicSettings.isGrabbable = false;
                    }
                }
            }
            else
            {
                if ((GetComponent<Collider>().ClosestPoint(dynamicSettings.leftHand.position) - dynamicSettings.leftHand.position).normalized.magnitude > float.Epsilon && !float.IsNaN((GetComponent<Collider>().ClosestPoint(dynamicSettings.leftHand.position) - dynamicSettings.leftHand.position).normalized.magnitude))
                {
                    //cast a ray that directs to the interactable and outputs the hitInfo
                    GetComponent<Collider>().Raycast(new Ray(dynamicSettings.leftHand.position, (GetComponent<Collider>().ClosestPoint(dynamicSettings.leftHand.position) - dynamicSettings.leftHand.position).normalized), out RaycastHit hitInfo, float.PositiveInfinity);
                    if (hitInfo.collider)
                    {
                        dynamicSettings.isGrabbable = true;
                        //set the rotation to be the hands rotation extending the the normal
                        leftAttach.leftAttachPosition = transform.InverseTransformPoint(hitInfo.point

                            - Quaternion.Lerp(dynamicSettings.leftHand.rotation, Quaternion.LookRotation(hitInfo.normal, -dynamicSettings.leftHand.right)
                            * Quaternion.Euler(180, 0, 90), dynamicSettings.angleWeight)
                            * -Vector3.down / 15f

                            + (Quaternion.Lerp(dynamicSettings.leftHand.rotation, Quaternion.LookRotation(hitInfo.normal, -dynamicSettings.leftHand.right)
                            * Quaternion.Euler(180, 0, 90), dynamicSettings.angleWeight)
                            * -Vector3.forward / 15f
                            * dynamicSettings.offset));

                        //set the leftAttachPosition to the hitPoint and add some offset
                        leftAttach.leftAttachRotation = Quaternion.Lerp(dynamicSettings.leftHand.rotation, Quaternion.LookRotation(hitInfo.normal, dynamicSettings.leftHand.right) * Quaternion.Euler(0, 180, 90), dynamicSettings.angleWeight).eulerAngles;
                    }
                    else
                    {
                        dynamicSettings.isGrabbable = false;
                    }
                }
            }
        }
        if (isHovering && !dynamicSettings.rightGrab.isGrabbing)
        {
            if (colliders.Length > 1 || !GetComponent<Collider>())
            {
                Collider closestCollider = ClosestCollider(handTypeEnum.Right);
                if ((closestCollider.ClosestPoint(dynamicSettings.rightHand.position) - dynamicSettings.rightHand.position).normalized.magnitude > float.Epsilon && !float.IsNaN((closestCollider.ClosestPoint(dynamicSettings.rightHand.position) - dynamicSettings.rightHand.position).normalized.magnitude))
                {
                    closestCollider.Raycast(new Ray(dynamicSettings.rightHand.position, (closestCollider.ClosestPoint(dynamicSettings.rightHand.position) - dynamicSettings.rightHand.position).normalized), out RaycastHit hitInfo, float.PositiveInfinity);

                    if (hitInfo.collider)
                    {
                        dynamicSettings.isGrabbable = true;
                        rightAttach.rightAttachPosition = transform.InverseTransformPoint(closestCollider.ClosestPoint(dynamicSettings.rightHand.position)
                            - Quaternion.Lerp(dynamicSettings.rightHand.rotation, Quaternion.LookRotation(hitInfo.normal, -dynamicSettings.rightHand.right)
                            * Quaternion.Euler(180, 0, 90), dynamicSettings.angleWeight)
                            * -Vector3.down / 15f

                            + (Quaternion.Lerp(dynamicSettings.rightHand.rotation, Quaternion.LookRotation(hitInfo.normal, -dynamicSettings.rightHand.right)
                            * Quaternion.Euler(180, 0, 90), dynamicSettings.angleWeight)
                            * -Vector3.forward / 15f * dynamicSettings.offset));
                        rightAttach.rightAttachRotation = Quaternion.Lerp(dynamicSettings.rightHand.rotation, Quaternion.LookRotation(hitInfo.normal, -dynamicSettings.rightHand.right) * Quaternion.Euler(180, 0, 90), dynamicSettings.angleWeight).eulerAngles;
                    }
                    else
                    {
                        dynamicSettings.isGrabbable = false;
                    }
                }
            }
            else
            {
                if ((GetComponent<Collider>().ClosestPoint(dynamicSettings.rightHand.position) - dynamicSettings.rightHand.position).normalized.magnitude > float.Epsilon && !float.IsNaN((GetComponent<Collider>().ClosestPoint(dynamicSettings.rightHand.position) - dynamicSettings.rightHand.position).normalized.magnitude))
                {
                    //cast a ray that directs to the interactable and outputs the hitInfo
                    GetComponent<Collider>().Raycast(new Ray(dynamicSettings.rightHand.position, (GetComponent<Collider>().ClosestPoint(dynamicSettings.rightHand.position) - dynamicSettings.rightHand.position).normalized), out RaycastHit hitInfo, float.PositiveInfinity);
                    if (hitInfo.collider)
                    {
                        dynamicSettings.isGrabbable = true;
                        rightAttach.rightAttachPosition = transform.InverseTransformPoint(hitInfo.point
                            - Quaternion.Lerp(dynamicSettings.rightHand.rotation, Quaternion.LookRotation(hitInfo.normal, -dynamicSettings.rightHand.right)
                            * Quaternion.Euler(180, 0, 90), dynamicSettings.angleWeight)
                            * -Vector3.down / 15f

                            + (Quaternion.Lerp(dynamicSettings.rightHand.rotation, Quaternion.LookRotation(hitInfo.normal, -dynamicSettings.rightHand.right)
                            * Quaternion.Euler(180, 0, 90), dynamicSettings.angleWeight)
                            * -Vector3.forward / 15f * dynamicSettings.offset));
                        rightAttach.rightAttachRotation = Quaternion.Lerp(dynamicSettings.rightHand.rotation, Quaternion.LookRotation(hitInfo.normal, -dynamicSettings.rightHand.right) * Quaternion.Euler(180, 0, 90), dynamicSettings.angleWeight).eulerAngles;
                    }
                    else
                    {
                        dynamicSettings.isGrabbable = false;
                    }
                }
            }
        }
    }
    public Collider ClosestCollider(handTypeEnum handType)
    {
        Collider closestCollider = colliders[0];
        Vector3 closestPosition = Vector3.zero;
        if (handType == handTypeEnum.Left)
        {
            closestPosition = colliders[0].ClosestPoint(dynamicSettings.leftHand.position);
        }
        else
        {
            closestPosition = colliders[0].ClosestPoint(dynamicSettings.rightHand.position);
        }
        float closestDistance;
        if (handType == handTypeEnum.Left)
        {
            closestDistance = Vector3.Distance(dynamicSettings.leftHand.position, closestPosition);
        }
        else
        {
            closestDistance = Vector3.Distance(dynamicSettings.rightHand.position, closestPosition);
        }
        // Loop through all positions and find the closest one
        for (int i = 0; i < colliders.Length; i++)
        {
            float distance;
            if (handType == handTypeEnum.Left)
            {
                distance = Vector3.Distance(dynamicSettings.leftHand.position, colliders[i].ClosestPoint(dynamicSettings.leftHand.position));
            }
            else
            {
                distance = Vector3.Distance(dynamicSettings.rightHand.position, colliders[i].ClosestPoint(dynamicSettings.rightHand.position));
            }

            if (distance < closestDistance)
            {
                closestCollider = colliders[i];
                closestDistance = distance;
            }
        }
        return closestCollider;
    }
}
