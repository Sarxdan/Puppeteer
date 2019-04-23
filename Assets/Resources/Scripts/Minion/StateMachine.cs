using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public uint tickRate = 10;
    public State CurrentState;
    public GameObject EnemySpawner;
    public GameObject TargetEntity;
    public float AttackCooldown;
    public uint AttackDamage;



    //Pathfind component reference (pathFinder)

    public void Start()
    {

        //pathFinder = getComponent.....
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
}

//-------------------------------------------------------------

public abstract class State : MonoBehaviour
{
    public abstract void Enter();

    public abstract void Run();

    public abstract void Exit();
}
