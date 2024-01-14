using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit;

public class Pierce : MonoBehaviour
{
    private AudioSource audioSource;
    [Header("Stab Data")]
    public Collider[] colliders;
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
    private List<GameObject> spawnedDecals = new List<GameObject>();
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
                foreach(DecalProjector decal in objectDecals)
                {
                    decal.fadeFactor += 0.25f;
                }

                StartCoroutine(WaitToSFX());
                stabbed = true;
                stabbedCollider = checkColliders[0];
                NPC stabbedNPC = stabbedCollider.transform.root.GetComponent<NPC>();
                stabbedNPC.isBleeding = true;

                hitPoint = new GameObject("HitPoint");
                hitPoint.transform.position = transform.TransformPoint(new Vector3(piercePoint.x, piercePoint.y, piercePoint.z - 0.15f));
                hitPoint.transform.parent = stabbedCollider.transform;

                spawnedDecals.Add(Instantiate(bloodDecal));
                spawnedDecals[spawnedDecals.Count - 1].transform.position = transform.TransformPoint(piercePoint);
                spawnedDecals[spawnedDecals.Count - 1].transform.rotation = transform.rotation;
                spawnedDecals[spawnedDecals.Count - 1].transform.parent = stabbedCollider.transform;
                StartCoroutine(DelayOpacity(spawnedDecals[spawnedDecals.Count - 1].GetComponent<DecalProjector>(), stabbedNPC));

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
    IEnumerator DelayOpacity(DecalProjector decal, NPC npc)
    {
        float timer = 0f;
        while (timer < 15)
        {
            decal.fadeFactor = Mathf.Lerp(1, 0, timer / 15);
            timer += Time.deltaTime;
            if (timer >= 15)
            {
                spawnedDecals.Remove(decal.gameObject);
                Destroy(decal.gameObject);
                if(spawnedDecals.Count == 0)
                {
                    npc.isBleeding = false;
                }
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