using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeightDisplay : MonoBehaviour
{
    public GameManager gameManager;
    private TextMeshProUGUI text;
    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        text.text = $"Height: \n{gameManager.height.ToString("0.00")}m";
    }
}
