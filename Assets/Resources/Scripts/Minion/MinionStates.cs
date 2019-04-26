using System.Collections;
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
            Ray attackRay = new Ray(machine.transform.position, Vector3.forward);
            Debug.DrawRay(machine.transform.position, Vector3.forward * machine.AttackRange, Color.green, 1);

            if (Physics.Raycast(attackRay, out RaycastHit target, machine.AttackRange))
            {
                if (target.collider.tag == ("Player"))
                {
                    machine.TargetEntity = target.transform.gameObject;
                    if (machine.coRunning == false)
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
                machine.coRunning = false;
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

        public WanderState(StateMachine machine)
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