using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class RevolverBullet
{
    public bool hasFired;
    public FixedJoint joint;
    public GameObject bullet;
    public GameObject casing;
}
public class RevolverBullets : MonoBehaviour
{
    public RevolverBullet[] revolverBullets;
    public Transform target;
    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < 0.0025f)
        {
            foreach (RevolverBullet bullet in revolverBullets)
            {
                Destroy(bullet.joint);
            }
        }
        foreach (RevolverBullet bullet in revolverBullets)
        {
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
