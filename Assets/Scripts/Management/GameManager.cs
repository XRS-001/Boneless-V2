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
    [HideInInspector]
    public float volume;
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
    public AudioSource audioSource;
    public AudioClip UIClickSound;
    private bool altCameraFollow;
    private float deltaTime;
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI volumeText;
    private float value = 0.5f;
    [Header("Waves")]
    public GameObject[] waveUIElements;
    public GameObject[] enemies;
    public AudioClip[] waveAudio;
    public AudioSource waveAudioSource;
    private bool isFadingAudio;
    public AudioClip defaultMusic;
    public Transform[] enemySpawnPoints;
    public int enemiesActive;
    public int enemiesFought;
    public bool waveRunning;
    public GameObject[] enemiesLeftUIElements;
    public TextMeshProUGUI enemiesLeftText;

    public void IncreaseVolume()
    {
        if (value < 1)
        {
            value += 0.1f;
        }
        volume = value;
    }
    public void DecreaseVolume()
    {
        if (volumeText.text != "0.0")
        {
            value -= 0.1f;
        }
        volume = value;
    }
    private void Start()
    {
        volume = value;
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
    }
    IEnumerator FadeMusic(bool waveMusic)
    {
        isFadingAudio = true;
        float timer = 0;
        while(timer < 1)
        {
            waveAudioSource.volume = Mathf.Lerp(volume, 0, timer / 1);
            timer += Time.deltaTime;
            yield return null;
        }
        waveAudioSource.Stop();
        if (waveMusic)
        {
            waveAudioSource.clip = waveAudio[Random.Range(0, waveAudio.Length)];
        }
        else
        {
            waveAudioSource.clip = defaultMusic;
        }
        waveAudioSource.Play();
        timer = 0;
        while (timer < 1)
        {
            waveAudioSource.volume = Mathf.Lerp(0, volume, timer / 1);
            timer += Time.deltaTime;
            yield return null;
        }
        isFadingAudio = false;
    }
    IEnumerator WaveRoutine(string difficulty)
    {
        foreach(GameObject gameObject in enemiesLeftUIElements)
        {
            gameObject.SetActive(true);
            gameObject.GetComponent<Graphic>()?.CrossFadeAlpha(0, 0, false);
            gameObject.GetComponent<Graphic>()?.CrossFadeAlpha(1, 1, false);
        }
        waveRunning = true;
        StartCoroutine(FadeMusic(true));
        switch (difficulty)
        {
            case "Easy":
                enemiesLeftText.text = $"Enemies Left: {15 - enemiesFought}";
                break;
            case "Medium":
                enemiesLeftText.text = $"Enemies Left: {25 - enemiesFought}";
                break;
            case "Hard":
                enemiesLeftText.text = $"Enemies Left: {35 - enemiesFought}";
                break;
        }
        yield return new WaitForSeconds(1);

        foreach (GameObject gameObject in waveUIElements)
        {
            gameObject.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(2);
        switch (difficulty)
        {
            case "Easy":
                while (enemiesFought < 15)
                {
                    if(enemiesActive < 1)
                    {
                        enemiesActive++;
                        yield return new WaitForSeconds(3);
                        int randomSpawn = Random.Range(0, enemySpawnPoints.Length);
                        GameObject enemy = Instantiate(enemies[Random.Range(0, enemies.Length)], enemySpawnPoints[randomSpawn].position, enemySpawnPoints[randomSpawn].rotation);
                        enemy.SetActive(false);
                        yield return new WaitForSeconds(0.5f);
                        enemy.SetActive(true);
                    }
                    float timeLeft = waveAudioSource.clip.length - waveAudioSource.time;
                    if (timeLeft < 1 && !isFadingAudio)
                        StartCoroutine(FadeMusic(true));

                    enemiesLeftText.text = $"Enemies Left: {15 - enemiesFought}";

                    yield return null;
                }
                StartCoroutine(FadeMusic(false));
                foreach (GameObject gameObject in waveUIElements)
                {
                    gameObject.gameObject.SetActive(true);
                }
                foreach (GameObject graphic in waveUIElements)
                {
                    graphic.GetComponent<Graphic>()?.CrossFadeAlpha(1, 1, false);
                    graphic.GetComponent<TextMeshProUGUI>()?.CrossFadeAlpha(1, 1, false);
                }
                break;

            case "Medium":
                while (enemiesFought < 25)
                {
                    if (enemiesActive < 2)
                    {
                        enemiesActive++;
                        yield return new WaitForSeconds(2);
                        int randomSpawn = Random.Range(0, enemySpawnPoints.Length);
                        GameObject enemy = Instantiate(enemies[Random.Range(0, enemies.Length)], enemySpawnPoints[randomSpawn].position, enemySpawnPoints[randomSpawn].rotation);
                        enemy.SetActive(false);
                        yield return new WaitForSeconds(0.5f);
                        enemy.SetActive(true);
                    }
                    float timeLeft = waveAudioSource.clip.length - waveAudioSource.time;
                    if (timeLeft < 1 && !isFadingAudio)
                        StartCoroutine(FadeMusic(true));

                    enemiesLeftText.text = $"Enemies Left: {25 - enemiesFought}";

                    yield return null;
                }
                StartCoroutine(FadeMusic(false));

                foreach (GameObject gameObject in waveUIElements)
                {
                    gameObject.gameObject.SetActive(true);
                }
                foreach (GameObject graphic in waveUIElements)
                {
                    graphic.GetComponent<Graphic>()?.CrossFadeAlpha(1, 1, false);
                    graphic.GetComponent<TextMeshProUGUI>()?.CrossFadeAlpha(1, 1, false);
                }
                break;

            case "Hard":
                while (enemiesFought < 35)
                {
                    if (enemiesActive < 3)
                    {
                        enemiesActive++;
                        yield return new WaitForSeconds(1);
                        int randomSpawn = Random.Range(0, enemySpawnPoints.Length);
                        GameObject enemy = Instantiate(enemies[Random.Range(0, enemies.Length)], enemySpawnPoints[randomSpawn].position, enemySpawnPoints[randomSpawn].rotation);
                        enemy.SetActive(false);
                        yield return new WaitForSeconds(0.5f);
                        enemy.SetActive(true);
                    }
                    float timeLeft = waveAudioSource.clip.length - waveAudioSource.time;
                    if (timeLeft < 1 && !isFadingAudio)
                        StartCoroutine(FadeMusic(true));

                    enemiesLeftText.text = $"Enemies Left: {35 - enemiesFought}";

                    yield return null;
                }
                StartCoroutine(FadeMusic(false));
                foreach (GameObject gameObject in waveUIElements)
                {
                    gameObject.gameObject.SetActive(true);
                }
                foreach (GameObject graphic in waveUIElements)
                {
                    graphic.GetComponent<Graphic>()?.CrossFadeAlpha(1, 1, false);
                    graphic.GetComponent<TextMeshProUGUI>()?.CrossFadeAlpha(1, 1, false);
                }
                break;
        }
        enemiesFought = 0;
        foreach (GameObject gameObject in enemiesLeftUIElements)
        {
            gameObject.GetComponent<Graphic>()?.CrossFadeAlpha(0, 1, false);
        }
        yield return new WaitForSeconds(1);
        foreach (GameObject gameObject in enemiesLeftUIElements)
        {
            gameObject.SetActive(false);
        }
    }
    public void StartWave(string difficulty)
    {
        if (!waveRunning)
        {
            foreach (GameObject graphic in waveUIElements)
            {
                graphic.GetComponent<Graphic>()?.CrossFadeAlpha(0, 1, false);
                graphic.GetComponent<TextMeshProUGUI>()?.CrossFadeAlpha(0, 1, false);
            }
            StartCoroutine(WaveRoutine(difficulty));
        }
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
                BaseGrab spawnedItem = Instantiate(item, itemSpawnPoint.position, itemSpawnPoint.rotation).GetComponent<BaseGrab>();
                spawnedItem.StartCoroutine(spawnedItem.Despawn());
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
        if (volumeText)
        {
            waveAudioSource.volume = volume;
            volumeText.text = value.ToString("0.0");
        }
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
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
