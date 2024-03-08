using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pistol : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bullet;
    public Transform firePoint;
    public GameObject casing;
    public Transform casingEjectPoint;
    public float casingEjectForce;
    [Header("FX")]
    public GameObject muzzleFlash;
    public AudioClip fireSound;
    public Animator animator;
    [Header("Forces")]
    public float bulletForce;
    public float recoilForce;
    private GrabTwoAttach grab;
    [Header("Input")]
    public InputActionReference leftFire;
    public InputActionReference rightFire;
    // Start is called before the first frame update
    void Start()
    {
        grab = GetComponent<GrabTwoAttach>();
    }

    // Update is called once per frame
    void Update()
    {
        if(grab.isGrabbing)
        {
            bool hasPulledTriggerLeft = leftFire.action.WasPressedThisFrame();
            bool hasPulledTriggerRight = rightFire.action.WasPressedThisFrame();

            if (grab.handGrabbing.gameObject.layer == LayerMask.NameToLayer("LeftHand") && hasPulledTriggerLeft)
            {
                Shoot();
            }
            else if (grab.handGrabbing.gameObject.layer == LayerMask.NameToLayer("RightHand") && hasPulledTriggerRight)
            {
                Shoot();
            }
        }
    }
    public void DisableAnimator()
    {
        animator.enabled = false;
    }
    void Shoot()
    {
        animator.enabled = true;
        animator.Play("Shoot");
        Rigidbody spawnedBullet = Instantiate(bullet, firePoint.position, firePoint.rotation).GetComponent<Rigidbody>();
        GameObject spawnedMuzzleFlash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation, firePoint);
        AudioSource.PlayClipAtPoint(fireSound, firePoint.position, 0.5f);

        Destroy(spawnedMuzzleFlash, 1);

        foreach (Collider c in grab.colliders)
        {
            Physics.IgnoreCollision(c, spawnedBullet.GetComponent<Collider>());
        }

        spawnedBullet.AddForce(firePoint.forward * bulletForce);
        GetComponent<Rigidbody>().mass *= 10;
        GetComponent<Rigidbody>().AddTorque(-firePoint.right * recoilForce);
        Invoke(nameof(RegainControl), 0.1f);
    }
    public void EjectCasing()
    {
        Rigidbody spawnedCasing = Instantiate(casing, casingEjectPoint.position, casingEjectPoint.rotation).GetComponent<Rigidbody>();
        foreach (Collider c in grab.colliders)
        {
            Physics.IgnoreCollision(c, spawnedCasing.GetComponent<Collider>());
        }
        spawnedCasing.AddForce(casingEjectPoint.right * casingEjectForce);
        Destroy(spawnedCasing.gameObject, 10);
    }
    void RegainControl()
    {
        GetComponent<Rigidbody>().mass /= 10;
    }
}
