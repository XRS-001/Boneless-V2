using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolverLoader : MonoBehaviour
{
    public GameObject loaded;
    public GameObject unLoaded;
    public bool isLoaded = true;
    public string loaderName;

    // Update is called once per frame
    void Update()
    {
        if (isLoaded)
        {
            loaded.SetActive(true);
            unLoaded.SetActive(false);
        }
        else
        {
            loaded.SetActive(false);
            unLoaded.SetActive(true);
        }
    }
}
