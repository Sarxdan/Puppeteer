﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinionStates
{
    public class AttackingState : State
    {

        private StateMachine machine;

        public AttackingState(StateMachine machine)
        {
            this.machine = machine;
        }
        

        public override void Enter()
        {
            
        }

        public override void Run()
        {
            machine.StartCoroutine("ProxyRoutine");

            Ray attackRay = new Ray(machine.transform.position, machine.transform.forward);
            if (machine.eDebug == true) Debug.DrawRay(machine.transform.position, Vector3.forward * machine.AttackRange, Color.green, 0.2f);

            if (machine.Follow == true)
            {
                //machine.transform.position = Vector3.MoveTowards(machine.transform.position, machine.TargetEntity.transform.position, 0.3f);
                machine.PathFinder.MoveTo(machine.TargetEntity.transform.position);
            }
            if (Physics.Raycast(attackRay, out RaycastHit target, machine.AttackRange))
            {
                if (target.collider.tag == ("Player"))
                {
                    machine.TargetEntity = target.transform.gameObject;
                    if (machine.CoRunning == false)
                    {
                        machine.StartCoroutine("AttackRoutine", target.transform.gameObject);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                machine.StopCoroutine("AttackRoutine");
                machine.CoRunning = false;
                machine.TargetEntity = null;
                return;
            }
        }
    
        public override void Exit()
        {

        }
    }

    //-------------------------------------------------------------

    public class ReturnToSpawnerState : State
    {
        private StateMachine machine;

        public ReturnToSpawnerState(StateMachine machine)
        {
            this.machine = machine;
        }
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
        private StateMachine machine;
        private Vector3 destination;

        public WanderState(StateMachine machine)
        {
            this.machine = machine;
        }
        public override void Enter()
        {
            destination = machine.EnemySpawner.GetNearbyDestination();
            machine.AnimController.SetBool("Walking", true);
            machine.PathFinder.MoveTo(destination);
        }

        public override void Run()
        {
            if(!machine.PathFinder.HasPath){
                machine.SetState(new WanderState(machine));
            }
        }

        public override void Exit()
        {
            machine.AnimController.SetBool("Walking", false);
        }
    }

    //-------------------------------------------------------------

    public class SeekState : State
    {
        private StateMachine machine;

        public SeekState(StateMachine machine)
        {
            this.machine = machine;
        }
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
}
