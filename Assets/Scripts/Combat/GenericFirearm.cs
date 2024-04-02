using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
public class GenericFirearm : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bullet;
    public Transform firePoint;
    public GameObject casing;
    public GameObject casingUnFired;
    public Transform casingEjectPoint;
    public float casingEjectForce;
    private int ammo = 0;
    private bool bulletInChamber;
    private bool shooting;
    public bool fullAuto;
    public float fullAutoBulletsPerSecond;
    [Header("Slide")]
    public GrabTwoAttach slide;
    public float slideThreshold;
    public Vector3 slidePoint;
    private bool hasSlide;
    private bool slideReleased = true;
    private bool primed;
    public AudioClip slideAudio;
    public ConfigurableJoint slideJoint;
    [Header("Magazine")]
    public GameObject magazinePrefab;
    public string magazineName;
    public Vector3 magazineEnterPoint;
    public float magazineEnterRadius;
    public Vector3 magazineEnterDirection;
    public float magazineEnterThreshold;
    private bool magazineInGun;
    public bool canGrabMagazineWhileInGun;
    public AudioClip magazineEnterSound;
    public GameObject animatedMag;
    public GameObject animatedEmptyMag;
    private bool canEnter = true;
    [Header("FX")]
    public GameObject muzzleFlash;
    public AudioClip fireSound;
    public Animator animator;
    private bool isSilenced;
    [Header("Forces")]
    public float bulletForce;
    public float recoilForce;
    private GrabSecondaryGrip grab;

    [System.Serializable]
    public class Attachment
    {
        public Vector3 attachPoint;
        public float attachRadius;
        public string attachmentName;
        public GameObject attachment;
        public UnityEvent attachEvent;
        public UnityEvent interactEvent;
        public bool attached;
    }
    [Header("Attachments")]
    public Attachment[] attachments;
    [Header("Input")]
    public InputActionReference leftFire;
    public InputActionReference rightFire;
    public InputActionReference leftMagRelease;
    public InputActionReference rightMagRelease;
    private bool hasPulledTrigger;
    // Start is called before the first frame update
    void Start()
    {
        grab = GetComponent<GrabSecondaryGrip>();
    }

    // Update is called once per frame
    void Update()
    {
        if(slide.isGrabbing && !slideReleased)
        {
            slideJoint.targetPosition *= -1;
            slideReleased = true;
        }
        if(ammo > 0)
        {
            animatedMag.SetActive(true);
            animatedEmptyMag.SetActive(false);
        }
        else
        {
            animatedMag.SetActive(false);
            animatedEmptyMag.SetActive(true);
        }
        if (magazineInGun)
        {
            animatedMag.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            animatedMag.transform.parent.gameObject.SetActive(false);
        }

        foreach(Attachment attachment in attachments)
        {
            if (grab.isTwoHandGrabbing && attachment.attached)
            {
                if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Left)
                {
                    bool hasPulledTrigger = rightFire.action.WasPressedThisFrame();
                    if (hasPulledTrigger)
                        attachment.interactEvent.Invoke();

                }
                else
                {
                    bool hasPulledTrigger = leftFire.action.WasPressedThisFrame();
                    if (hasPulledTrigger)
                        attachment.interactEvent.Invoke();
                }
            }

            Collider[] potentialAttachment = Physics.OverlapSphere(transform.TransformPoint(attachment.attachPoint), attachment.attachRadius);
            foreach(Collider collider in potentialAttachment)
                if (collider.transform.GetComponentInParent<GunAttachment>())
                    if (collider.transform.GetComponentInParent<GunAttachment>().attachmentName == attachment.attachmentName && !attachment.attached && collider.transform.GetComponentInParent<GrabTwoAttach>().isGrabbing)
                        if(collider.transform.GetComponentInParent<GrabTwoAttach>().handGrabbing.handType == EnumDeclaration.handTypeEnum.Left)
                        {
                            bool hasPulledTrigger= leftFire.action.WasPressedThisFrame();
                            if (hasPulledTrigger)
                                Attach(attachment, collider.transform.GetComponentInParent<GunAttachment>().gameObject);
                        }
                        else
                        {
                            bool hasPulledTrigger = rightFire.action.WasPressedThisFrame();
                            if(hasPulledTrigger)
                                Attach(attachment, collider.transform.GetComponentInParent<GunAttachment>().gameObject);
                        }
        }

        Collider[] potentialMags = Physics.OverlapSphere(transform.TransformPoint(magazineEnterPoint), magazineEnterRadius);
        foreach (Collider collider in potentialMags)
            if (collider.transform.GetComponentInParent<Magazine>())
                if (collider.transform.GetComponentInParent<Magazine>().magazineName == magazineName && Vector3.Dot(collider.transform.GetComponentInParent<Magazine>().transform.up, -transform.TransformDirection(magazineEnterDirection).normalized) > magazineEnterThreshold && !magazineInGun && collider.transform.GetComponentInParent<GrabTwoAttach>().isGrabbing && canEnter)
                    MagazineEnter(collider.transform.GetComponentInParent<GrabTwoAttach>());

        if (grab.isPrimaryGrabbing)
        {
            bool hasPulledTriggerLeft = leftFire.action.ReadValue<float>() > 0.95f;
            bool hasPulledTriggerRight = rightFire.action.ReadValue<float>() > 0.95f;

            if (!hasPulledTriggerRight && !hasPulledTriggerLeft)
                hasPulledTrigger = false;

            bool hasMagReleasedLeft = leftMagRelease.action.WasPressedThisFrame();
            bool hasMagReleasedRight = rightMagRelease.action.WasPressedThisFrame();

            if (!slide.isGrabbing)
            {
                if (!fullAuto && !animator.enabled)
                {
                    if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Left && hasPulledTriggerLeft)
                        Shoot();

                    else if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Right && hasPulledTriggerRight)
                        Shoot();
                }
                else if (ammo > 0 && !animator.enabled)
                {
                    if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Left && hasPulledTriggerLeft && !hasPulledTrigger)
                        StartCoroutine(ShootFullAuto(true));

                    else if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Right && hasPulledTriggerRight && !hasPulledTrigger)
                        StartCoroutine(ShootFullAuto(false));
                }
                else if (!animator.enabled)
                {
                    if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Left && hasPulledTriggerLeft)
                        Shoot();

                    else if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Right && hasPulledTriggerRight)
                        Shoot();
                }
            }
            if (!animator.enabled && !shooting)
            {
                if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Left && hasMagReleasedLeft && magazineInGun)
                    MagazineExit();

                else if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Right && hasMagReleasedRight && magazineInGun)
                    MagazineExit();
            }
        }
        if (Vector3.Distance(slide.transform.localPosition, slidePoint) < slideThreshold && !animator.enabled)
        {
            if (!hasSlide)
                Slide();
            hasSlide = true;
        }
        else
        {
            hasSlide = false;
        }
    }
    void Attach(Attachment attachmentOnGun, GameObject attachment)
    {
        attachmentOnGun.attachment.SetActive(true);
        attachment.GetComponent<GrabTwoAttach>().handGrabbing.UnGrab();
        Destroy(attachment);
        AudioSource.PlayClipAtPoint(magazineEnterSound, transform.TransformPoint(attachmentOnGun.attachPoint), 0.25f);
        if (attachmentOnGun.attachEvent != null)
        {
            attachmentOnGun.attachEvent.Invoke();
        }
        attachmentOnGun.attached = true;
    }
    public void GrabMagazine(GrabPhysics grab)
    {
        if (canGrabMagazineWhileInGun)
        {
            Magazine mag = Instantiate(magazinePrefab, animatedMag.transform.position, animatedMag.transform.rotation).GetComponent<Magazine>();
            mag.ammo = ammo;
            ammo = 0;
            grab.grab = mag.GetComponent<GrabTwoAttach>();
            grab.GenericGrab(null, mag.GetComponent<Rigidbody>());
            grab.grab.handGrabbing = grab;
            magazineInGun = false;
            animatedMag.transform.parent.gameObject.SetActive(false);
            animator.enabled = true;
            animator.Play("MagazineOut");
            canEnter = false;
            mag.GetComponentInChildren<Collider>().enabled = false;
            StartCoroutine(DelayCanEnter(mag.GetComponentInChildren<Collider>()));
        }
    }
    IEnumerator DelayCanEnter(Collider mag)
    {
        yield return new WaitForSeconds(1);
        canEnter = true;
        mag.enabled = true;
    }
    public void Silence()
    {
        isSilenced = true;
    }
    void MagazineEnter(GrabTwoAttach mag)
    {
        animator.enabled = true;
        ammo = mag.GetComponent<Magazine>().ammo;
        mag.handGrabbing.UnGrab();
        Destroy(mag.gameObject);
        animator.Play("MagazineIn");
        magazineInGun = true;
        AudioSource.PlayClipAtPoint(magazineEnterSound, transform.TransformPoint(magazineEnterPoint), 0.25f);
    }
    void MagazineExit()
    {
        AudioSource.PlayClipAtPoint(magazineEnterSound, transform.TransformPoint(magazineEnterPoint), 0.25f);
        animator.enabled = true;
        animator.Play("MagazineOut");
    }
    public void SpawnMag()
    {
        if (animatedMag.transform.parent.gameObject.activeInHierarchy)
        {
            Magazine spawnedMag = Instantiate(magazinePrefab, animatedMag.transform.parent.transform.position, animatedMag.transform.parent.transform.rotation).GetComponent<Magazine>();
            spawnedMag.ammo = ammo;
            ammo = 0;
            animatedMag.transform.parent.gameObject.SetActive(false);
            magazineInGun = false;
            spawnedMag.GetComponent<BaseGrab>().StartCoroutine(spawnedMag.GetComponent<BaseGrab>().Despawn());
        }
    }

    public void Slide()
    {
        if (slideReleased && !shooting)
        {
            if ((ammo > 0 && primed) || bulletInChamber)
            {
                EjectCasing();
                ammo--;
                if (ammo <= 0)
                {
                    bulletInChamber = false;
                    primed = false;
                }
                AudioSource.PlayClipAtPoint(slideAudio, slide.transform.position, 0.25f);
            }
            else if (ammo > 0)
            {
                primed = true;
                bulletInChamber = true;
                AudioSource.PlayClipAtPoint(slideAudio, slide.transform.position, 0.25f);
            }
        }
    }
    public void DisableAnimator()
    {
        animator.enabled = false;
    }
    IEnumerator ShootFullAuto(bool isLeftHand)
    {
        float timer = 0;
        bool isFiring = true;
        Shoot();
        hasPulledTrigger = true;

        while (isFiring)
        {
            if (isLeftHand)
                isFiring = leftFire.action.ReadValue<float>() > 0.8f;
            else
                isFiring = rightFire.action.ReadValue<float>() > 0.8f;

            timer += Time.deltaTime;
            if(timer >= 1 / fullAutoBulletsPerSecond)
            {
                Shoot();
                timer = 0;
            }
            yield return null;
        }
    }
    void Shoot()
    {
        if((ammo > 0 && primed) || bulletInChamber)
        {
            if (!hasPulledTrigger || fullAuto)
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
                if (!isSilenced)
                {
                    GameObject spawnedMuzzleFlash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation, firePoint);
                    AudioSource.PlayClipAtPoint(fireSound, firePoint.position, 0.5f);
                    Destroy(spawnedMuzzleFlash, 1);
                }
                else
                    AudioSource.PlayClipAtPoint(fireSound, firePoint.position, 0.25f);

                foreach (Collider c in grab.colliders)
                {
                    Physics.IgnoreCollision(c, spawnedBullet.GetComponent<Collider>());
                }

                spawnedBullet.AddForce(firePoint.forward * bulletForce);
                GetComponent<Rigidbody>().mass *= 20;
                GetComponent<Rigidbody>().AddTorque(-firePoint.right * recoilForce);
                slideJoint.targetPosition *= -1;
                Invoke(nameof(RegainControl), 0.1f);
                EjectCasing();

                if (ammo == 0 && bulletInChamber && slideReleased)
                {
                    AudioSource.PlayClipAtPoint(slideAudio, slide.transform.position, 0.25f);
                    bulletInChamber = false;
                    primed = false;
                    slideJoint.targetPosition *= -1;
                    slideReleased = false;
                }
                animator.enabled = false;
            }
        }
        else if (!hasPulledTrigger)
        {
            hasPulledTrigger = true;
            if (ammo > 0 && !slideReleased)
            {
                slideJoint.targetPosition *= -1;
                bulletInChamber = true;
                primed = true;
                slideReleased = true;
            }
            AudioSource.PlayClipAtPoint(slideAudio, firePoint.position, 0.25f);
        }
    }
    public void EjectCasing()
    {
        Rigidbody spawnedCasing = null;
        if (shooting)
        {
            spawnedCasing = Instantiate(casing, casingEjectPoint.position, casingEjectPoint.rotation).GetComponent<Rigidbody>();

        }
        else
        {
            spawnedCasing = Instantiate(casingUnFired, casingEjectPoint.position, casingEjectPoint.rotation).GetComponent<Rigidbody>();
        }

        spawnedCasing.GetComponent<Collider>().enabled = false;
        StartCoroutine(DelayCasingCollision(spawnedCasing.GetComponent<Collider>()));
        spawnedCasing.AddForce(casingEjectPoint.right * casingEjectForce);
        Destroy(spawnedCasing.gameObject, 10);
    }
    IEnumerator DelayCasingCollision(Collider casing)
    {
        yield return new WaitForSeconds(0.1f);
        casing.enabled = true;
    }
    void RegainControl()
    {
        shooting = false;
        slideJoint.targetPosition *= -1;
        GetComponent<Rigidbody>().mass /= 20;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(magazineEnterPoint), magazineEnterRadius);
        Gizmos.DrawLine(transform.TransformPoint(magazineEnterPoint), transform.TransformPoint(magazineEnterPoint) - transform.TransformDirection(magazineEnterDirection));

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        if (attachments != null)
            if (attachments.Length > 0)
                foreach (Attachment attachment in attachments)
                    Gizmos.DrawSphere(transform.TransformPoint(attachment.attachPoint), attachment.attachRadius);
    }
}
