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
    public bool AttackRdy;
    public List<GameObject> DownedPuppets;
    //Pathfind component reference (pathFinder)

    public void Start()
    {
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
            if (CurrentState != null) CurrentState.Run();
        }

    }

    //IEnumerator hitRoutine()
    //{
    //    Debug.Log(AttackRdy);
    //    AttackRdy = true;
    //    yield return new WaitForSecondsRealtime(5);
    //    StopCoroutine("waitRoutine");

    //}
}



//-------------------------------------------------------------

public abstract class State
{
    public abstract void Enter();

    public abstract void Run();

    public abstract void Exit();
}
