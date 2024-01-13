using System.Collections.Generic;
using UnityEngine;
using static GrabTwoAttach;
using static EnumDeclaration;
using Unity.VisualScripting;

public class GrabDynamic : GrabTwoAttach
{
    [System.Serializable]
    public class DynamicSettings
    {
        [Tooltip("The transform used to calculate the leftAttach (must be the physical presence of the left hand)")]
        public Transform leftHand;
        [HideInInspector]
        public GrabPhysics leftGrab;
        [Tooltip("The transform used to calculate the rightAttach (must be the physical presence of the right hand)")]
        public Transform rightHand;
        [HideInInspector]
        public GrabPhysics rightGrab;
        [Tooltip("The offset from the surface of the interactable")]
        public float offset;
        [Tooltip("The weight the hand is made to look at the normal (1 being 100%)")]
        public float angleWeight;
    }
    public DynamicSettings dynamicSettings;
    private void Start()
    {
        dynamicSettings.leftGrab = dynamicSettings.leftHand.GetComponent<GrabPhysics>();
        dynamicSettings.rightGrab = dynamicSettings.rightHand.GetComponent<GrabPhysics>();
        //set the rb because of overriding of the baseGrab start function
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, dynamicSettings.leftHand.position) < dynamicSettings.leftGrab.calculationDistance)
        {
            if (colliders.Length > 1 || !GetComponent<Collider>())
            {
                ClosestCollider(handTypeEnum.Left).Raycast(new Ray(dynamicSettings.leftHand.position, ClosestCollider(handTypeEnum.Left).transform.position - dynamicSettings.leftHand.position), out RaycastHit hitInfo, float.PositiveInfinity);

                leftAttach.leftAttachPosition = transform.InverseTransformPoint(hitInfo.point + (-dynamicSettings.leftHand.right / 20 * dynamicSettings.offset));
                leftAttach.leftAttachRotation = Quaternion.Lerp(dynamicSettings.leftHand.rotation, Quaternion.LookRotation(hitInfo.normal, dynamicSettings.leftHand.up) * Quaternion.Euler(0, 90, 0), dynamicSettings.angleWeight).eulerAngles;
            }
            else
            {
                //cast a ray that directs to the interactable and outputs the hitInfo
                GetComponent<Collider>().Raycast(new Ray(dynamicSettings.leftHand.position, transform.position - dynamicSettings.leftHand.position), out RaycastHit hitInfo, float.PositiveInfinity);

                //set the leftAttachPosition to the hitPoint and add some offset
                leftAttach.leftAttachPosition = transform.InverseTransformPoint(hitInfo.point + (-dynamicSettings.leftHand.right / 20 * dynamicSettings.offset));
                //set the rotation to be the hands rotation extending the the normal
                leftAttach.leftAttachRotation = Quaternion.Lerp(dynamicSettings.leftHand.rotation, Quaternion.LookRotation(hitInfo.normal, dynamicSettings.leftHand.up) * Quaternion.Euler(0, 90, 0), dynamicSettings.angleWeight).eulerAngles;
            }
        }
        if (Vector3.Distance(transform.position, dynamicSettings.rightHand.position) < dynamicSettings.rightGrab.calculationDistance)
        {
            if (colliders.Length > 1 || !GetComponent<Collider>())
            {
                ClosestCollider(handTypeEnum.Right).Raycast(new Ray(dynamicSettings.rightHand.position, ClosestCollider(handTypeEnum.Right).transform.position - dynamicSettings.rightHand.position), out RaycastHit hitInfo, float.PositiveInfinity);

                rightAttach.rightAttachPosition = transform.InverseTransformPoint(hitInfo.point + (dynamicSettings.rightHand.right / 20 * dynamicSettings.offset));
                rightAttach.rightAttachRotation = Quaternion.Lerp(dynamicSettings.rightHand.rotation, Quaternion.LookRotation(hitInfo.normal, dynamicSettings.rightHand.up) * Quaternion.Euler(0, -90, 0), dynamicSettings.angleWeight).eulerAngles;
            }
            else
            {
                //cast a ray that directs to the interactable and outputs the hitInfo
                GetComponent<Collider>().Raycast(new Ray(dynamicSettings.rightHand.position, transform.position - dynamicSettings.rightHand.position), out RaycastHit hitInfo, float.PositiveInfinity);

                //set the leftAttachPosition to the hitPoint and add some offset
                rightAttach.rightAttachPosition = transform.InverseTransformPoint(hitInfo.point + (dynamicSettings.rightHand.right / 20 * dynamicSettings.offset));
                //set the rotation to be the hands rotation extending the the normal
                rightAttach.rightAttachRotation = Quaternion.Lerp(dynamicSettings.rightHand.rotation, Quaternion.LookRotation(hitInfo.normal, dynamicSettings.rightHand.up) * Quaternion.Euler(0, -90, 0), dynamicSettings.angleWeight).eulerAngles;
            }
        }
    }
    public Collider ClosestCollider(handTypeEnum handType)
    {
        Collider closestCollider = colliders[0];
        Vector3 closestPosition = colliders[0].transform.position;
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
        for (int i = 1; i < colliders.Length; i++)
        {
            float distance;
            if (handType == handTypeEnum.Left)
            {
                distance = Vector3.Distance(dynamicSettings.leftHand.position, colliders[i].transform.position);
            }
            else
            {
                distance = Vector3.Distance(dynamicSettings.rightHand.position, colliders[i].transform.position);
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
