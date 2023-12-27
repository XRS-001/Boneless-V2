using UnityEngine;
using UnityEngine.InputSystem;

public class GrabPhysics : MonoBehaviour
{
    public enum handTypeEnum { Left, Right }
    public handTypeEnum handType;
    public Rigidbody rb;
    private SetPose poseSetup;

    [Tooltip("The forearm collider to ignore on grab")]
    public Collider forearmCollider;
    [Tooltip("The collider group of the hand")]
    public GameObject colliderGroup;

    public InputActionProperty grabInputSource;
    public float radius = 0.1f;
    [Tooltip("The local offset for the grab zone of the hand")]
    public Vector3 grabZoneOffset;
    public LayerMask grabLayer;

    private ConfigurableJoint configJoint;
    private GrabTwoAttach grab;
    public bool isGrabbing = false;
    private void Start()
    {
        poseSetup = GetComponent<SetPose>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;
        bool isGrabButtonPressedThisFrame = grabInputSource.action.WasPressedThisFrame();

        if (isGrabButtonPressedThisFrame && !isGrabbing)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position - (transform.rotation * grabZoneOffset), radius, grabLayer, QueryTriggerInteraction.Ignore);
            if(nearbyColliders.Length > 0)
            {
                Rigidbody nearbyRigidbody = FindClosestInteractable(nearbyColliders);

                if(nearbyRigidbody != null )
                {
                    grab = nearbyRigidbody.GetComponent<GrabTwoAttach>();

                    if (!grab.isGrabbing)
                    {
                        grab.handGrabbing = this;
                        configJoint = gameObject.AddComponent<ConfigurableJoint>();
                        configJoint.autoConfigureConnectedAnchor = false;
                        grab.SetAttachPoint(handType);
                        grab.isGrabbing = true;
                        transform.rotation = nearbyRigidbody.rotation * Quaternion.Euler(grab.attachRotation);

                        configJoint.xMotion = ConfigurableJointMotion.Locked;
                        configJoint.yMotion = ConfigurableJointMotion.Locked;
                        configJoint.zMotion = ConfigurableJointMotion.Locked;

                        configJoint.angularXMotion = ConfigurableJointMotion.Locked;
                        configJoint.angularYMotion = ConfigurableJointMotion.Locked;
                        configJoint.angularZMotion = ConfigurableJointMotion.Locked;

                        configJoint.connectedBody = nearbyRigidbody;
                        configJoint.connectedAnchor = grab.attachPoint;

                        foreach (Collider collider in grab.colliders)
                        {
                            Physics.IgnoreCollision(forearmCollider, collider, true);
                        }
                        isGrabbing = true;
                    }
                    else if (grab.twoHanded)
                    {
                        grab.secondHandGrabbing = this;
                        configJoint = gameObject.AddComponent<ConfigurableJoint>();
                        configJoint.autoConfigureConnectedAnchor = false;
                        grab.SetAttachPoint(handType);
                        grab.isTwoHandGrabbing = true;
                        transform.rotation = nearbyRigidbody.rotation * Quaternion.Euler(grab.attachRotation);

                        configJoint.xMotion = ConfigurableJointMotion.Locked;
                        configJoint.yMotion = ConfigurableJointMotion.Locked;
                        configJoint.zMotion = ConfigurableJointMotion.Locked;

                        configJoint.angularXMotion = ConfigurableJointMotion.Locked;
                        configJoint.angularYMotion = ConfigurableJointMotion.Locked;
                        configJoint.angularZMotion = ConfigurableJointMotion.Locked;

                        configJoint.connectedBody = nearbyRigidbody;
                        configJoint.connectedAnchor = grab.attachPoint;

                        foreach (Collider collider in grab.colliders)
                        {
                            Physics.IgnoreCollision(forearmCollider, collider, true);
                        }
                        colliderGroup.SetActive(false);
                        grab.handGrabbing.colliderGroup.SetActive(false);
                        Physics.IgnoreCollision(forearmCollider, grab.handGrabbing.forearmCollider, true);
                        isGrabbing = true;
                    }
                    grab.SetPose(handType);
                    poseSetup.pose = grab.pose;
                    poseSetup.SetupPose();
                }
                else
                {
                    configJoint = gameObject.AddComponent<ConfigurableJoint>();

                    configJoint.xMotion = ConfigurableJointMotion.Locked;
                    configJoint.yMotion = ConfigurableJointMotion.Locked;
                    configJoint.zMotion = ConfigurableJointMotion.Locked;

                    configJoint.angularXMotion = ConfigurableJointMotion.Locked;
                    configJoint.angularYMotion = ConfigurableJointMotion.Locked;
                    configJoint.angularZMotion = ConfigurableJointMotion.Locked;

                    configJoint.autoConfigureConnectedAnchor = false;
                    configJoint.connectedAnchor = transform.position;
                    isGrabbing = true;
                }
            }
        }
        else if(!isGrabButtonPressed && isGrabbing)
        {
            isGrabbing = false;
            if (grab != null)
            {
                if (!grab.isTwoHandGrabbing)
                {
                    if (configJoint)
                    {
                        Destroy(configJoint);
                    }
                    foreach (Collider collider in grab.colliders)
                    {
                        Physics.IgnoreCollision(forearmCollider, collider, false);
                    }
                    grab.handGrabbing = null;
                    grab.isGrabbing = false;
                }
                else
                {
                    if (configJoint)
                    {
                        Destroy(configJoint);
                    }
                    foreach (Collider collider in grab.colliders)
                    {
                        Physics.IgnoreCollision(forearmCollider, collider, false);
                    }
                    Physics.IgnoreCollision(forearmCollider, grab.handGrabbing.forearmCollider, true);
                    colliderGroup.SetActive(true);
                    grab.handGrabbing.colliderGroup.SetActive(true);
                    grab.isTwoHandGrabbing = false;
                    if (grab.handGrabbing == this)
                    {
                        grab.secondHandGrabbing.colliderGroup.SetActive(true);
                    }
                    grab.secondHandGrabbing = null;
                }
                poseSetup.UnSetPose();
            }
            else
            {
                Destroy(configJoint);
            }
            grab = null;
        }
    }
    public Rigidbody FindClosestInteractable(Collider[] collidersGrabbed)
    {
        if (!collidersGrabbed[0].attachedRigidbody)
        {
            return null;
        }
        Rigidbody closestRigidbody = collidersGrabbed[0].attachedRigidbody;
        Vector3 closestPosition = collidersGrabbed[0].attachedRigidbody.position;
        float closestDistance = Vector3.Distance(transform.position, closestPosition);

        // Loop through all positions and find the closest one
        for (int i = 1; i < collidersGrabbed.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, collidersGrabbed[i].attachedRigidbody.position);

            if (distance < closestDistance)
            {
                closestRigidbody = collidersGrabbed[i].attachedRigidbody;
                closestDistance = distance;
            }
        }
        return closestRigidbody;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position - (transform.rotation * grabZoneOffset), radius);
    }
}
