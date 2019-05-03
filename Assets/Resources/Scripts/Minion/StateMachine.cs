using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MinionStates;
using Mirror;

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * Carl Appelkvist
 * 
 * DESCRIPTION:
 * A finite state machine used to drive minion AI.
 * Requires separate class for states 
 *
 * CODE REVIEWED BY:
 * 
 */


public class StateMachine : NetworkBehaviour
{
    //General
    public uint tickRate = 10;

    //States
    public string CurrentStateName;
    public State CurrentState;

    //References
    [HideInInspector]
    public EnemySpawner EnemySpawner;
    [HideInInspector]
    public GameObject TargetEntity;
    [HideInInspector]
    public Animator AnimController;
    [HideInInspector]
    public PathfinderComponent PathFinder;
    [HideInInspector]
    public List<GameObject> Puppets;

    //Attack
    public float AttackCooldown;
    public uint AttackDamage;
    public float AttackRange;
    [HideInInspector]
    public bool CanAttack;

    //Aggro
    public float InstantAggroRange;
    public float ConeAggroRange;
    public float FOVConeAngle;
    public bool AtkPrioRunning;
    private float PostThreat;

    //Other shit
    public Vector3 RaycastOffset; //Safety offset so raycast doesn't hit ground instantly
    public bool debug;

    public void Start()
    {
        AnimController = GetComponent<Animator>();

        //If not server, disable self
        if(!isServer)
        {
            this.enabled = false;
            AnimController.applyRootMotion = false;
            return;
        }

        PathFinder = GetComponent<PathfinderComponent>();
        SetState(new WanderState(this));
        CanAttack = true;
        GetComponent<HealthComponent>().AddDeathAction(StartDeath);
        PostThreat = Mathf.NegativeInfinity;
    }

    public void SetState(State newState)
    {
        if (CurrentState != null) CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        if (System.Environment.TickCount % tickRate == 0) //Runs current state every 'tickRate' ticks
        {
            Puppets.Clear(); //TODO move this to Start(). Currently in update for dev purposes
            Puppets.AddRange(GameObject.FindGameObjectsWithTag("Player"));
            if (CurrentState != null) CurrentState.Run();
        }
    }

    //Method to check if players are within proximity
    public void CheckProximity()
    {
        int mask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact"));
        foreach (GameObject puppet in Puppets)
        {   
            //Finds closest alive puppet
            if (puppet != null && puppet.GetComponent<HealthComponent>().Health > 0)
            {
                float puppDist = Vector3.Distance(puppet.transform.position, gameObject.transform.position);

                //If within cone range and not obscured
                if (puppDist <= ConeAggroRange && Physics.Raycast(transform.position + RaycastOffset, puppet.transform.position - transform.position, out RaycastHit hit, ConeAggroRange, mask)) 
                {
                    if(hit.transform.tag.Equals("Player"))
                    {
                        
                        //If inside instant-aggro range or within vision cone
                        if(puppDist < InstantAggroRange|| Vector3.Angle(transform.forward, puppet.transform.position - transform.position) <= FOVConeAngle)
                        {
                            //Attack player
                            TargetEntity = puppet.gameObject;
                            SetState(new AttackState(this));
                        }
                    }
                }
            }
            else if (puppet == null)
            {
                Puppets.Remove(puppet);
            }
        }
    }

    //Runs when health = 0
    public void StartDeath()
    {
        this.GetComponent<Collider>().enabled = false;
        this.enabled = false;
        PathFinder.Stop();
        PathFinder.enabled = false;
        AnimController.SetTrigger("Death");
        AnimController.SetInteger("RandomAnimationIndex", Random.Range(0,3));
    }

    //Runs when death animation is complete, despawns object
    public void Despawn()
    {
        Noise.Minions.Remove(this);
        EnemySpawner.SpawnedEnemies.Remove(this.gameObject);
        Destroy(this.gameObject);
    }

    //Runs during attack animation, deals damage to player
    public void Attack()
    {
        if(!isServer) return;
        HealthComponent health = TargetEntity.GetComponent<HealthComponent>();
        if(health == null)
        {
            TargetEntity = null;
            SetState(new WanderState(this));
            return;
        }
        if (health.Health > 0)
        {
            health.Damage(AttackDamage);
        }
    }

    //Starts attack cooldown
    private IEnumerator attackTimer()
    {
        CanAttack = false;
        yield return new WaitForSeconds(AttackCooldown);
        CanAttack = true;
    }

    //Placeholder for sound
    public void Step()
    {

    }
}



//-------------------------------------------------------------
public abstract class State
{
    public abstract void Enter();

    public abstract void Run();
    
    public abstract void Exit();
}
