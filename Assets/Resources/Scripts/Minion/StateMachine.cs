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
    public GameObject EnemySpawner;
    public GameObject TargetEntity;
    public float AttackCooldown;
    public uint AttackDamage;
    public float AttackRange;
    //public bool AttackRdy;
    public List<GameObject> Puppets;
    //public GameObject[] Puppets;
    public bool coRunning;
    //Pathfind component reference (pathFinder)

    public void Start()
    {
        Puppets.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        SetState(new AttackingState(this));
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

            foreach (GameObject pupp in Puppets)
            {
                float puppDist = Vector3.Distance(pupp.transform.position, gameObject.transform.position);
            }
            if (CurrentState != null) CurrentState.Run();
        }
    }

    private IEnumerator AttackRoutine(GameObject target)
    {
        coRunning = true;
        HealthComponent health = target.transform.GetComponent<HealthComponent>();
        while (target != null && health.Health > 0)
        {
            health.Damage(AttackDamage);
            yield return new WaitForSeconds(AttackCooldown);
            if (health.Health == 0)
            {
                coRunning = false;
                yield break;
            }
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
