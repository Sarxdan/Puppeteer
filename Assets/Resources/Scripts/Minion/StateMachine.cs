using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MinionStates;
using Mirror;

#pragma warning disable IDE1006 // Naming Styles

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

public enum EnemyType
{
    Minion,
    Tank
};


public class StateMachine : NetworkBehaviour
{

    [Header("General settings")]
    public uint tickRate = 10;
    //States
    public string CurrentStateName;
    public State CurrentState;
    public EnemyType MinionType;

    //References
    [HideInInspector]
    public EnemySpawner Spawner;
    [HideInInspector]
    public GameObject TargetEntity;
    [HideInInspector]
    public Animator AnimController;
    [HideInInspector]
    public PathfinderComponent PathFinder;
    [HideInInspector]
    public List<GameObject> Puppets;




    [Header("Attack settings")]
    public float AttackCooldown;
    public uint AttackDamage;
    public float AttackRange;
    [HideInInspector]
    public bool CanAttack;
    //[HideInInspector]
    public bool ChargeStopped;
    public float ChargeAccelerationSpeed = 0.15f;
    public float CurrentChargeSpeed;
    public float StartChargeSpeed = 0.15f;
    public int ChargeCharge;


    [Header("Aggro settings")]
    public float AggroDropTime;
    public float InstantAggroRange;
    public float ConeAggroRange;
    public float FOVConeAngle;
    public bool AtkPrioRunning;
    public bool AssholeMode;
    private float PreThreat;
    private float PostThreat;

    [Header("Misc. settings")]
    public Vector3 RaycastOffset; //Safety offset so raycast doesn't hit ground instantly
    public bool debug;

    public void Start()
    {
        AnimController = GetComponent<Animator>();

        //TODO: remove when prefab gets changed from Acceleration 0
        ChargeAccelerationSpeed = 0.15f;


        GetComponent<HealthComponent>().AddDeathAction(Die);
        //If not server, disable self
        if(!isServer)
        {
            this.enabled = false;
            AnimController.applyRootMotion = false;
            return;
        }

        PathFinder = GetComponent<PathfinderComponent>();
        //If regular Minion or Tank
        if (MinionType == EnemyType.Minion)
        {
            SetState(new WanderState(this));
        }
        else
        {
            SetState(new IdleState(this));
        }

        CanAttack = true;
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
            //TODO make it work with invisible puppet, for now the tag changes from player when it becomes invisible and reverts after
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
                //If within cone range and not obscured
                if (WithinCone(transform, puppet.transform, FOVConeAngle, ConeAggroRange, InstantAggroRange))
                {     //Attack player
                    TargetEntity = puppet.gameObject;
                    if (MinionType == EnemyType.Minion)
                    {
                        SetState(new AttackState(this));
                    }
                    else
                    {
                        SetState(new BigAttackState(this));
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
    public void Die()
    {
        this.GetComponent<Collider>().enabled = false;

        if(isServer){
            this.enabled = false;
            PathFinder.Stop();
            PathFinder.enabled = false;
        }

        AnimController.SetTrigger("Death");
        AnimController.SetInteger("RandomAnimationIndex", Random.Range(0,3));
    }

    //Runs when death animation is complete, despawns object
    public void Despawn()
    {
        EnemySpawner.AllMinions.Remove(this);
        if(Spawner !=null) Spawner.LocalMinions.Remove(this);
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
            if (MinionType == EnemyType.Minion)
            {
                SetState(new WanderState(this));
            }
            else
            {
                SetState(new IdleState(this));
            }
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

    public bool WithinCone(Transform source, Transform target, float coneAngle, float coneLength, float coneIgnoreRadius)
    {
        int mask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact"));
        float distance = Vector3.Distance(source.position, target.position);
        if (distance <= coneLength && Physics.Raycast(source.position + RaycastOffset, target.position - source.position, out RaycastHit hit, coneLength, mask))
        {
            if (hit.transform.tag.Equals("Player"))
            {
                //If inside instant-aggro range or within vision cone
                if (distance < coneIgnoreRadius || Vector3.Angle(source.forward, target.position - source.position) <= coneAngle)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator chargeRoutine()
    {
        
        if (AnimController.GetBool("IsCharging") == true && AnimController.GetFloat("ChargeSpeed") < 1)
        {
            AnimController.SetFloat("ChargeSpeed", CurrentChargeSpeed + ChargeAccelerationSpeed);
        }
        Vector3 lastPos = transform.position;
        foreach (GameObject pupp in Puppets)
        {
            HealthComponent health = pupp.GetComponent<HealthComponent>();
            if (WithinCone(transform, pupp.transform, 80f, 2f, 0f))
            {
                uint chargeDamage = (uint)CurrentChargeSpeed * 10;

                //PathFinder.StuckCheck(transform, lastPos, 0.2, 0.2,);

                health.Damage(chargeDamage);
                ChargeStopped = true;
                AnimController.SetFloat("ChargeSpeed", 0);
                yield break;
            }
        }
        yield return new WaitForSeconds(0.1f);
    }


    //Isn't used at the time this file is reviewed

    private IEnumerator attackPriority()
    {
        AtkPrioRunning = true;

        foreach (GameObject pupp in Puppets)
        {
            float puppDist = Vector3.Distance(pupp.transform.position, gameObject.transform.position);
            float puppH = pupp.GetComponent<HealthComponent>().Health;
            float puppA = pupp.GetComponent<PlayerController>().Ammunition;
            float puppR = pupp.GetComponent<ReviveComponent>().DeathDelay;

            if (puppA == 0f) puppA = 150f;
            if (pupp.GetComponent<HealthComponent>().Health == 0 || puppDist > ConeAggroRange * 1.5f)
            {
                AtkPrioRunning = false;
                yield break;
            }

            if (AssholeMode)
            {
                if (pupp.GetComponent<PlayerController>().HasMedkit)
                {
                    PreThreat = ((-puppA / -puppH) * 3 - puppR) / puppDist;
                }
                else
                {
                    PreThreat = ((-puppA / -puppH) - puppR) / puppDist;
                }

            }
            else
            {
                if (pupp.GetComponent<PlayerController>().HasMedkit)
                {
                    PreThreat = (150 - puppH) / puppDist;
                }
                else
                {
                    PreThreat = (100 - puppH) / puppDist;
                }
            }

            if (PostThreat > PreThreat)
            {
                PostThreat = PreThreat;
            }
        }
        yield return new WaitForSeconds(0.1f);
    }

}



//-------------------------------------------------------------
public abstract class State
{
    public abstract void Enter();

    public abstract void Run();
    
    public abstract void Exit();
}
