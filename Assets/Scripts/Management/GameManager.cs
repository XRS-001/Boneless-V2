using RootMotion.Demos;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class VRIKData
{
    public VRIKCalibrator.CalibrationData ikData;
}
public class GameManager : MonoBehaviour
{    
    [Tooltip("The opaque black backround that loses opacity on start")]
    public Image blurImage;
    public float height;
    public Camera externalCamera;
    private Quaternion startRotation;
    private Vector3 startPosition;
    public Transform player;
    public VRIKCalibratedData calibrator;
    [Tooltip("The \"done\" button at the start after calculating height (will be null if not in start scene)")]
    public GameObject sceneChangeButton;
    private VRIKData vrikData = new VRIKData();
    private AudioSource audioSource;
    public AudioClip uiClickSound;
    private bool altCameraFollow;
    private void Start()
    {
        startPosition = externalCamera.transform.position;
        startRotation = externalCamera.transform.rotation;

        Application.targetFrameRate = 120;
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (externalCamera)
        {
            if (altCameraFollow)
            {
                Quaternion previousRotation = externalCamera.transform.rotation;
                externalCamera.transform.LookAt(new Vector3(player.position.x, height - 0.5f, player.position.z));
                Quaternion targetRotation = externalCamera.transform.rotation;

                externalCamera.transform.rotation = Quaternion.Slerp(previousRotation, targetRotation, 0.05f);
                externalCamera.transform.position = Vector3.Lerp(externalCamera.transform.position, new Vector3(player.position.x, height, player.position.z) + Vector3.forward * 2, 0.05f);
            }
            else
            {
                externalCamera.transform.position = Vector3.Lerp(externalCamera.transform.position, startPosition, 0.05f);
                externalCamera.transform.rotation = Quaternion.Slerp(externalCamera.transform.rotation, startRotation, 0.05f);
            }
        }
        if (sceneChangeButton)
        {
            if (height == 0)
            {
                sceneChangeButton.SetActive(false);
            }
            else
            {
                sceneChangeButton.SetActive(true);
            }
        }
        //calculate the player height
        if (calibrator.data.scale != 0)
        {
            height = 1.75f * calibrator.data.scale;
        }
    }
    public void CameraFollow()
    {
        if (altCameraFollow)
        {
            altCameraFollow = false;
        }
        else
        {
            altCameraFollow = true;
        }
    }
    public void ChangeCamera()
    {
        if (externalCamera)
        {
            if (externalCamera.enabled)
            {
                externalCamera.enabled = false;
            }
            else
            {
                externalCamera.enabled = true;
            }
        }
    }
    void SaveData()
    {
        VRIKCalibrator.CalibrationData calibrationData = calibrator.data;

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

            calibrator.data = loadedCalibrationData;
            Debug.Log("<color=#00c04b> Loaded Calibration Data </color>");
        }
        else
        {
            Debug.Log("<color=#FF2400> No saved calibration data found. </color>");
        }
    }
    public void PlayUISound()
    {
        //make the volume independent
        audioSource.PlayOneShot(uiClickSound, 1 / audioSource.volume);
    }
    private void OnEnable()
    {
        //fade out the colour of the fade backround
        Color newColor = blurImage.color;
        newColor.a = 1;
        blurImage.color = newColor;
        blurImage.CrossFadeAlpha(0, 3, false);
        LoadData();
    }
    public void ChangeScene(string scene)
    {
        SaveData();
        SceneManager.LoadScene(scene);
    }
}
