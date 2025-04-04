﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace RootMotion.Demos
{
    public class VRIKCalibrationBasic : MonoBehaviour
    {
        public InputActionProperty calibrateHeight;
        [Tooltip("The VRIK component.")] public VRIK ik;

        [Header("Head")]
        [Tooltip("HMD.")] public Transform centerEyeAnchor;
        [Tooltip("Position offset of the camera from the head bone (root space).")] public Vector3 headAnchorPositionOffset;
        [Tooltip("Rotation offset of the camera from the head bone (root space).")] public Vector3 headAnchorRotationOffset;

        [Header("Hands")]
        [Tooltip("Left Hand Controller")] public Transform leftHandAnchor;
        [Tooltip("Right Hand Controller")] public Transform rightHandAnchor;
        [Tooltip("Position offset of the hand controller from the hand bone (controller space).")] public Vector3 handAnchorPositionOffset;
        [Tooltip("Rotation offset of the hand controller from the hand bone (controller space).")] public Vector3 handAnchorRotationOffset;

        [Header("Scale")]
        [Tooltip("Multiplies the scale of the root.")] public float scaleMlp = 1f;

        [Header("Data stored by Calibration")]
        public VRIKCalibrator.CalibrationData data = new VRIKCalibrator.CalibrationData();

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.C) || calibrateHeight.action.WasPressedThisFrame() && SceneManager.GetActiveScene().name == "Start Environment")
            {
                data = VRIKCalibrator.Calibrate(ik, centerEyeAnchor, leftHandAnchor, rightHandAnchor, headAnchorPositionOffset, headAnchorRotationOffset, handAnchorPositionOffset, handAnchorRotationOffset, scaleMlp);
            }

            /*
            * calling Calibrate with settings will return a VRIKCalibrator.CalibrationData, which can be used to calibrate that same character again exactly the same in another scene (just pass data instead of settings), 
            * without being dependent on the pose of the player at calibration time.
            * Calibration data still depends on bone orientations though, so the data is valid only for the character that it was calibrated to or characters with identical bone structures.
            * If you wish to use more than one character, it would be best to calibrate them all at once and store the CalibrationData for each one.
            * */

            // Recalibrates avatar scale only. Can be called only if the avatar has been calibrated already.
            if (Input.GetKeyDown(KeyCode.V))
            {
                if (data.scale == 0f)
                {
                    Debug.LogError("Avatar needs to be calibrated before RecalibrateScale is called.");
                }
                VRIKCalibrator.RecalibrateScale(ik, data, scaleMlp);
            }
        }
        private void Start()
        {
            StartCoroutine(DelayStart());
        }
        IEnumerator DelayStart()
        {
            yield return new WaitForSeconds(0.1f);

            if (data.scale == 0f)
            {
                Debug.LogError("No Calibration Data to calibrate to, please calibrate with 'C' first.");
            }
            else
            {
                VRIKCalibrator.Calibrate(ik, data, centerEyeAnchor, null, leftHandAnchor, rightHandAnchor);
            }
        }
    }
}
