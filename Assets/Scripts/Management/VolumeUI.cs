using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VolumeUI : MonoBehaviour
{
    public AudioSource audioSource;
    private TextMeshProUGUI volumeText;
    private float value = 0.5f;
    private void Start()
    {
        volumeText = GetComponent<TextMeshProUGUI>();
    }
    private void Update()
    {
        //prevent it from getting too loud with division
        audioSource.volume = value / 2;
        volumeText.text = value.ToString("0.0");
    }
    public void IncreaseValue()
    {
        if(value < 1)
        {
            value += 0.1f;
        }
        audioSource.volume = value;
    }
    public void DecreaseValue()
    {
        if (volumeText.text != "0.0")
        {
            value -= 0.1f;
        }
        audioSource.volume = value;
    }
}
