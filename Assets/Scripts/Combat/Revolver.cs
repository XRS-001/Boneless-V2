using System.Collections.Generic;
using System.Linq;
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
    public Transform hammer;
    public float hammerUnPrimeDegrees;
    private Quaternion initialHammerRot;

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
    public GameObject loaderPrefab;
    public Transform loadingPoint;
    public GameObject bulletsLoaded;
    public float loadingRadius;
    public string loaderName;
    private RevolverBullets bullets;
    public GameObject bulletLoadAnimation;


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
        initialHammerRot = hammer.localRotation;
    }
    private void LateUpdate()
    {
        if (!primed)
        {
            hammer.localRotation = initialHammerRot;
            hammer.RotateAround(hammer.position, hammer.right, hammerUnPrimeDegrees);
        }
        else
        {
            hammer.localRotation = initialHammerRot;
        }
    }
    private void Update()
    {
        if (bullets)
        {
            if (!bullets.revolverBullets[0].joint)
            {
                bullets.transform.parent.parent = null;
                foreach (RevolverBullet bullet in bullets.revolverBullets)
                {
                    bullet.bullet.transform.parent.parent = null;
                    bullet.bullet.transform.parent.GetComponent<Collider>().enabled = true;
                }
                Destroy(bullets.transform.parent.gameObject);
                bullets = null;
                ammo = 0;
            }
        }
        if (grab.isGrabbing)
        {
            bool hasPulledTriggerLeft = leftFire.action.ReadValue<float>() > 0.95f;
            bool hasPulledTriggerRight = rightFire.action.ReadValue<float>() > 0.95f;

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
                    if (collider.transform.GetComponentInParent<RevolverLoader>())
                        if (collider.transform.GetComponentInParent<RevolverLoader>().loaderName == loaderName && collider.transform.GetComponentInParent<GrabTwoAttach>().isGrabbing && ammo == 0)
                            Load(collider.transform.GetComponentInParent<RevolverLoader>());
            }
        }
    }
    void Load(RevolverLoader loader)
    {
        animator.enabled = true;  
        animator.Play("BulletsLoad");
        AudioSource.PlayClipAtPoint(loadSound, loadingPoint.position, 0.25f);
        loader.isLoaded = false;
        ammo = loader.ammo;
        loader.ammo = 0;
    }
    public void LoadBullets()
    {
        bulletLoadAnimation.SetActive(false);
        bullets = Instantiate(bulletsLoaded, loadingPoint).GetComponentInChildren<RevolverBullets>();

        foreach (RevolverBullet bullet in bullets.revolverBullets)
        {
            foreach (Collider collider in grab.colliders)
            {
                Physics.IgnoreCollision(collider, bullet.joint.GetComponent<Collider>());
            }
        }

        bullets.transform.localPosition = Vector3.zero;
        bullets.transform.localRotation = Quaternion.Euler(Vector3.zero);
        bullets.GetComponent<ConfigurableJoint>().connectedBody = loadingPoint.parent.GetComponent<Rigidbody>();
        bullets.GetComponent<ConfigurableJoint>().connectedAnchor = loadingPoint.transform.parent.InverseTransformPoint(loadingPoint.transform.position) + bullets.GetComponent<ConfigurableJoint>().connectedAnchor;
    }
    void Hinge()
    {
        if (!hingeDown)
        {
            if (bullets)
            {
                bullets.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Limited;
                bullets.GetComponent<ConfigurableJoint>().anchor = Vector3.zero;
            }
            hingeDown = true;
            JointSpring newSpring = new JointSpring();

            newSpring.spring = hingeDownSpring.spring;
            newSpring.damper = hingeDownSpring.damper;
            newSpring.targetPosition = hingeDownSpring.targetRotation;

            hinge.spring = newSpring;

            canHinge = false;
            Invoke(nameof(DelayCanHinge), 0.25f);
        }
        else
        {
            if (bullets)
            {
                //offset the bullets slightly into the chamber
                bullets.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked;
                bullets.GetComponent<ConfigurableJoint>().anchor = new Vector3(-0.02f, 0, 0);
            }

            AudioSource.PlayClipAtPoint(hingeSound, loadingPoint.position, 0.15f);
            hingeDown = false;
            JointSpring newSpring = new JointSpring();

            newSpring.spring = hingeUpSpring.spring;
            newSpring.damper = hingeUpSpring.damper;
            newSpring.targetPosition = hingeUpSpring.targetRotation;

            hinge.spring = newSpring;
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
        if (!hasPulledTrigger && !hingeDown && ammo > 0 && bullets)
        {
            if (primed)
            {
                hasPulledTrigger = true;
                if (ammo > 0 && primed)
                {
                    ammo--;
                }
                if (ammo == 0)
                {
                    primed = false;
                }
                else
                {
                    animator.enabled = true;
                    animator.Play("Shoot");
                }
                bullets.revolverBullets[bullets.revolverBullets.Length - ammo - 1].hasFired = true;
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
