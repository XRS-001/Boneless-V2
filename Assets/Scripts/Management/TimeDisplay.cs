using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class TimeDisplay : MonoBehaviour
{
    public TextMeshProUGUI text;
    // Update is called once per frame
    void Update()
    {
        // Get the current date and time
        DateTime now = DateTime.Now;

        // Format the date and time
        string formattedDate = now.ToString("dd/MM/yyyy");
        string formattedTime = now.ToString("HH:mm:ss");

        // Combine the date and time into a single string
        string dateTimeString = formattedDate + "\n" + formattedTime;

        text.text = dateTimeString;
    }
}
