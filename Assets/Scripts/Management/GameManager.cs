using RootMotion.Demos;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static EnumDeclaration;
public class VRIKData
{
    public VRIKCalibrator.CalibrationData ikData;
}
public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public float health;
    public HexaBody body;
    public bool canKill;
    public AudioSource mouthAudio;
    public AudioClip[] hitSounds;
    public AudioClip[] deathSounds;
    private float startingHealth;
    private bool dead = false;
    private bool canDamage = true;
    public Volume postProcessingVolume;
    private Vignette vignette;
    public InputActionReference toggleMenu;
    [Header("Default Targets")]
    public Transform defaultLeftHandTarget;
    public Transform defaultRightHandTarget;
    public HandData leftDynamicPose;
    public HandData rightDynamicPose;

    [Header("Calibration Data")]
    private VRIKData vrikData = new VRIKData();
    public VRIKCalibratedData calibrator;
    public float height;
    [System.Serializable]
    public class ImpactEffect
    {
        public surfaceType material;
        public GameObject impactEffect;
        public GameObject decal;
    }
    [Header("Effects")]
    public ImpactEffect[] impactEffects;
    [Tooltip("The opaque black backround that loses opacity on start")]
    public Image blurImage;
    [Header("Camera")]
    public Camera externalCamera;
    public GameObject recordingIcon;
    private Quaternion startRotation;
    private Vector3 startPosition;
    public Transform player;

    [Header("UI")]
    public GameObject menu;
    [Tooltip("The \"done\" button at the start after calculating height (will be null if not in start scene)")]
    public bool despawnItems = true;
    public TextMeshProUGUI healthText;
    public GameObject sceneChangeButton;
    public GameObject[] spawnableItems;
    public Transform itemSpawnPoint;
    private AudioSource audioSource;
    public AudioClip UIClickSound;
    private bool altCameraFollow;
    private float deltaTime;
    public TextMeshProUGUI fpsText;

    private void Start()
    {
        if(postProcessingVolume)
            if (postProcessingVolume.profile.TryGet<Vignette>(out vignette))

        startingHealth = health;
        StartCoroutine(HealPlayer());
        if(externalCamera)
        {
            startPosition = externalCamera.transform.position;
            startRotation = externalCamera.transform.rotation;
        }
        Application.targetFrameRate = 120;
        audioSource = GetComponent<AudioSource>();
    }
    public void IncreasePlayerHealth()
    {
        health += 10;
        startingHealth += 10;
    }
    public void DecreasePlayerHealth()
    {
        health -= 10;
        startingHealth -= 10;
    }
    public void SpawnItem(string itemName)
    {
        foreach(GameObject item in spawnableItems)
        {
            if(item.name == itemName && !Physics.CheckSphere(itemSpawnPoint.position, 0.01f))
            {
                Instantiate(item, itemSpawnPoint.position, itemSpawnPoint.rotation);
                item.GetComponent<BaseGrab>().StartCoroutine(item.GetComponent<BaseGrab>().Despawn());
            }
        }
    }
    public GameObject FindEffect(surfaceType material)
    {
        GameObject impactEffect = null;

        foreach (ImpactEffect effect in impactEffects)
            if (effect.material == material)
                impactEffect = effect.impactEffect;

        return impactEffect;
    }
    public GameObject FindDecal(surfaceType material)
    {
        GameObject impactDecal = null;

        foreach (ImpactEffect effect in impactEffects)
            if (effect.material == material)
                if (effect.decal)
                    impactDecal = effect.decal;

        return impactDecal;
    }
    public void ToggleMenu()
    {
        if (menu.activeInHierarchy)
        {
            menu.SetActive(false);
        }
        else
        {
            menu.transform.LookAt(new Vector3(player.position.x, body.Fender.transform.position.y + 1, player.position.z));
            menu.transform.position = new Vector3((player.position + player.transform.forward).x, body.Fender.transform.position.y + 1, (player.position + player.transform.forward).z);
            menu.SetActive(true);
        }
    }

    void Kill()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator HealPlayer()
    {
        float timer = 0;
        while (health > 0)
        {
            if (!canDamage)
            {
                timer = 0;
            }
            if (timer > 5)
                health = Mathf.Clamp(health += Time.deltaTime * 4, 0, startingHealth);

            timer += Time.deltaTime;
            yield return null;
        }
    }
    public void DealDamage(float damage)
    {
        if (canDamage)
        {
            health -= damage;
            mouthAudio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length - 1)]);
            StartCoroutine(DelayCanDamage());
        }
    }
    IEnumerator DelayCanDamage()
    {
        canDamage = false;
        yield return new WaitForSeconds(1);
        canDamage = true;
    }
    private void Update()
    {
        if (healthText)
        {
            healthText.text = startingHealth.ToString();
        }
        if (menu)
        {
            bool toggledMenu = toggleMenu.action.WasPressedThisFrame();
            if (toggledMenu)
            {
                ToggleMenu();
            }
            if (menu.activeInHierarchy)
            {
                Quaternion oldRotation = menu.transform.rotation;
                menu.transform.LookAt(new Vector3(player.position.x, body.Fender.transform.position.y + 1, player.position.z));
                Quaternion newRotation = menu.transform.rotation;

                menu.transform.rotation = Quaternion.Slerp(oldRotation, newRotation, 0.1f);
                menu.transform.position = Vector3.Lerp(menu.transform.position, new Vector3((player.position + player.transform.forward).x, body.Fender.transform.position.y + 1, (player.position + player.transform.forward).z), 0.1f);
            }
        }

        if(vignette)
            vignette.intensity.value = Mathf.Lerp(1f, 0, health / startingHealth);
        if (health <= 0 && !dead && canKill)
        {
            dead = true;
            JointDrive jointDrive = body.Spine.yDrive;
            jointDrive.positionSpring = 0;
            body.Spine.yDrive = jointDrive;
            blurImage.CrossFadeAlpha(1, 2, false);
            mouthAudio.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length - 1)]);
            Invoke(nameof(Kill), 3);
        }
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        if (fpsText)
        {
            fpsText.text = string.Format("{0:0.} fps", fps);
        }
        if (externalCamera)
        {
            if (altCameraFollow)
            {
                Quaternion previousRotation = externalCamera.transform.rotation;
                externalCamera.transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));
                Quaternion targetRotation = externalCamera.transform.rotation;

                externalCamera.transform.rotation = Quaternion.Slerp(previousRotation, targetRotation, 0.05f);
                externalCamera.transform.position = Vector3.Lerp(externalCamera.transform.position, new Vector3(player.position.x, Mathf.Clamp(player.position.y, height / 2, float.PositiveInfinity), player.position.z) + Vector3.forward * 2, 0.05f);
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
                recordingIcon.SetActive(false);
                externalCamera.enabled = false;
            }
            else
            {
                recordingIcon.SetActive(true);
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
        audioSource.PlayOneShot(UIClickSound, 1 / audioSource.volume);
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
