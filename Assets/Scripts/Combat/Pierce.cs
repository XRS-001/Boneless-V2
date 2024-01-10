using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class Pierce : MonoBehaviour
{
    private AudioSource audioSource;
    public Collider[] colliders;
    public Vector3 piercePoint;
    [Tooltip("The damper of the piercing")]
    public float damper;
    [Tooltip("The limit of the piercing")]
    public float limit;
    [Tooltip("The velocity needed to pierce")]
    public float velocityThreshold;
    private float velocity;
    public AudioClip stabSound;
    public float stabPitch;
    public float stabVolume;
    public LayerMask pierceableLayer;
    private bool stabbed = false;
    private Collider stabbedCollider;
    private ConfigurableJoint configurableJoint;
    private Rigidbody rb;
    private GameObject hitPoint;
    private void Start()
    {
        if (!GetComponent<CollisionImpact>())
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1;
        }
        else
        {
            audioSource = GetComponent<CollisionImpact>().audioSource;
        }
        rb = GetComponent<Rigidbody>();
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
                StartCoroutine(WaitToSFX());
                stabbed = true;
                stabbedCollider = checkColliders[0];

                hitPoint = new GameObject("HitPoint");
                hitPoint.transform.position = transform.TransformPoint(new Vector3(piercePoint.x, piercePoint.y, piercePoint.z - 0.1f));
                hitPoint.transform.parent = stabbedCollider.transform;

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

                configurableJoint.xMotion = ConfigurableJointMotion.Locked;
                configurableJoint.yMotion = ConfigurableJointMotion.Locked;
                configurableJoint.zMotion = ConfigurableJointMotion.Limited;

                configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
                configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
                configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
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
    IEnumerator WaitToSFX()
    {
        yield return new WaitForSeconds(0.01f);
        if(stabbed)
        {
            Debug.Log("Pierced");
            audioSource.pitch = stabPitch;
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