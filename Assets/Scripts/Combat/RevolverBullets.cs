using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class RevolverBullet
{
    public ConfigurableJoint joint;
    public Transform destroyJointTarget;
    public bool hasFired;
    public GameObject bullet;
    public GameObject casing;
}
public class RevolverBullets : MonoBehaviour
{
    public RevolverBullet[] revolverBullets;
    // Update is called once per frame
    void Update()
    {
        foreach(RevolverBullet bullet in revolverBullets)
        {
            if (bullet.joint)
            {
                if (Vector3.Distance(bullet.joint.transform.position, bullet.destroyJointTarget.position) < 0.0025f)
                {
                    bullet.joint.transform.parent = null;
                    Destroy(bullet.joint);
                }
            }
            if(bullet.hasFired)
            {
                bullet.bullet.SetActive(false);
                bullet.casing.SetActive(true);
            }
            else
            {
                bullet.bullet.SetActive(true);
                bullet.casing.SetActive(false);
            }
        }
    }
}
