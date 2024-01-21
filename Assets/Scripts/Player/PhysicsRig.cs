using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;
[System.Serializable]
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
    public DetectCollisionJoint detectCollisionHandLeft;
    [Header("Right Hand")]
    public ConfigurableJoint rightHandJoint;
    public DetectCollisionJoint detectCollisionHandRight;
    private GrabPhysics rightHandGrab;
    public DetectCollisionJoint[] bodyDetectCollisions;
    [HideInInspector]
    public bool isBodyColliding;
    [System.Serializable]
    public class Joints
    {
        public Transform camera;
        [HideInInspector]
        public Camera physicalCameraComponent;
        public ConfigurableJoint headJoint;
        [HideInInspector]
        public DetectCollisionJoint detectCollisionHead;
        public Transform headTarget;
        public Transform headDriver;
        [HideInInspector]
        public Camera cameraComponent;

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
    private ContinuousMovementPhysics movement;
    [HideInInspector]
    public bool delayStart;
    private void Start()
    {
        movement = GetComponent<ContinuousMovementPhysics>();
        joints.cameraComponent = joints.headDriver.GetComponent<Camera>();
        joints.physicalCameraComponent = joints.camera.GetComponent<Camera>();
        joints.detectCollisionHead = joints.headJoint.GetComponent<DetectCollisionJoint>();
        StartCoroutine(DelayStart());
        leftHandGrab = leftHandJoint.GetComponent<GrabPhysics>();
        rightHandGrab = rightHandJoint.GetComponent<GrabPhysics>();
    }
    IEnumerator DelayStart()
    {
        delayStart = true;
        yield return new WaitForSeconds(1f);
        delayStart = false;
    }
    public Vector3 CalculateWeight(Vector3 currentPosition, Vector3 targetPosition, float weight, BaseGrab grab)
    {
        if (weight > 1)
        {
            if(grab.gameObject.layer != LayerMask.NameToLayer("Ragdoll"))
            {
                if (grab.isTwoHandGrabbing)
                {
                    //calculate the damping of the position to simulate weight
                    float dampingFactor = Mathf.Clamp(1 / (weight * 4), float.NegativeInfinity, 1);
                    return Vector3.Lerp(currentPosition, targetPosition, dampingFactor * 2);
                }
                else
                {
                    //calculate the damping of the position to simulate weight
                    float dampingFactor = Mathf.Clamp(1 / (weight * 4), float.NegativeInfinity, 1);
                    return Vector3.Lerp(currentPosition, targetPosition, dampingFactor);
                }
            }
            else
            {
                if (grab.isTwoHandGrabbing)
                {
                    //calculate the damping of the position to simulate weight
                    float dampingFactor = Mathf.Clamp(1 / (weight * 8), float.NegativeInfinity, 1);
                    return Vector3.Lerp(currentPosition, targetPosition, dampingFactor * 2);
                }
                else
                {
                    //calculate the damping of the position to simulate weight
                    float dampingFactor = Mathf.Clamp(1 / (weight * 4), float.NegativeInfinity, 1);
                    return Vector3.Lerp(currentPosition, targetPosition, dampingFactor);
                }
            }
        }
        else
        {
            return targetPosition;
        }
    }
    public Quaternion CalculateAngle(Transform transform, Quaternion currentAngle, Quaternion targetAngle, float weight)
    {
        if (weight > 1)
        {
            if(weight > 5)
            {
                //rotate the hands in the direction of looking downwards
                float dampingFactor = Mathf.Clamp(1 / (weight * 4), float.NegativeInfinity, 1);
                Vector3 eulerAngles = Quaternion.Slerp(currentAngle, Quaternion.Slerp(targetAngle, Quaternion.LookRotation(Vector3.down, transform.up) * Quaternion.Euler(110, 0, 90), 0.01f * weight), dampingFactor).eulerAngles;
                eulerAngles.y = Quaternion.Slerp(currentAngle, targetAngle, dampingFactor).eulerAngles.y;
                return Quaternion.Euler(eulerAngles);
            }
            else
            {
                //calculate the damping of the position to simulate weight
                float dampingFactor = Mathf.Clamp(1 / (weight * 4), float.NegativeInfinity, 1);
                return Quaternion.Slerp(currentAngle, targetAngle, dampingFactor);
            }
        }
        else
        {
            return targetAngle;
        }
    }
    public void ChangeLimit()
    {
        bool isPiercingLeft = false;
        Blade pierceLeft = null;
        if (leftHandGrab.isGrabbing)
        {
            pierceLeft = leftHandGrab.grab.GetComponent<Blade>();
        }
        if (pierceLeft)
        {
            if (pierceLeft.stabbed)
            {
                isPiercingLeft = true;
            }
        }
        if (!isPiercingLeft)
        {
            if (detectCollisionHandLeft.isColliding)
            {
                if (detectCollisionHandLeft.layerColliding != "Interactable" && detectCollisionHandLeft.layerColliding != "Ragdoll")
                {
                    movement.LimitPositionLeft();
                }
            }
            else
            {
                if (leftHandGrab.detectCollision)
                {
                    if (leftHandGrab.detectCollision.isColliding)
                    {
                        if (!leftHandGrab.isClimbing)
                        {
                            if (leftHandGrab.detectCollision.layerColliding != "Interactable" && leftHandGrab.detectCollision.layerColliding != "Ragdoll")
                            {
                                movement.LimitPositionLeft();
                            }
                        }
                    }
                }
            }
        }
        bool isPiercingRight = false;
        Blade pierceRight = null;
        if (rightHandGrab.isGrabbing)
        {
            pierceRight = rightHandGrab.grab.GetComponent<Blade>();
        }
        if (pierceRight)
        {
            if (pierceRight.stabbed)
            {
                isPiercingRight = true;
            }
        }
        if (!isPiercingRight)
        {
            if (detectCollisionHandRight.isColliding)
            {
                if (detectCollisionHandRight.layerColliding != "Interactable" && detectCollisionHandRight.layerColliding != "Ragdoll")
                {
                    //movement.LimitPositionRight();
                }
            }
            else
            {
                if (rightHandGrab.detectCollision)
                {
                    if (rightHandGrab.detectCollision.isColliding)
                    {
                        if (!rightHandGrab.isClimbing)
                        {
                            if (rightHandGrab.detectCollision.layerColliding != "Interactable" && rightHandGrab.detectCollision.layerColliding != "Ragdoll")
                            {
                                //movement.LimitPositionRight();
                            }
                        }
                    }
                }
            }
        }
    }
    public void IsBodyColliding(bool collisionEnter)
    {
        if(collisionEnter)
        {
            isBodyColliding = true;
        }
        else
        {
            isBodyColliding = false;
            foreach (DetectCollisionJoint joint in bodyDetectCollisions)
            {
                if (joint.isColliding)
                {
                    isBodyColliding = true;
                }
            }
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        ChangeLimit();
        joints.camera.position = joints.headJoint.transform.position;
        joints.camera.rotation = joints.headDriver.transform.rotation;

        if (joints.detectCollisionHead.isColliding)
        {
            joints.cameraComponent.enabled = false;
            joints.physicalCameraComponent.enabled = true;
        }
        else
        {
            joints.cameraComponent.enabled = true;
            joints.physicalCameraComponent.enabled = false;
        }

        leftHandJoint.targetPosition = CalculateWeight(leftHandJoint.targetPosition, leftHandPhysicsTarget.localPosition, leftHandGrab.connectedMass, leftHandGrab.grab);
        leftHandJoint.targetRotation = CalculateAngle(leftHandPhysicsTarget, leftHandJoint.targetRotation, leftHandPhysicsTarget.localRotation, leftHandGrab.connectedMass);

        rightHandJoint.targetPosition = CalculateWeight(rightHandJoint.targetPosition, rightHandPhysicsTarget.localPosition, rightHandGrab.connectedMass, rightHandGrab.grab);
        rightHandJoint.targetRotation = CalculateAngle(rightHandPhysicsTarget, rightHandJoint.targetRotation, rightHandPhysicsTarget.localRotation, rightHandGrab.connectedMass);

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
