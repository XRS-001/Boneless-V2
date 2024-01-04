using UnityEngine;
using static GrabTwoAttach;

public class GrabDynamic : GrabTwoAttach
{
    [System.Serializable]
    public class DynamicSettings
    {
        [Tooltip("The local radius in which the attach point can go")]
        public float attachScale;
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
    }
    public DynamicSettings dynamicSettings;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        dynamicSettings.leftGrab = dynamicSettings.leftHand.GetComponent<GrabPhysics>();
        dynamicSettings.rightGrab = dynamicSettings.rightHand.GetComponent<GrabPhysics>();
    }
    // Update is called once per frame
    void Update()
    {
        if (dynamicSettings.leftGrab.isHovering && !dynamicSettings.leftGrab.isGrabbing)
        {
            //cast a ray that directs to the interactable and outputs the hitInfo
            GetComponent<BoxCollider>().Raycast(new Ray(dynamicSettings.leftHand.position, transform.position - dynamicSettings.leftHand.position), out RaycastHit hitInfo, float.PositiveInfinity);

            //set the leftAttachPosition to the hitPoint and add some offset
            leftAttach.leftAttachPosition = transform.InverseTransformPoint(hitInfo.point + (-dynamicSettings.leftHand.right / 20 * dynamicSettings.offset));
            //set the rotation to be the hands rotation extending the the normal
            leftAttach.leftAttachRotation = Quaternion.Lerp(dynamicSettings.leftHand.rotation, Quaternion.LookRotation(hitInfo.normal, dynamicSettings.leftHand.up) * Quaternion.Euler(0, 90, 0), 0.35f).eulerAngles;
        }
        if (dynamicSettings.rightGrab.isHovering && !dynamicSettings.rightGrab.isGrabbing)
        {
            //cast a ray that directs to the interactable and outputs the hitInfo
            GetComponent<BoxCollider>().Raycast(new Ray(dynamicSettings.rightHand.position, transform.position - dynamicSettings.rightHand.position), out RaycastHit hitInfo, float.PositiveInfinity);

            //set the rightAttachPosition to the hitPoint and add some offset
            rightAttach.rightAttachPosition = transform.InverseTransformPoint(hitInfo.point + (dynamicSettings.rightHand.right / 20 * dynamicSettings.offset));
            //set the rotation to be the hands rotation extending the the normal
            rightAttach.rightAttachRotation = Quaternion.Slerp(dynamicSettings.rightHand.rotation, Quaternion.LookRotation(hitInfo.normal, dynamicSettings.rightHand.up) * Quaternion.Euler(0, -90, 0), 0.35f).eulerAngles;
        }
    }
    
}
