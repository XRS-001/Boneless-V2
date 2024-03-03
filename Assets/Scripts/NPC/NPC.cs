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
    public bool dead;
    private float startingHealth;
    public bool canKill;
    private bool canHit = true;
    private float startDamper;
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
    private bool stunned = false;
    private bool canChase = true;
    private bool isStanding = true;
    private bool isAttacking = false;
    private bool canPunch = true;
    public float despawnTime;
    public bool playHitSound;
    public AudioClip[] hitAudios;
    public AudioClip[] deathAudios;
    public AudioSource hitAudioSource;
    private void Start()
    {
        startDamper = puppet.muscleDamper;
        startingHealth = health;
        if (!player)
        {
            player = GameObject.Find("CameraDriven").transform;
        }
        if (behaviour)
        {
            behaviour.onLoseBalance.unityEvent.AddListener(Fall);
            behaviour.onRegainBalance.unityEvent.AddListener(Stand);
        }
    }
    void Stand()
    {
        puppet.muscleDamper = startDamper;
        isStanding = true;
        puppet.angularLimits = false;
        puppet.internalCollisions = false;
    }
    void Fall()
    {
        puppet.muscleDamper = 25;
        puppet.angularLimits = true;
        puppet.internalCollisions = true;
        isStanding = false;
    }
    public void DealDamage(string bodyPart, float damage, bool blunt)
    {
        if(canHit && !blunt)
            switch (bodyPart)
            {
                case "Limb":
                    health -= damage;

                    if(playHitSound)
                    {
                        if (health > 0)
                            hitAudioSource.PlayOneShot(hitAudios[Random.Range(0, hitAudios.Length - 1)]);
                        else if (!dead)
                        {
                            hitAudioSource.PlayOneShot(deathAudios[Random.Range(0, deathAudios.Length - 1)]);
                        }
                    }

                    break;
                case "Head":
                    health -= damage * 4;
                    break;
                case "Torso":
                    stunned = true;
                    health -= damage * 2;

                    if (playHitSound)
                    {
                        if (health > 0)
                            hitAudioSource.PlayOneShot(hitAudios[Random.Range(0, hitAudios.Length - 1)]);
                        else if (!dead)
                        {
                            hitAudioSource.PlayOneShot(deathAudios[Random.Range(0, deathAudios.Length - 1)]);
                        }
                    }

                    if (animator)
                        animator.SetTrigger("Hit");
                    Invoke(nameof(UnStun), 1f);
                    break;
            }
        else if (canHit)
        {
            health -= damage;
            if(bodyPart == "Torso")
            {
                stunned = true;

                if (animator)
                    animator.SetTrigger("Hit");
                Invoke(nameof(UnStun), 1f);
            }

            if (playHitSound)
            {
                if (health > 0)
                    hitAudioSource.PlayOneShot(hitAudios[Random.Range(0, hitAudios.Length - 1)]);
                else if (!dead)
                {
                    hitAudioSource.PlayOneShot(deathAudios[Random.Range(0, deathAudios.Length - 1)]);
                }
            }
        }

        if (blunt)
            StartCoroutine(DelayCanHit());
    }
    IEnumerator DelayCanHit()
    {
        canHit = false;
        yield return new WaitForSeconds(0.25f);
        canHit = true;
    }
    void UnStun()
    {
        stunned = false;
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
            dead = true;
            puppet.muscleDamper = 25;
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
                Invoke(nameof(DelayChase), 0.2f);
            }
            if (!isGrabbing && distance > attackDistance && canChase && isStanding && !stunned)
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
        if (isAttacking && canPunch)
        {
            int randomNum = Random.Range(1, 3);
            switch (randomNum)
            {
                case 1:
                    animator.SetBool("Punch2", false);
                    animator.SetBool("Punch1", true);
                    break;

                case 2:
                    animator.SetBool("Punch1", false);
                    animator.SetBool("Punch2", true);
                    break;
            }
            canPunch = false;
            Invoke(nameof(DelayCanPunch), 1);
        }
    }
    void DelayCanPunch()
    {
        canPunch = true;
        animator.SetBool("Punch1", false);
        animator.SetBool("Punch2", false);
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
        isAttacking = false;
        animator.SetBool("Chasing", true);
        animator.SetBool("Attacking", false);

        agent.SetDestination(player.position);

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            Vector3 lookPos = agent.steeringTarget - agent.transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, rotation, Time.deltaTime * 5f);
        }
        Vector3 localVelocity = agent.transform.InverseTransformPoint(agent.steeringTarget - agent.velocity).normalized;

        animator.SetFloat("X", localVelocity.x);
        animator.SetFloat("Y", localVelocity.z);
    }
    void AttackPlayer()
    {
        canChase = false;
        isAttacking = true;
        animator.SetBool("Attacking", true);
        agent.SetDestination(agent.transform.position);
        agent.transform.LookAt(new Vector3(player.position.x, agent.transform.position.y, player.transform.position.z));

        Vector3 localVelocity = agent.transform.InverseTransformPoint(agent.steeringTarget - agent.velocity).normalized;

        animator.SetFloat("X", localVelocity.x);
        animator.SetFloat("Y", localVelocity.z);
    }
    void DelayChase()
    {
        if(distance > attackDistance)
        {
            canChase = true;
        }
    }
}
