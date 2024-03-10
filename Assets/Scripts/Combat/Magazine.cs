using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magazine : MonoBehaviour
{
    public string magazineName;
    public int ammo;
    public GameObject magazine;
    public GameObject emptyMagazine;

    // Update is called once per frame
    void Update()
    {
        if (ammo == 0)
        {
            magazine.SetActive(false);
            emptyMagazine.SetActive(true);
        }
        else
        {
            magazine.SetActive(true);
            emptyMagazine.SetActive(false);
        }
    }
}
