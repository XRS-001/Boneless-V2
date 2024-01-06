using RootMotion.Demos;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{    
    [Tooltip("The opaque black backround that loses opacity on start")]
    public Image blurImage;
    public float height;
    public VRIKCalibratedData calibrator;
    [Tooltip("The \"done\" button at the start after calculating height (will be null if not in start scene)")]
    public GameObject sceneChangeButton;
    private VRIKData vrikData = new VRIKData();
    private void Update()
    {
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
