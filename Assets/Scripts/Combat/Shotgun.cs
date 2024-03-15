using RootMotion.Demos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static RootMotion.FinalIK.FBIKChain;
using UnityEngine.Purchasing;
using UnityEngine.UIElements.Experimental;
using UnityEngine.UIElements;
using UnityEditor.Rendering;

public class Shotgun : MonoBehaviour
{
    [Header("Shooting")]
    public GrabTwoAttach grab;
    public GameObject bullet;
    public Transform fireDirection;
    public Transform[] firePoints;
    public Transform ammoEjectPoint;
    public float bulletForce;
    public float recoilForce;
    public float ammoEjectForce;
    public int ammoCapacity;
    private int ammo = 0;
    private bool shooting;
    private bool hasShot;
    private bool primed;

    [Header("FX")]
    public GameObject muzzleFlash;
    public AudioClip fireSound;
    public AudioClip pumpAudio;
    public AudioClip ammoEnterSound;
    public Animator animator;

    [Header("Ammo")]
    public GameObject ammoShotPrefab;
    public GameObject ammoPrefab;
    public string ammoName;
    public Vector3 ammoEnterPoint;
    public float ammoEnterRadius;
    public GameObject ammoInGun;

    [Header("Pump")]
    public GrabTwoAttach pump;
    private ConfigurableJoint pumpJoint;
    private float startDamper;
    public float pumpThreshold;
    public Vector3 pumpPoint;
    public Vector3 pumpEndPoint;
    private bool hasPumped;
    private bool atPumpEnd;

    [Header("Input")]
    public InputActionReference leftFire;
    public InputActionReference rightFire;
    private bool hasPulledTrigger;
    // Start is called before the first frame update
    void Start()
    {
        pumpJoint = pump.GetComponent<ConfigurableJoint>();
        startDamper = pumpJoint.yDrive.positionDamper;
    }

    public void DisableAnimator()
    {
        animator.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        if(ammo > 0)
            ammoInGun.SetActive(true);
        else
            ammoInGun.SetActive(false);
        if (!pump.isGrabbing)
        {
            JointDrive newDrive = pumpJoint.yDrive;
            newDrive.positionSpring = 0;
            newDrive.positionDamper = float.PositiveInfinity;
            pumpJoint.yDrive = newDrive;
        }
        else if (!primed)
        {
            JointDrive newDrive = pumpJoint.yDrive;
            newDrive.positionSpring = 0;
            newDrive.positionDamper = startDamper;
            pumpJoint.yDrive = newDrive;
        }
        else if(ammo > 0)
        {
            JointDrive newDrive = pumpJoint.yDrive;
            newDrive.positionDamper = startDamper;
            newDrive.positionSpring = float.PositiveInfinity;
            pumpJoint.yDrive = newDrive;
        }
        else 
        {
            JointDrive newDrive = pumpJoint.yDrive;
            newDrive.positionSpring = 0;
            newDrive.positionDamper = startDamper;
            pumpJoint.yDrive = newDrive;
        }
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
        }
        Collider[] potentialMags = Physics.OverlapSphere(transform.TransformPoint(ammoEnterPoint), ammoEnterRadius);
        foreach (Collider collider in potentialMags)
            if (collider.transform.root.GetComponent<ShotgunShell>())
                if (collider.transform.root.GetComponent<ShotgunShell>().canEnterGun)
                    if (collider.transform.root.GetComponent<ShotgunShell>().shellName == ammoName && ammo < ammoCapacity && atPumpEnd)
                    AmmoEnter(collider.transform.root.GetComponent<GrabTwoAttach>());

        if (Vector3.Distance(pump.transform.localPosition, pumpPoint) < pumpThreshold / 2 && !hasShot)
        {
            if (!hasPumped)
                PumpStart();
            hasPumped = true;
        }
        else
        {
            hasPumped = false;
        }

        if (Vector3.Distance(pump.transform.localPosition, pumpEndPoint) < pumpThreshold)
        {
            if (!atPumpEnd)
                PumpEnd();
            atPumpEnd = true;
        }
        else
        {
            atPumpEnd = false;
        }
    }
    void PumpStart()
    {
        if(ammo > 0)
        {
            primed = true;
        }
    }
    void PumpEnd()
    {
        if(ammo > 0)
        {
            hasShot = false;
            EjectCasing();
        }
        AudioSource.PlayClipAtPoint(pumpAudio, pump.transform.position, 0.25f);
    }
    void AmmoEnter(GrabTwoAttach ammoGrab)
    {
        AudioSource.PlayClipAtPoint(ammoEnterSound, transform.TransformPoint(ammoEnterPoint), 0.25f);
        animator.enabled = true;
        animator.Play("AmmoEnter");
        ammoGrab.handGrabbing.UnGrab();
        Destroy(ammoGrab.gameObject);
        ammo++;
    }
    void Shoot()
    {
        if (ammo > 0 && primed && !hasPulledTrigger)
        {
            hasPulledTrigger = true;
            shooting = true;
            hasShot = true;
            primed = false;
            if (ammo > 0 && primed)
            {
                ammo--;
            }
            AudioSource.PlayClipAtPoint(fireSound, fireDirection.position, 0.5f);
            GameObject spawnedMuzzleFlash = Instantiate(muzzleFlash, fireDirection.position, fireDirection.rotation, fireDirection);
            Destroy(spawnedMuzzleFlash, 1);

            for (int i = 0; i < firePoints.Length; i++)
            {
                Rigidbody spawnedBullet = Instantiate(bullet, firePoints[i].position, firePoints[i].rotation).GetComponent<Rigidbody>();

                foreach (Collider c in grab.colliders)
                {
                    Physics.IgnoreCollision(c, spawnedBullet.GetComponent<Collider>());
                }

                spawnedBullet.AddForce(firePoints[i].forward * bulletForce);
            }
            GetComponent<Rigidbody>().mass *= 10;
            GetComponent<Rigidbody>().AddTorque(-transform.right * recoilForce);
            Invoke(nameof(RegainControl), 0.1f);

        }
        else if (!hasPulledTrigger)
        {
            hasPulledTrigger = true;
            AudioSource.PlayClipAtPoint(pumpAudio, fireDirection.position, 0.25f);
        }
    }
    public void EjectCasing()
    {
        Rigidbody spawnedCasing = null;
        if (hasShot)
        {
            spawnedCasing = Instantiate(ammoShotPrefab, ammoEjectPoint.position, ammoEjectPoint.rotation).GetComponent<Rigidbody>();
        }
        else
        {
            spawnedCasing = Instantiate(ammoPrefab, ammoEjectPoint.position, ammoEjectPoint.rotation).GetComponent<Rigidbody>();
        }
        foreach (Collider c in grab.colliders)
        {
            Physics.IgnoreCollision(c, spawnedCasing.GetComponent<Collider>());
        }
        spawnedCasing.AddForce(ammoEjectPoint.right * ammoEjectForce);
        Destroy(spawnedCasing.gameObject, 10);
        spawnedCasing.GetComponent<ShotgunShell>().DelayCanEnter();
        ammo--;
    }
    void RegainControl()
    {
        shooting = false;
        GetComponent<Rigidbody>().mass /= 10;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(ammoEnterPoint), ammoEnterRadius);
    }
}
