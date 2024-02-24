using RootMotion.Dynamics;
using UnityEngine;
using UnityEngine.AI;
using static EnumDeclaration;
public class NPC : MonoBehaviour
{
    public Collider[] colliders;
    public float health;
    private float startingHealth;
    public bool canKill;
    public PuppetMaster puppet;
    public BehaviourPuppet behaviour;
    public NavMeshAgent agent;
    public Animator animator;
    public Transform player;
    public bool isGrabbing;
    [Tooltip("The distance at which the enemy will begin attacking")]
    public float attackDistance;
    public enemyTypeEnum enemyType;
    private float distance;
    private bool canChase = true;
    private void Start()
    {
        startingHealth = health;
        if (!player)
        {
            player = GameObject.Find("CameraDriven").transform;
        }
    }
    private void Update()
    {
        if (!canKill)
        {
            health = startingHealth;
        }
        if(health <= 0)
        {
            puppet.state = PuppetMaster.State.Dead;
        }
        if (behaviour)
        {
            if (isGrabbing)
            {
                behaviour.canGetUp = false;
            }
            else
            {
                behaviour.canGetUp = true;
            }
        }

        if(enemyType != enemyTypeEnum.dummy)
        {
            distance = Vector3.Distance(agent.transform.position, new Vector3(player.position.x, agent.transform.position.y, player.transform.position.z));
            if (distance > attackDistance && canChase == false)
            {
                Invoke(nameof(DelayChase), 0.05f);
            }
            if (!isGrabbing && distance > attackDistance && canChase)
            {
                if (enemyType == enemyTypeEnum.aggresive)
                {
                    FollowPlayer();
                }
                else if (health < startingHealth)
                {
                    FollowPlayer();
                }
            }
            if (isGrabbing || distance < attackDistance)
            {
                AttackPlayer();
            }
        }   
    }
    void FollowPlayer()
    {
        animator.SetBool("Attacking", false);
        animator.SetBool("Chasing", true);
        agent.SetDestination(player.position);

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            Vector3 lookPos = agent.steeringTarget - agent.transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, rotation, Time.deltaTime * 5f);
        }
    }
    void AttackPlayer()
    {
        canChase = false;
        agent.SetDestination(agent.transform.position);
        animator.SetBool("Attacking", true);
        agent.transform.LookAt(new Vector3(player.position.x, agent.transform.position.y, player.transform.position.z));
    }
    void DelayChase()
    {
        if(distance > attackDistance)
        {
            canChase = true;
        }
    }
}
