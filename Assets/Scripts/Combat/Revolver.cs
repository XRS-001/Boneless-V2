using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Revolver;

public class Revolver : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bullet;
    public Transform firePoint;
    public float bulletForce;
    public float recoilForce;
    private bool hasPulledTrigger;
    private int ammo = 0;
    private bool primed = true;

    [Header("Hinge")]
    public HingeJoint hinge;
    [System.Serializable]
    public class HingeSpring
    {
        public float spring;
        public float damper;
        public float targetRotation;
    }
    public HingeSpring hingeDownSpring;
    public HingeSpring hingeUpSpring;

    public Transform hingeTarget;
    private bool hingeDown;
    private bool canHinge = true;

    [Header("Loading")]
    public Transform loadingPoint;
    public GameObject bulletsLoaded;
    public List<RevolverBullet> bulletsInGun = new List<RevolverBullet>();
    public float loadingRadius;
    public string loaderName;


    [Header("FX")]
    public GameObject muzzleFlash;
    public AudioClip fireSound;
    public AudioClip loadSound;
    public AudioClip hingeSound;
    public Animator animator;

    private GrabTwoAttach grab;
    [Header("Input")]
    public InputActionReference leftFire;
    public InputActionReference rightFire;
    public InputActionReference leftUnHinge;
    public InputActionReference rightUnHinge;
    private void Start()
    {
        grab = GetComponent<GrabTwoAttach>();
        foreach(Collider collider in grab.colliders)
        {
            foreach (Collider collider2 in grab.colliders)
            {
                Physics.IgnoreCollision(collider, collider2);
            }
        }
    }
    private void Update()
    {
        List<RevolverBullet> bulletsToRemove = new List<RevolverBullet>();

        foreach (RevolverBullet bulletInGun in bulletsInGun)
        {
            if (!bulletInGun.joint)
            {
                bulletsToRemove.Add(bulletInGun);
            }
            else
            {
                if (!hingeDown)
                {
                    bulletInGun.joint.zMotion = ConfigurableJointMotion.Locked;
                    bulletInGun.joint.anchor = new Vector3(0, 0, -0.02f);
                }
                else
                {
                    bulletInGun.joint.zMotion = ConfigurableJointMotion.Limited;
                    bulletInGun.joint.anchor = new Vector3(0, 0, 0);
                }
            }
        }

        foreach (RevolverBullet bulletToRemove in bulletsToRemove)
        {
            bulletsInGun.Remove(bulletToRemove);
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

            bool hasHingedLeft = leftUnHinge.action.WasPressedThisFrame();
            bool hasHingedRight = rightUnHinge.action.WasPressedThisFrame();

            if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Left)
            {
                if (Quaternion.Angle(hinge.transform.rotation, hingeTarget.transform.rotation) < 1 && hingeDown && !hasHingedLeft && canHinge)
                {
                    Hinge();
                }
                if (!hingeDown && hasHingedLeft)
                {
                    Hinge();
                }
            }
            else if (grab.handGrabbing.handType == EnumDeclaration.handTypeEnum.Right)
            {
                if (Quaternion.Angle(hinge.transform.rotation, hingeTarget.transform.rotation) < 1 && hingeDown && !hasHingedRight && canHinge)
                {
                    Hinge();
                }
                if (!hingeDown && hasHingedRight)
                {
                    Hinge();
                }
            }

            if (hingeDown)
            {
                Collider[] potentialMags = Physics.OverlapSphere(loadingPoint.position, loadingRadius);
                foreach (Collider collider in potentialMags)
                    if (collider.transform.root.GetComponent<RevolverLoader>())
                        if (collider.transform.root.GetComponent<RevolverLoader>().loaderName == loaderName && collider.transform.root.GetComponent<GrabTwoAttach>().isGrabbing && ammo == 0)
                            Load(collider.transform.root.GetComponent<RevolverLoader>());
            }
        }
    }
    void Load(RevolverLoader loader)
    {
        AudioSource.PlayClipAtPoint(loadSound, loadingPoint.position, 0.25f);
        loader.isLoaded = false;
        RevolverBullets spawnedBullets = Instantiate(bulletsLoaded, loadingPoint).GetComponent<RevolverBullets>();

        foreach (RevolverBullet bullet in spawnedBullets.revolverBullets)
        {
            foreach (Collider collider in grab.colliders)
            {
                Physics.IgnoreCollision(collider, bullet.joint.GetComponent<Collider>());
            }
        }

        spawnedBullets.transform.localPosition = Vector3.zero;
        spawnedBullets.transform.localRotation = Quaternion.Euler(Vector3.zero);
        spawnedBullets.GetComponent<FixedJoint>().connectedBody = loadingPoint.parent.GetComponent<Rigidbody>();
        spawnedBullets.GetComponent<FixedJoint>().connectedAnchor = loadingPoint.transform.parent.InverseTransformPoint(loadingPoint.transform.position);

        foreach (RevolverBullet bullet in spawnedBullets.revolverBullets)
        {
            bulletsInGun.Add(bullet);
        }
        ammo = bulletsInGun.Count;
    }
    void Hinge()
    {
        if (!hingeDown)
        {
            hingeDown = true;
            JointSpring newSpring = new JointSpring();

            newSpring.spring = hingeDownSpring.spring;
            newSpring.damper = hingeDownSpring.damper;
            newSpring.targetPosition = hingeDownSpring.targetRotation;

            hinge.spring = newSpring;
        }
        else
        {
            AudioSource.PlayClipAtPoint(hingeSound, loadingPoint.position, 0.15f);
            hingeDown = false;
            JointSpring newSpring = new JointSpring();

            newSpring.spring = hingeUpSpring.spring;
            newSpring.damper = hingeUpSpring.damper;
            newSpring.targetPosition = hingeUpSpring.targetRotation;

            hinge.spring = newSpring;
            canHinge = false;
            Invoke(nameof(DelayCanHinge), 1);
        }
    }
    void DelayCanHinge()
    {
        canHinge = true;
    }
    public void DisableAnimator()
    {
        animator.enabled = false;
    }
    void Shoot()
    {
        if (!hasPulledTrigger && !hingeDown && ammo > 0 && bulletsInGun.Count > 0)
        {
            if (primed)
            {
                bulletsInGun[bulletsInGun.Count - ammo].hasFired = true;
                hasPulledTrigger = true;
                if (ammo > 0 && primed)
                {
                    ammo--;
                }
                animator.enabled = true;
                if (ammo == 0)
                {
                    primed = false;
                    animator.Play("ShootUnPrime");
                }
                else
                {
                    animator.Play("Shoot");
                }
                Rigidbody spawnedBullet = Instantiate(bullet, firePoint.position, firePoint.rotation).GetComponent<Rigidbody>();
                GameObject spawnedMuzzleFlash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation, firePoint);
                Destroy(spawnedMuzzleFlash, 1);
                AudioSource.PlayClipAtPoint(fireSound, firePoint.position, 0.5f);

                foreach (Collider c in grab.colliders)
                {
                    Physics.IgnoreCollision(c, spawnedBullet.GetComponent<Collider>());
                }

                spawnedBullet.AddForce(firePoint.forward * bulletForce);
                GetComponent<Rigidbody>().mass *= 15;
                GetComponent<Rigidbody>().AddTorque(-firePoint.right * recoilForce);
                Invoke(nameof(RegainControl), 0.2f);
            }
            else
            {
                animator.enabled = true;
                animator.Play("HammerPrime");
                hasPulledTrigger = true;
                AudioSource.PlayClipAtPoint(loadSound, firePoint.position, 0.25f);
                primed = true;
            }
        }
        else if (!hasPulledTrigger)
        {
            hasPulledTrigger = true;
            AudioSource.PlayClipAtPoint(loadSound, firePoint.position, 0.25f);
        }
    }

    void RegainControl()
    {
        GetComponent<Rigidbody>().mass /= 15;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(loadingPoint.position, loadingRadius);
    }
}
