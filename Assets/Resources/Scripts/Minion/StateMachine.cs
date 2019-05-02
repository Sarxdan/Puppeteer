﻿using System.Collections;
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
    //General
    public uint tickRate = 10;
    public string CurrentStateName;
    public State CurrentState;
    [HideInInspector]
    public EnemySpawner EnemySpawner;
    public GameObject TargetEntity;
    public float ProxyCooldown;
    public float AttackCooldown;
    public uint AttackDamage;
    public float AttackRange;
    public bool CanAttack;
    public float InstantAggroRange;
    public float ConeAggroRange;
    public float FOVConeAngle;
    public Vector3 RaycastOffset;

    [HideInInspector]
    public List<GameObject> Puppets;
    [HideInInspector]
    public bool CoRunning;
    private float closestPuppDist = 0;

    [HideInInspector]
    public Animator AnimController;
    [HideInInspector]
    public PathfinderComponent PathFinder;

    public bool eDebug;
    //Pathfind component reference (pathFinder)

    public void Start()
    {
        PathFinder = GetComponent<PathfinderComponent>();
        AnimController = GetComponent<Animator>();
        SetState(new WanderState(this));
        CanAttack = true;
        GetComponent<HealthComponent>().AddDeathAction(StartDeath);
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
            Puppets.Clear();
            Puppets.AddRange(GameObject.FindGameObjectsWithTag("Player"));
            if (CurrentState != null) CurrentState.Run();

        }
    }

    //REEEEEE FUCKING FIXA FRAMTIDA FLOOF
    public void CheckProxy()
    {
        int mask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact"));
        closestPuppDist = Mathf.Infinity;
        foreach (GameObject pupp in Puppets)
        {
            if (pupp != null && pupp.GetComponent<HealthComponent>().Health > 0)
            {
                float puppDist = Vector3.Distance(pupp.transform.position, gameObject.transform.position);
                if (closestPuppDist == 0 || closestPuppDist > puppDist)
                {
                    closestPuppDist = puppDist;
                }

                //If within cone range
                if (closestPuppDist <= ConeAggroRange)
                {
                    //If outside instant-aggro range
                    if(closestPuppDist > InstantAggroRange)
                    {
                        //If within vision cone angle and in direct line of sight
                        if(Vector3.Angle(transform.forward, pupp.transform.position - transform.position) <= FOVConeAngle &&
                        Physics.Raycast(transform.position + RaycastOffset, pupp.transform.position - transform.position, out RaycastHit hit, ConeAggroRange, mask))
                        {
                            if(hit.transform.tag.Equals("Player")){
                                TargetEntity = pupp.gameObject;
                                SetState(new AttackState(this));
                            }
                        }
                    }
                    else
                    {
                        //Instant aggro
                        TargetEntity = pupp.gameObject;
                        SetState(new AttackState(this));
                    }
                
                    
                }
            }
            else if (pupp == null)
            {
                Puppets.Remove(pupp);
            }
        }
    }

    public void StartDeath(){
        this.GetComponent<Collider>().enabled = false;
        this.enabled = false;
        PathFinder.Stop();
        PathFinder.enabled = false;
        AnimController.SetTrigger("Death");
        AnimController.SetInteger("RandomAnimationIndex", Random.Range(0,3));
    }

    public void Despawn(){
        EnemySpawner.SpawnedEnemies.Remove(this.gameObject);
        Destroy(this.gameObject);
    }

    public void Attack(){
        HealthComponent health = TargetEntity.GetComponent<HealthComponent>();
        if (health.Health > 0)
        {
            health.Damage(AttackDamage);
        }
    }


    private IEnumerator attackTimer(){
        CanAttack = false;
        yield return new WaitForSeconds(AttackCooldown);
        CanAttack = true;
    }
}



//-------------------------------------------------------------
public abstract class State
{
    public abstract void Enter();

    public abstract void Run();
    
    public abstract void Exit();
}
