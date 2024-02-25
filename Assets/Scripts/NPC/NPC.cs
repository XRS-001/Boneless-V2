using RootMotion.Dynamics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.VisualScripting;
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
    public List<Blade> piercedBy = new List<Blade>();
    [Tooltip("The distance at which the enemy will begin attacking")]
    public float attackDistance;
    public enemyTypeEnum enemyType;
    private float distance;
    private bool canChase = true;
    private bool canTurn = true;
    public float despawnTime;
    private void Start()
    {
        startingHealth = health;
        if (!player)
        {
            player = GameObject.Find("CameraDriven").transform;
        }
        if (behaviour)
        {
            behaviour.onLoseBalance.unityEvent.AddListener(CanMoveFalse);
            behaviour.onRegainBalance.unityEvent.AddListener(CanMoveTrue);
        }
    }
    void CanMoveTrue()
    {
        canTurn = false;
        canChase = false;
    }
    void CanMoveFalse()
    {
        canTurn = true;
        canChase = true;
    }
    public void DealDamage(string bodyPart, float damage)
    {
        switch(bodyPart)
        {
            case "Limb":
                health -= damage;
                break;
            case "Head":
                health -= damage * 4;
                break;
            case "Torso":
                health -= damage * 2;
                break;
        }
    }
    private void Update()
    {
        if (!canKill)
        {
            health = startingHealth;
        }
        if(health <= 0 && puppet.state != PuppetMaster.State.Dead)
        {
            puppet.state = PuppetMaster.State.Dead;
            puppet.muscleDamper = 10;
            StartCoroutine(Destroy());
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
    IEnumerator Destroy()
    {
        float timer = 0f;
        while (timer < despawnTime)
        {
            if (isGrabbing || piercedBy.Count > 0)
            {
                timer = 0f;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
    void FollowPlayer()
    {
        animator.SetBool("Attacking", false);
        animator.SetBool("Chasing", true);
        agent.SetDestination(player.position);

        if(canTurn)
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
