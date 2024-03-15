using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunShell : MonoBehaviour
{
    public string shellName;
    public bool canEnterGun = true;
    public void DelayCanEnter()
    {
        canEnterGun = false;
        Invoke(nameof(CanEnter), 1);
    }
    void CanEnter()
    {
        canEnterGun = true;
    }
}
