using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit;
using static EnumDeclaration;
public class Pierce : MonoBehaviour
{
    private AudioSource audioSource;
    [Header("Stab Data")]
    public Collider[] colliders;
    [Tooltip("The axis of the blade")]
    public upDirection stabDirection;
    private Vector3 stabAxis;
    public Vector3 piercePoint;
    public LayerMask pierceableLayer;
    [Tooltip("The damper of the piercing")]
    public float damper;
    [Tooltip("The limit of the piercing")]
    public float limit;
    [Tooltip("The velocity needed to pierce")]
    public float velocityThreshold;
    private float velocity;
    [Header("Effects")]
    public AudioClip stabSound;
    public float stabVolume;
    public DecalProjector[] objectDecals;
    public GameObject bloodDecal;
    [HideInInspector]
    public bool stabbed = false;
    private Collider stabbedCollider;
    private ConfigurableJoint configurableJoint;
    private Rigidbody rb;
    private GameObject hitPoint;
    private GameObject spawnedDecal;
    private void Update()
    {
        switch (stabDirection)
        {
            case upDirection.forward:
                stabAxis = transform.forward;
                break;

            case upDirection.up:
                stabAxis = transform.up;
                break;

            case upDirection.right:
                stabAxis = transform.right;
                break;
        }
    }
    private void Start()
    {
        StartCoroutine(WaitToCheckAudio());
        rb = GetComponent<Rigidbody>();
    }
    IEnumerator WaitToCheckAudio()
    {
        yield return new WaitForSeconds(0.1f);
        if (!GetComponent<CollisionImpact>())
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1;
        }
        else
        {
            audioSource = GetComponent<CollisionImpact>().audioSource;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        velocity = rb.velocity.magnitude;
        if (Physics.CheckSphere(transform.TransformPoint(piercePoint), 0.035f, pierceableLayer) && !stabbed && velocity > velocityThreshold)
        {
            Collider[] checkColliders = Physics.OverlapSphere(transform.TransformPoint(piercePoint), 0.035f, pierceableLayer);
            if (checkColliders[0].transform.root.name != gameObject.name)
            {
                stabbedCollider = checkColliders[0];

                stabbedCollider.Raycast(new Ray(transform.position, stabbedCollider.ClosestPoint(transform.TransformPoint(piercePoint)) - transform.position), out RaycastHit hitInfo, float.PositiveInfinity);

                if (Vector3.Dot(stabAxis, hitInfo.normal) < -0.7f)
                {
                    stabbed = true;

                    foreach (DecalProjector decal in objectDecals)
                    {
                        decal.fadeFactor += 0.25f;
                    }

                    StartCoroutine(WaitToSFX());

                    hitPoint = new GameObject("HitPoint");
                    hitPoint.transform.position = transform.TransformPoint(new Vector3(piercePoint.x, piercePoint.y, piercePoint.z - 0.15f));
                    hitPoint.transform.parent = stabbedCollider.transform;

                    spawnedDecal = Instantiate(bloodDecal);
                    spawnedDecal.transform.position = hitInfo.point;
                    spawnedDecal.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                    spawnedDecal.transform.parent = stabbedCollider.transform;
                    StartCoroutine(DelayOpacity(spawnedDecal.GetComponent<DecalProjector>()));

                    foreach (Collider ragdollCollider in stabbedCollider.transform.root.GetComponentsInChildren<Collider>())
                    {
                        foreach (Collider collider in colliders)
                        {
                            Physics.IgnoreCollision(collider, ragdollCollider, true);
                        }
                    }
                    configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
                    configurableJoint.connectedBody = stabbedCollider.GetComponent<Rigidbody>();
                    SoftJointLimit jointLimit = configurableJoint.linearLimit;
                    jointLimit.limit = limit;
                    configurableJoint.linearLimit = jointLimit;

                    JointDrive zDrive = configurableJoint.zDrive;
                    zDrive.positionDamper = damper;
                    configurableJoint.zDrive = zDrive;
                    switch (stabDirection)
                    {
                        case upDirection.forward:
                            configurableJoint.xMotion = ConfigurableJointMotion.Locked;
                            configurableJoint.yMotion = ConfigurableJointMotion.Locked;
                            configurableJoint.zMotion = ConfigurableJointMotion.Limited;
                            break;

                        case upDirection.up:
                            configurableJoint.xMotion = ConfigurableJointMotion.Locked;
                            configurableJoint.yMotion = ConfigurableJointMotion.Limited;
                            configurableJoint.zMotion = ConfigurableJointMotion.Locked;
                            break;

                        case upDirection.right:
                            configurableJoint.xMotion = ConfigurableJointMotion.Limited;
                            configurableJoint.yMotion = ConfigurableJointMotion.Locked;
                            configurableJoint.zMotion = ConfigurableJointMotion.Locked;
                            break;
                    }
                    configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
                    configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
                    configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
                }
            }
        }
        if (stabbed && Vector3.Distance(hitPoint.transform.position, transform.TransformPoint(piercePoint)) < 0.1f)
        {
            stabbed = false;
            Destroy(configurableJoint);
            Destroy(hitPoint);
            foreach (Collider ragdollCollider in stabbedCollider.transform.root.GetComponentsInChildren<Collider>())
            {
                foreach (Collider collider in colliders)
                {
                    Physics.IgnoreCollision(collider, ragdollCollider, false);
                }
            }
        }
    }
    IEnumerator DelayOpacity(DecalProjector decal)
    {
        float timer = 0f;
        while (timer < 15)
        {
            decal.fadeFactor = Mathf.Lerp(1, 0, timer / 15);
            timer += Time.deltaTime;
            if (timer >= 15)
            {
                Destroy(decal);
            }
            yield return null;
        }
    }
    IEnumerator WaitToSFX()
    {
        yield return new WaitForSeconds(0.025f);
        if(stabbed)
        {
            audioSource.PlayOneShot(stabSound, stabVolume);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(piercePoint), 0.035f);
        Gizmos.color = new Color(0, 1, 0, 1f);
        Gizmos.DrawRay(transform.TransformPoint(piercePoint), transform.TransformPoint(piercePoint) - transform.TransformPoint(new Vector3(piercePoint.x, piercePoint.y, piercePoint.z + limit)));
    }
}