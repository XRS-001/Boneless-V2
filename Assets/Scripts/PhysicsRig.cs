using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsRig : MonoBehaviour
{
    public Transform playerHead;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    public ConfigurableJoint leftHandJoint;
    public ConfigurableJoint rightHandJoint;

    public ConfigurableJoint rightJoint;
    public Collider[] rightJointColliders;

    public ConfigurableJoint leftJoint;
    public Collider[] leftJointColliders;

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

    // Update is called once per frame
    void FixedUpdate()
    {
        leftHandJoint.targetPosition = leftHandTarget.localPosition;
        leftHandJoint.targetRotation = leftHandTarget.localRotation;

        rightHandJoint.targetPosition = rightHandTarget.localPosition;
        rightHandJoint.targetRotation = rightHandTarget.localRotation;

        headJoint.targetPosition = headTarget.localPosition;
        headJoint.targetRotation = headTarget.localRotation;

        chestJoint.targetPosition = chestTarget.localPosition;
        chestJoint.targetRotation = chestTarget.localRotation;

        rightArmJoint.targetPosition = rightArmTarget.localPosition;
        rightArmJoint.targetRotation = rightArmTarget.localRotation;

        rightForearmJoint.targetPosition = rightForearmTarget.localPosition;
        rightForearmJoint.targetRotation = rightForearmTarget.localRotation;

        leftArmJoint.targetPosition = leftArmTarget.localPosition;
        leftArmJoint.targetRotation = leftArmTarget.localRotation;

        leftForearmJoint.targetPosition = leftForearmTarget.localPosition;
        leftForearmJoint.targetRotation = leftForearmTarget.localRotation;

        rightThighJoint.targetPosition = rightThighTarget.localPosition;
        rightThighJoint.targetRotation = rightThighTarget.localRotation;

        rightLegJoint.targetPosition = rightLegTarget.localPosition;
        rightLegJoint.targetRotation = rightLegTarget.localRotation;

        leftThighJoint.targetPosition = leftThighTarget.localPosition;
        leftThighJoint.targetRotation = leftThighTarget.localRotation;

        leftLegJoint.targetPosition = leftLegTarget.localPosition;
        leftLegJoint.targetRotation = leftLegTarget.localRotation;
    }
}
