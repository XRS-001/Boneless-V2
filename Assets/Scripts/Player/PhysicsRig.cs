using RootMotion.Demos;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[System.Serializable]
public class JointDriveNew
{
    public float postionSpring;
    public float positionDamper;
}
public class VRIKData
{
    public VRIKCalibrator.CalibrationData ikData;
}
public class PhysicsRig : MonoBehaviour
{
    public Rigidbody bodyRb;
    public VRIK playerModel;
    public Transform leftHandPhysicsTarget;
    public Transform rightHandPhysicsTarget;
    [Header("Left Hand")]
    public ConfigurableJoint leftHandJoint;
    private GrabPhysics leftHandGrab;
    public JointDriveNew jointDriveCollidingLeft;
    public JointDrive jointLeftStart;
    public DetectCollisionJoint detectCollisionHandLeft;
    [Header("Right Hand")]
    public ConfigurableJoint rightHandJoint;
    public JointDriveNew jointDriveCollidingRight;
    public JointDrive jointRightStart;
    public DetectCollisionJoint detectCollisionHandRight;
    private GrabPhysics rightHandGrab;
    [System.Serializable]
    public class Joints
    {
        public Transform camera;
        public ConfigurableJoint headJoint;
        [HideInInspector]
        public DetectCollisionJoint detectCollisionHead;
        public Transform headTarget;
        public Transform headDriver;

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
        joints.detectCollisionHead = joints.headJoint.GetComponent<DetectCollisionJoint>();
        StartCoroutine(DelayStart());
        leftHandGrab = leftHandJoint.GetComponent<GrabPhysics>();
        rightHandGrab = rightHandJoint.GetComponent<GrabPhysics>();

        jointLeftStart = leftHandJoint.xDrive;
        jointRightStart = rightHandJoint.xDrive;
    }
    IEnumerator DelayStart()
    {
        GetComponentInChildren<Rigidbody>().isKinematic = true;
        yield return new WaitForSeconds(1f);
        GetComponentInChildren<Rigidbody>().isKinematic = false;
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
    public Quaternion CalculateAngle(Quaternion currentAngle, Quaternion targetAngle, float weight)
    {
        if (weight > 1)
        {
            //calculate the damping of the position to simulate weight
            float dampingFactor = Mathf.Clamp(1 / (weight * 4), float.NegativeInfinity, 1);
            return Quaternion.Slerp(currentAngle, targetAngle, dampingFactor);
        }
        else
        {
            return targetAngle;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(detectCollisionHandLeft.isColliding)
        {
            JointDrive newDrive = leftHandJoint.xDrive;
            newDrive.positionDamper = jointDriveCollidingLeft.positionDamper;
            newDrive.positionSpring = jointDriveCollidingLeft.postionSpring;

            leftHandJoint.xDrive = newDrive;
            leftHandJoint.yDrive = newDrive;
            leftHandJoint.zDrive = newDrive;
            leftHandJoint.slerpDrive = newDrive;
        }
        else
        {
            if (leftHandGrab.detectCollision)
            {
                if (leftHandGrab.detectCollision.isColliding)
                {
                    JointDrive newDrive = leftHandJoint.xDrive;
                    newDrive.positionDamper = jointDriveCollidingLeft.positionDamper;
                    newDrive.positionSpring = jointDriveCollidingLeft.postionSpring;

                    leftHandJoint.xDrive = newDrive;
                    leftHandJoint.yDrive = newDrive;
                    leftHandJoint.zDrive = newDrive;
                    leftHandJoint.slerpDrive = newDrive;
                }
                else
                {
                    leftHandJoint.xDrive = jointLeftStart;
                    leftHandJoint.yDrive = jointLeftStart;
                    leftHandJoint.zDrive = jointLeftStart;
                    leftHandJoint.slerpDrive = jointLeftStart;
                }
            }
            else
            {
                leftHandJoint.xDrive = jointLeftStart;
                leftHandJoint.yDrive = jointLeftStart;
                leftHandJoint.zDrive = jointLeftStart;
                leftHandJoint.slerpDrive = jointLeftStart;
            }
        }

        if (detectCollisionHandRight.isColliding)
        {
            JointDrive newDrive = rightHandJoint.xDrive;
            newDrive.positionDamper = jointDriveCollidingRight.positionDamper;
            newDrive.positionSpring = jointDriveCollidingRight.postionSpring;

            rightHandJoint.xDrive = newDrive;
            rightHandJoint.yDrive = newDrive;
            rightHandJoint.zDrive = newDrive;
            rightHandJoint.slerpDrive = newDrive;
        }
        else
        {
            if (rightHandGrab.detectCollision)
            {
                if (rightHandGrab.detectCollision.isColliding)
                {
                    JointDrive newDrive = rightHandJoint.xDrive;
                    newDrive.positionDamper = jointDriveCollidingRight.positionDamper;
                    newDrive.positionSpring = jointDriveCollidingRight.postionSpring;

                    rightHandJoint.xDrive = newDrive;
                    rightHandJoint.yDrive = newDrive;
                    rightHandJoint.zDrive = newDrive;
                    rightHandJoint.slerpDrive = newDrive;
                }
                else
                {
                    rightHandJoint.xDrive = jointRightStart;
                    rightHandJoint.yDrive = jointRightStart;
                    rightHandJoint.zDrive = jointRightStart;
                    rightHandJoint.slerpDrive = jointRightStart;
                }
            }
            else
            {
                rightHandJoint.xDrive = jointRightStart;
                rightHandJoint.yDrive = jointRightStart;
                rightHandJoint.zDrive = jointRightStart;
                rightHandJoint.slerpDrive = jointRightStart;
            }
        }
        if (joints.detectCollisionHead.isColliding)
        {
            Vector3 newPosition = joints.camera.position;
            newPosition.x = joints.headTarget.transform.position.x;
            newPosition.y = joints.headJoint.transform.position.y;
            newPosition.z = joints.headTarget.transform.position.z;
            joints.camera.position = newPosition;
        }
        else
        {
            joints.camera.position = joints.headDriver.position;
        }
        joints.camera.rotation = joints.headDriver.transform.rotation;

        leftHandJoint.targetPosition = CalculateWeight(leftHandJoint.targetPosition, leftHandPhysicsTarget.localPosition, leftHandGrab.connectedMass);
        leftHandJoint.targetRotation = CalculateAngle(leftHandJoint.targetRotation, leftHandPhysicsTarget.localRotation, leftHandGrab.connectedMass);

        rightHandJoint.targetPosition = CalculateWeight(rightHandJoint.targetPosition, rightHandPhysicsTarget.localPosition, rightHandGrab.connectedMass);
        rightHandJoint.targetRotation = CalculateAngle(rightHandJoint.targetRotation, rightHandPhysicsTarget.localRotation, rightHandGrab.connectedMass);

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
