using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class GunFlashlight : MonoBehaviour
{
    public GameObject flashlight;
    public Material mat;
    public AudioClip toggleSound;

    public void ToggleLight()
    {
        AudioSource.PlayClipAtPoint(toggleSound, flashlight.transform.position, 0.25f);
        if (!flashlight.activeInHierarchy)
        {
            mat.EnableKeyword("_EMISSION");
            flashlight.SetActive(true);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            flashlight.SetActive(false);
        }
    }
}
