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
            Debug.DrawRay(machine.transform.position, Vector3.forward*10, Color.green, 1);
          
            if (Physics.Raycast(attackRay, out RaycastHit hit, 10f))
            {
                if (hit.collider.tag == "Player" && machine.DownedPuppets.Contains(hit.transform.gameObject) == false)
                {
                    HealthComponent health = hit.transform.GetComponent<HealthComponent>();
                    machine.TargetEntity = hit.transform.gameObject;

                    if (health.Health == 0)
                    {
                        machine.DownedPuppets.Add(machine.TargetEntity);
                        machine.TargetEntity = null;
                    }
                    else
                    {
                        health.Damage(machine.AttackDamage);
                    }
                    foreach (GameObject i in machine.DownedPuppets)
                    {
                        if (i.GetComponent<HealthComponent>().Health > 0)
                        {
                            Debug.Log("borta");
                            machine.DownedPuppets.Remove(i);
                            machine.TargetEntity = i;
                        }
                    }
                    //for (int i = 0; i < machine.DownedPuppets.Count; i++)
                    //{
                    //    // This was pushed with error for some reason
                    //    //if (machine.DownedPuppets.) 

                    //}
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