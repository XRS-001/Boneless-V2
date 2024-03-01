using System.Collections;
using System.Threading;
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
    [Header("Stab Data")]
    public Collider[] colliders;
    public float pierceDamage;
    public GameObject decal;
    [Tooltip("The axis of the blade")]
    public upDirection stabDirection;
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
    public bool stabbed = false;
    private Collider stabbedCollider;
    private ConfigurableJoint stabbedJoint;
    private Rigidbody rb;
    private GameObject hitPoint;
    private bool canStab = true;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        velocity = rb.velocity.magnitude;
        if (Physics.CheckSphere(transform.TransformPoint(piercePoint), 0.001f, pierceableLayer) && !stabbed && velocity > velocityThreshold)
        {
            TryStab();
        }
        if (hitPoint)
            if (stabbed && Vector3.Distance(hitPoint.transform.position, transform.TransformPoint(piercePoint)) < 0.2f && !canStab)
                UnStab();
    }
    public void UnStab()
    {
        stabbed = false;
        Destroy(stabbedJoint);
        Destroy(hitPoint);
        NPC npc = stabbedCollider.transform.root.GetComponent<NPC>();
        if (npc)
        {
            npc.piercedBy.Remove(this);
            foreach (Collider ragdollCollider in npc.colliders)
            {
                foreach (Collider collider in colliders)
                {
                    Physics.IgnoreCollision(collider, ragdollCollider, false);
                }
            }
        }
        StartCoroutine(DelayCanStab());
        stabbedCollider = null;
    }
    void TryStab()
    {
        Collider[] checkColliders = Physics.OverlapSphere(transform.TransformPoint(piercePoint), 0.001f, pierceableLayer);
        if (checkColliders[0].transform.root.name != gameObject.name && canStab)
        {
            stabbedCollider = checkColliders[0];

            Vector3 velocityDirection = Vector3.zero;
            switch (stabDirection)
            {
                case upDirection.forward:
                    velocityDirection = transform.forward;
                    break;

                case upDirection.up:
                    velocityDirection = transform.up;
                    break;

                case upDirection.right:
                    velocityDirection = transform.right;
                    break;
            }
            if (Vector3.Dot(rb.velocity, velocityDirection) > 1.25f)
            {
                stabbed = true;

                StartCoroutine(WaitToSFX());

                hitPoint = new GameObject("HitPoint");
                hitPoint.transform.position = transform.TransformPoint(new Vector3(piercePoint.x, piercePoint.y, piercePoint.z - 0.2f));
                hitPoint.transform.parent = stabbedCollider.transform;

                foreach (Collider ragdollCollider in stabbedCollider.transform.root.GetComponentsInChildren<Collider>())
                {
                    foreach (Collider collider in colliders)
                    {
                        Physics.IgnoreCollision(collider, ragdollCollider, true);
                    }
                }
                stabbedJoint = gameObject.AddComponent<ConfigurableJoint>();

                if (stabbedCollider.GetComponent<Rigidbody>())
                    stabbedJoint.connectedBody = stabbedCollider.GetComponent<Rigidbody>();
                else
                    stabbedJoint.connectedBody = stabbedCollider.transform.parent.GetComponent<Rigidbody>();

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
    IEnumerator DelayCanStab()
    {
        yield return new WaitForSeconds(0.5f);
        canStab = true;
    }
    IEnumerator WaitToSFX()
    {
        yield return new WaitForSeconds(0.025f);
        if (stabbed)
        {
            AudioSource.PlayClipAtPoint(stabSound, hitPoint.transform.position, 0.15f);

            NPC npc = stabbedCollider.transform.root.GetComponent<NPC>();
            if (npc)
            {
                npc.piercedBy.Add(this);
                npc.DealDamage(stabbedCollider.transform.tag, pierceDamage, false);
            }
        }
        yield return new WaitForSeconds(0.5f);
        if(stabbed)
            canStab = false;
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