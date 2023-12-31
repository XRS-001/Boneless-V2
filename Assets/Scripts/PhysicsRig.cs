using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsRig : MonoBehaviour
{
    [Tooltip("The players real world height in meters")]
    public float height;
    public VRIK playerModel;
    public VRIK followIk;

    public Transform leftHandPhysicsTarget;
    public Transform rightHandPhysicsTarget;

    public ConfigurableJoint leftHandJoint;
    private GrabPhysics leftHandGrab;
    public ConfigurableJoint rightHandJoint;
    private GrabPhysics rightHandGrab;

    [System.Serializable]
    public class Joints
    {
        public ConfigurableJoint headJoint;
        public Transform headTarget;

        public ConfigurableJoint chestJoint;
        public Transform chestTarget;

        public ConfigurableJoint rightArmJoint;
        public Transform rightArmTarget;

        public ConfigurableJoint rightForearmJoint;
        public Transform rightForearmTarget;

        public ConfigurableJoint leftArmJoint;
        public Transform leftArmTarget;

        public ConfigurableJoint leftForearmJoint;
        public Transform leftForearmTarget;

        public ConfigurableJoint rightThighJoint;
        public Transform rightThighTarget;

        public ConfigurableJoint rightLegJoint;
        public Transform rightLegTarget;

        public ConfigurableJoint leftThighJoint;
        public Transform leftThighTarget;

        public ConfigurableJoint leftLegJoint;
        public Transform leftLegTarget;
    }
    public Joints joints;
    private void Start()
    {
        //calculate IK scale based on real world height
        playerModel.solver.scale /= 1.75f / height;
        followIk.solver.scale /= 1.75f / height;

        leftHandGrab = leftHandJoint.GetComponent<GrabPhysics>();
        rightHandGrab = rightHandJoint.GetComponent<GrabPhysics>();
    }
    public Vector3 CalculateWeight(Vector3 currentPosition, Vector3 targetPosition, float weight)
    {
        if (weight > 1)
        {
            //calculate the damping of the position to simulate weight
            float dampingFactor = Mathf.Clamp(1 / (weight * 4), float.NegativeInfinity, 1);
            return Vector3.Lerp(currentPosition, targetPosition, dampingFactor);
        }
        else
        {
            return targetPosition;
        }
    }
    public Vector3 CalculateYOffset(float weight)
    {
        Vector3 offset = new Vector3(0, 0, 0);
        offset.y -= weight / 3;
        return offset;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        leftHandJoint.targetPosition = CalculateWeight(leftHandJoint.targetPosition, leftHandPhysicsTarget.localPosition, leftHandGrab.connectedMass);
        leftHandJoint.targetVelocity = CalculateYOffset(leftHandGrab.connectedMass);
        leftHandJoint.targetRotation = leftHandPhysicsTarget.localRotation;

        rightHandJoint.targetPosition = CalculateWeight(rightHandJoint.targetPosition, rightHandPhysicsTarget.localPosition, rightHandGrab.connectedMass);
        rightHandJoint.targetVelocity = CalculateYOffset(rightHandGrab.connectedMass);
        rightHandJoint.targetRotation = rightHandPhysicsTarget.localRotation;

        joints.headJoint.targetPosition = joints.headTarget.localPosition;
        joints.headJoint.targetRotation = joints.headTarget.localRotation;

        joints.chestJoint.targetPosition = joints.chestTarget.localPosition;
        joints.chestJoint.targetRotation = joints.chestTarget.localRotation;

        joints.rightArmJoint.targetPosition = joints.rightArmTarget.localPosition;
        joints.rightArmJoint.targetRotation = joints.rightArmTarget.localRotation;

        joints.rightForearmJoint.targetPosition = joints.rightForearmTarget.localPosition;
        joints.rightForearmJoint.targetRotation = joints.rightForearmTarget.localRotation;

        joints.leftArmJoint.targetPosition = joints.leftArmTarget.localPosition;
        joints.leftArmJoint.targetRotation = joints.leftArmTarget.localRotation;

        joints.leftForearmJoint.targetPosition = joints.leftForearmTarget.localPosition;
        joints.leftForearmJoint.targetRotation = joints.leftForearmTarget.localRotation;

        joints.rightThighJoint.targetPosition = joints.rightThighTarget.localPosition;
        joints.rightThighJoint.targetRotation = joints.rightThighTarget.localRotation;

        joints.rightLegJoint.targetPosition = joints.rightLegTarget.localPosition;
        joints.rightLegJoint.targetRotation = joints.rightLegTarget.localRotation;

        joints.leftThighJoint.targetPosition = joints.leftThighTarget.localPosition;
        joints.leftThighJoint.targetRotation = joints.leftThighTarget.localRotation;

        joints.leftLegJoint.targetPosition = joints.leftLegTarget.localPosition;
        joints.leftLegJoint.targetRotation = joints.leftLegTarget.localRotation;
    }
}
