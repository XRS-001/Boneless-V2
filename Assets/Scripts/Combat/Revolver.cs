using RootMotion.Demos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static RootMotion.FinalIK.FBIKChain;
using UnityEngine.Purchasing;
using UnityEngine.UIElements.Experimental;
using UnityEngine.UIElements;

public class Revolver : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bullet;
    public Transform firePoint;
    public float bulletForce;
    public float recoilForce;
    private bool hasPulledTrigger;
    private bool shooting;
    private int ammo = 0;
    private bool primed;

    [Header("Loading")]
    public Transform loadingPoint;
    private bool hingeDown;

    [Header("FX")]
    public GameObject muzzleFlash;
    public AudioClip fireSound;
    public AudioClip loadSound;
    public Animator animator;

    [Header("Input")]
    private GrabTwoAttach grab;
    public InputActionReference leftFire;
    public InputActionReference rightFire;
    public InputActionReference leftUnHinge;
    public InputActionReference rightUnHinge;
    private void Start()
    {
        grab = GetComponent<GrabTwoAttach>();
    }
    private void Update()
    {
        if (grab.isGrabbing)
        {
            bool hasPulledTriggerLeft = leftFire.action.ReadValue<float>() > 0.8f;
            bool hasPulledTriggerRight = rightFire.action.ReadValue<float>() > 0.8f;

            if (!hasPulledTriggerRight && !hasPulledTriggerLeft)
                hasPulledTrigger = false;

            if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Left && hasPulledTriggerLeft)
                Shoot();

            else if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Right && hasPulledTriggerRight)
                Shoot();

            bool hasHingedLeft = leftUnHinge.action.WasPressedThisFrame();
            bool hasHingedRight = rightUnHinge.action.WasPressedThisFrame();


            if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Left && hasHingedLeft)
                Hinge();

            else if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Right && hasHingedRight)
                Hinge();
        }
    }
    void Hinge()
    {
        animator.enabled = true;
        AudioSource.PlayClipAtPoint(loadSound, loadingPoint.position, 0.3f);
        if (!hingeDown)
        {
            hingeDown = true;
            animator.Play("HingeDown");
        }
        else
        {
            hingeDown = false;
            animator.Play("HingeUp");
        }
    }
    public void DisableAnimator()
    {
        animator.enabled = false;
    }
    void Shoot()
    {
        if (!hasPulledTrigger && !hingeDown)
        {
            hasPulledTrigger = true;
            shooting = true;
            if (ammo > 0 && primed)
            {
                ammo--;
            }
            animator.enabled = true;
            animator.Play("Shoot");
            Rigidbody spawnedBullet = Instantiate(bullet, firePoint.position, firePoint.rotation).GetComponent<Rigidbody>();
            GameObject spawnedMuzzleFlash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation, firePoint);
            Destroy(spawnedMuzzleFlash, 1);
            AudioSource.PlayClipAtPoint(fireSound, firePoint.position, 0.5f);

            foreach (Collider c in grab.colliders)
            {
                Physics.IgnoreCollision(c, spawnedBullet.GetComponent<Collider>());
            }

            spawnedBullet.AddForce(firePoint.forward * bulletForce);
            GetComponent<Rigidbody>().mass *= 10;
            GetComponent<Rigidbody>().AddTorque(-firePoint.right * recoilForce);
            Invoke(nameof(RegainControl), 0.1f);
        }
        else if (!hasPulledTrigger)
        {
            hasPulledTrigger = true;
            AudioSource.PlayClipAtPoint(loadSound, firePoint.position, 0.25f);
        }
    }

    void RegainControl()
    {
        shooting = false;
        GetComponent<Rigidbody>().mass /= 10;
    }
}
