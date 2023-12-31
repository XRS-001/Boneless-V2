using UnityEngine;

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
    }
    public DynamicSettings dynamicSettings;
    private void Start()
    {
        dynamicSettings.leftGrab = dynamicSettings.leftHand.GetComponent<GrabPhysics>();
        dynamicSettings.rightGrab = dynamicSettings.rightHand.GetComponent<GrabPhysics>();
    }
    // Update is called once per frame
    void Update()
    {
        if (dynamicSettings.leftGrab.isHovering)
        {
            leftAttach.leftAttachPosition = transform.InverseTransformPoint(colliders[0].ClosestPoint(dynamicSettings.leftHand.position));
            rightAttach.rightAttachPosition = transform.InverseTransformPoint(colliders[0].ClosestPoint(dynamicSettings.rightHand.position));
        }
        if(dynamicSettings.rightGrab.isHovering)
        {

            leftAttach.leftAttachRotation = dynamicSettings.leftHand.rotation.eulerAngles;
            rightAttach.rightAttachRotation = dynamicSettings.rightHand.rotation.eulerAngles;
        }
    }
}
