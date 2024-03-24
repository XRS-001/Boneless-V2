using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagazinePouch : MonoBehaviour
{
    public GrabPhysics[] grabs;
    public void GrabMagazine(GrabPhysics grab)
    {
        GameObject spawnedMag = null;
        for (int i = 0; i < grabs.Length; i++)
        {
            if (grabs[i].grab)
            {
                object gunType = grabs[i].grab.GetComponent<GenericFirearm>();
                if(gunType == null)
                    gunType = grabs[i].grab.GetComponent<Revolver>();

                if (gunType == null)
                    gunType = grabs[i].grab.GetComponent<Shotgun>();

                if (gunType != null)
                {
                    switch (gunType)
                    {
                        case GenericFirearm:
                            GenericFirearm genericFirearm = gunType as GenericFirearm;
                            spawnedMag = Instantiate(genericFirearm.magazinePrefab, grab.transform.position, grab.transform.rotation);
                            break;

                        case Shotgun:
                            Shotgun shotgun = gunType as Shotgun;
                            spawnedMag = Instantiate(shotgun.ammoPrefab, grab.transform.position, grab.transform.rotation);
                            break;

                        case Revolver:
                            Revolver revolver = gunType as Revolver;
                            spawnedMag = Instantiate(revolver.loaderPrefab, grab.transform.position, grab.transform.rotation);
                            break;
                    }
                }
            }
        }
        if (spawnedMag)
        {
            grab.grab = spawnedMag.GetComponent<GrabTwoAttach>();
            grab.grab.handGrabbing = grab;
            grab.GenericGrab(null, spawnedMag.GetComponent<Rigidbody>(), true);
        }
    }
}
