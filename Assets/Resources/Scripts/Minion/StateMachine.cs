using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MinionStates;
/*
 * AUTHOR:
 * Ludvig Björk Förare
 * Carl Appelkvist
 * 
 * DESCRIPTION:
 * 
 * 
 * CODE REVIEWED BY:
 * 
 */


public class StateMachine : MonoBehaviour
{
    public uint tickRate = 10;
    public State CurrentState;
    public EnemySpawner EnemySpawner;
    public GameObject TargetEntity;
    public float ProxyCooldown;
    public float AttackCooldown;
    public uint AttackDamage;
    public float AttackRange;
    public float AggroRange;
    public List<GameObject> Puppets;
    public bool CoRunning;
    public bool EnInRange;
    public float ClosestPuppDist = 0;
    public bool Follow;

    public Animator AnimController;
    public Rigidbody rigidbody;

    public bool eDebug;
    //Pathfind component reference (pathFinder)
    public PathfinderComponent PathFinder;

    public void Start()
    {
        PathFinder = GetComponent<PathfinderComponent>();
        AnimController = GetComponent<Animator>();
        Follow = false;
        SetState(new WanderState(this));
    }

    public void SetState(State newState)
    {
        if (CurrentState != null) CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();

    }

    public void Update()
    {
        if (System.Environment.TickCount % tickRate == 0)
        {
<<<<<<< HEAD
=======
            Puppets.Clear();
            Puppets.AddRange(GameObject.FindGameObjectsWithTag("Player"));
>>>>>>> b9320e9e4251c0022f61a6c43fab3a530e1c0787
            if (CurrentState != null) CurrentState.Run();

        }
    }

    private IEnumerator AttackRoutine(GameObject target)
    {
        CoRunning = true;
        HealthComponent health = target.transform.GetComponent<HealthComponent>();
        while (target != null && health.Health > 0)
        {
            
            health.Damage(AttackDamage);
            yield return new WaitForSeconds(AttackCooldown);
            if (health.Health == 0)
            {
                CoRunning = false;
                yield break;
            }
        }
    }
    //REEEEEE FUCKING FIXA FRAMTIDA FLOOF
    private IEnumerator ProxyRoutine()
    {
        ClosestPuppDist = Mathf.Infinity;
        foreach (GameObject pupp in Puppets)
        {
            if (pupp != null)
            {
                float puppDist = Vector3.Distance(pupp.transform.position, gameObject.transform.position);
                if (ClosestPuppDist == 0)
                {
                    Follow = false;
                    ClosestPuppDist = puppDist;
                }
                else if (ClosestPuppDist > puppDist)
                {
                    Follow = false;
                    ClosestPuppDist = puppDist;
                }
                if (ClosestPuppDist <= AggroRange)
                {
                    Follow = true;
                    TargetEntity = pupp.gameObject;
                    //EnInRange = true;
                    if (CoRunning == false)
                    {
                        StartCoroutine("AttackRoutine", TargetEntity);
                    }
                
                    
                }
            }
            else if (pupp == null)
            {
                Puppets.Remove(pupp);
            }
            yield return new WaitForSeconds(ProxyCooldown);
        }
    }
}

//-------------------------------------------------------------
public abstract class State
{
    public abstract void Enter();

    public abstract void Run();
    
    public abstract void Exit();
}
