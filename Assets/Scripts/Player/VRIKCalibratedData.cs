using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class VRIKCalibratedData : MonoBehaviour
{
    public InputActionProperty calibrateHeight;
    public VRIK ik;

    public Transform centerEyeAnchor;
    public Vector3 headAnchorPositionOffset;
    public Vector3 headAnchorRotationOffset;

    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    public Vector3 handAnchorPositionOffset;
    public Vector3 handAnchorRotationOffset;

    public float scaleMlp = 1f;

    public VRIKCalibrator.CalibrationData data = new VRIKCalibrator.CalibrationData();

    private void LateUpdate()
    {
        if (calibrateHeight.action.WasPressedThisFrame() && SceneManager.GetActiveScene().name == "StartEnvironment")
        {
            data = VRIKCalibrator.Calibrate(ik, centerEyeAnchor, leftHandAnchor, rightHandAnchor, headAnchorPositionOffset, headAnchorRotationOffset, handAnchorPositionOffset, handAnchorRotationOffset, scaleMlp);
        }
    }
    private void Start()
    {
        StartCoroutine(DelayStart());
    }
    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(0.1f);

        if(data.scale > 0f)
        {
            VRIKCalibrator.Calibrate(ik, data, centerEyeAnchor, null, leftHandAnchor, rightHandAnchor);
        }
    }
}
