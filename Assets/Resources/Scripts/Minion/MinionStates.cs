using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingState : State
{
    public override void Enter()
    {

    }

    public override void Run()
    {
        Ray visionRay = new Ray(transform.position, Vector3.forward);
        if (Physics.Raycast(visionRay, out RaycastHit hit, 10f))
        {
            if (hit.collider.tag == "Player")
            {
                transform.position = Vector3.MoveTowards(transform.position, GameObject.FindWithTag("Player").transform.position, Time.time);
            }
        }
    }

    public override void Exit()
    {

    }
}

//-------------------------------------------------------------

public class ReturnToSpawnerState : State
{
    public override void Enter()
    {

    }

    public override void Run()
    {

    }

    public override void Exit()
    {

    }
}

//-------------------------------------------------------------

public class WanderState : State
{
    public override void Enter()
    {

    }

    public override void Run()
    {

    }

    public override void Exit()
    {

    }
}

//-------------------------------------------------------------

public class SeekState : State
{
    public override void Enter()
    {

    }

    public override void Run()
    {

    }

    public override void Exit()
    {

    }
}
