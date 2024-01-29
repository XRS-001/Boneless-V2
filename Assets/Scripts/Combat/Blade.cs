using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static EnumDeclaration;
[System.Serializable]
public class SlicePoint
{
    public Vector3 start;
    public Vector3 end;
}
public class Blade : MonoBehaviour
{
    private AudioSource audioSource;
    [Header("Stab Data")]
    public Collider[] colliders;
    public float pierceDamage;
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
    public bool stabbed = false;
    private Collider stabbedCollider;
    private ConfigurableJoint stabbedJoint;
    private Rigidbody rb;
    private GameObject hitPoint;
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
            TryStab();
        }
        if (stabbed && Vector3.Distance(hitPoint.transform.position, transform.TransformPoint(piercePoint)) < 0.1f)
        {
            stabbed = false;
            Destroy(stabbedJoint);
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
    void TryStab()
    {
        Collider[] checkColliders = Physics.OverlapSphere(transform.TransformPoint(piercePoint), 0.035f, pierceableLayer);
        if (checkColliders[0].transform.root.name != gameObject.name)
        {
            stabbedCollider = checkColliders[0];

            stabbedCollider.Raycast(new Ray(transform.position, stabbedCollider.ClosestPoint(transform.TransformPoint(piercePoint)) - transform.position), out RaycastHit hitInfo, float.PositiveInfinity);

            if (Vector3.Dot(stabAxis, hitInfo.normal) < -0.7f)
            {
                stabbed = true;

                StartCoroutine(WaitToSFX());

                hitPoint = new GameObject("HitPoint");
                hitPoint.transform.position = transform.TransformPoint(new Vector3(piercePoint.x, piercePoint.y, piercePoint.z - 0.125f));
                hitPoint.transform.parent = stabbedCollider.transform;

                foreach (Collider ragdollCollider in stabbedCollider.transform.root.GetComponentsInChildren<Collider>())
                {
                    foreach (Collider collider in colliders)
                    {
                        Physics.IgnoreCollision(collider, ragdollCollider, true);
                    }
                }
                stabbedJoint = gameObject.AddComponent<ConfigurableJoint>();
                stabbedJoint.connectedBody = stabbedCollider.GetComponent<Rigidbody>();
                SoftJointLimit jointLimit = stabbedJoint.linearLimit;
                jointLimit.limit = limit;
                stabbedJoint.linearLimit = jointLimit;

                JointDrive zDrive = stabbedJoint.zDrive;
                zDrive.positionDamper = damper;
                stabbedJoint.zDrive = zDrive;
                switch (stabDirection)
                {
                    case upDirection.forward:
                        stabbedJoint.xMotion = ConfigurableJointMotion.Locked;
                        stabbedJoint.yMotion = ConfigurableJointMotion.Locked;
                        stabbedJoint.zMotion = ConfigurableJointMotion.Limited;
                        break;

                    case upDirection.up:
                        stabbedJoint.xMotion = ConfigurableJointMotion.Locked;
                        stabbedJoint.yMotion = ConfigurableJointMotion.Limited;
                        stabbedJoint.zMotion = ConfigurableJointMotion.Locked;
                        break;

                    case upDirection.right:
                        stabbedJoint.xMotion = ConfigurableJointMotion.Limited;
                        stabbedJoint.yMotion = ConfigurableJointMotion.Locked;
                        stabbedJoint.zMotion = ConfigurableJointMotion.Locked;
                        break;
                }
                stabbedJoint.angularXMotion = ConfigurableJointMotion.Locked;
                stabbedJoint.angularYMotion = ConfigurableJointMotion.Locked;
                stabbedJoint.angularZMotion = ConfigurableJointMotion.Locked;
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
                Destroy(decal.gameObject);
            }
            yield return null;
        }
    }
    IEnumerator WaitToSFX()
    {
        yield return new WaitForSeconds(0.025f);
        if (stabbed)
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
        Gizmos.color = new Color(1, 0, 0, 1f);
    }
}