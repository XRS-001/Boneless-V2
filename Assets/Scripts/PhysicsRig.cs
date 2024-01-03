using RootMotion.Demos;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRIKData
{
    public VRIKCalibrator.CalibrationData ikData;
}
public class PhysicsRig : MonoBehaviour
{
    public float height;
    public VRIK playerModel;
    private VRIKCalibrationBasic calibrator;
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
    public void ChangeScene(string scene)
    {
        SaveData();
        SceneManager.LoadScene(scene);
    }
    private void Start()
    {
        calibrator = GetComponent<VRIKCalibrationBasic>();
        LoadData();
        leftHandGrab = leftHandJoint.GetComponent<GrabPhysics>();
        rightHandGrab = rightHandJoint.GetComponent<GrabPhysics>();
    }
    private VRIKData vrikData = new VRIKData();
    void SaveData()
    {
        VRIKCalibrator.CalibrationData calibrationData = GetComponent<VRIKCalibrationBasic>().data;

        vrikData.ikData = calibrationData;

        string json = JsonUtility.ToJson(vrikData);

        PlayerPrefs.SetString("CalibrationData", json);
        PlayerPrefs.Save();
    }

    void LoadData()
    {
        string json = PlayerPrefs.GetString("CalibrationData", "");

        if (!string.IsNullOrEmpty(json))
        {
            vrikData = JsonUtility.FromJson<VRIKData>(json);

            VRIKCalibrator.CalibrationData loadedCalibrationData = vrikData.ikData;

            GetComponent<VRIKCalibrationBasic>().data = loadedCalibrationData;
            Debug.Log("<color=#00c04b> Loaded Calibration Data </color>");
        }
        else
        {
            Debug.Log("<color=#FF2400> No saved calibration data found. </color>");
        }
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
    // Update is called once per frame
    void FixedUpdate()
    {
        //calculate the player height
        height = 1.75f * calibrator.data.scale;
        leftHandJoint.targetPosition = CalculateWeight(leftHandJoint.targetPosition, leftHandPhysicsTarget.localPosition, leftHandGrab.connectedMass);
        leftHandJoint.targetRotation = leftHandPhysicsTarget.localRotation;

        rightHandJoint.targetPosition = CalculateWeight(rightHandJoint.targetPosition, rightHandPhysicsTarget.localPosition, rightHandGrab.connectedMass);
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
