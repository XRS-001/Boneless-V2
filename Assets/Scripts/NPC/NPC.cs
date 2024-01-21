using RootMotion.Dynamics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    public float health;
    public PuppetMaster puppet;
    public BehaviourPuppet puppetFall;
    public Animator animator;
    private bool canDamage = true;

    private float speed;
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    public float timeBetweenAttacks;
    private bool alreadyAttacked;

    public float attackRange;
    public bool playerInSightRange, playerInAttackRange;

    public GrabDynamic[] grabbingPoints;
    private void Awake()
    {
        speed = agent.speed;
        player = GameObject.Find("TrackedObjects").transform;
    }
    // Update is called once per frame
    void Update()
    {
        puppetFall.canGetUp = true;

        foreach (GrabDynamic grab in grabbingPoints)
        {
            if(grab.isGrabbing)
            {
                puppetFall.canGetUp = false;
            }
        }
        playerInAttackRange = Physics.CheckCapsule(agent.transform.position, new Vector3(agent.transform.position.x, agent.height, agent.transform.position.z), attackRange, whatIsPlayer);
        if (!playerInAttackRange) ChasePlayer();
        if (playerInAttackRange) AttackPlayer();
        health = Mathf.Clamp(health, 0, 100);
    }
    private void ChasePlayer()
    {
        animator.SetBool("Attacking", false);
        agent.speed = speed;

        agent.SetDestination(player.position);
        if (agent.remainingDistance > agent.stoppingDistance)
        {
            Vector3 lookPos = agent.steeringTarget - agent.transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, rotation, Time.deltaTime * 5f);
        }

        Vector3 localVelocity = agent.transform.InverseTransformDirection(agent.velocity);

        animator.SetFloat("X", localVelocity.x);
        animator.SetFloat("Y", localVelocity.z);
    }
    private void AttackPlayer()
    {
        animator.SetBool("Attacking", true);

        agent.SetDestination(player.position);
        if (agent.remainingDistance > agent.stoppingDistance)
        {
            Vector3 lookPos = agent.steeringTarget - agent.transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, rotation, Time.deltaTime * 5f);
        }

        if (!alreadyAttacked)
        {
            animator.SetTrigger("Punch");
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    public void DealDamage(float damage, float delayTime)
    {
        if (canDamage)
        {
            health -= damage;
            StartCoroutine(Delay(delayTime));
            if (health <= 0)
            {
                puppet.state = PuppetMaster.State.Dead;
            }
        }
    }
    IEnumerator Delay(float delayTime)
    {
        canDamage = false;
        yield return new WaitForSeconds(delayTime);
        canDamage = true;
    }
}
