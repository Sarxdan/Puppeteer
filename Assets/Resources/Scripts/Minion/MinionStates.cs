using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinionStates
{
    public class AttackState : State
    {

        private StateMachine machine;
        private int mask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact")); //Layer mask to ignore puppeteer interact colliders
        private float maxLostTime = 5;
        private float lostTime = 0;
        private float lastTime = 0;

        public AttackState(StateMachine machine)
        {
            this.machine = machine;
        }
        

        public override void Enter()
        {
            machine.CurrentStateName = "Attack";
            machine.AnimController.SetBool("Running", true);
        }

        public override void Run()
        {

            //If no target, go idle
            if(machine.TargetEntity == null) machine.SetState(new WanderState(machine));

            Ray attackRay = new Ray(machine.transform.position + new Vector3(0,.5f,0), machine.transform.forward);
            if (machine.eDebug) Debug.DrawRay(machine.transform.position, Vector3.forward * machine.AttackRange, Color.green, 0.2f);

            //Tests if player is in front
            if (Physics.Raycast(attackRay, out RaycastHit target, machine.AttackRange, mask))
            {
                if (target.collider.tag == ("Player"))
                {
                    //If canAttack, perform attack. Otherwise stop moving
                    if(machine.CanAttack)
                    {
                        machine.StartCoroutine("attackTimer");
                        machine.AnimController.SetInteger("RandomAnimationIndex", Random.Range(0,6));
                        machine.AnimController.SetTrigger("Attack");
                    }
                    else
                    {
                        machine.PathFinder.Stop();
                    }
                }
            }
            else
            {
                //Moves towards player
                machine.PathFinder.MoveTo(machine.TargetEntity.transform.position);
            }
            

            //Tests if player is visible
            RaycastHit hit;
            if (!Physics.Raycast(machine.transform.position + new Vector3(0,.5f,0), machine.TargetEntity.transform.position - machine.transform.position + new Vector3(0,.5f,0), out hit, machine.ConeAggroRange, mask) || hit.transform.tag != ("Player"))
            {
                //Counts seconds since player was lost, goes idle if past threshold 
                lostTime += (Time.time - lastTime);
                if(lostTime > maxLostTime)
                {
                    machine.SetState(new WanderState(machine));
                    machine.TargetEntity = null;
                }
            }
            else
            {
                lostTime = 0;
            }
            lastTime = Time.time;
        }
    
        public override void Exit()
        {
            machine.AnimController.SetBool("Running", false);
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
            //Setup
            machine.CurrentStateName = "ReturnToSpawn";
            machine.AnimController.SetBool("Running", true);

            //Fetches navmesh from spawner room and walks to a (semi)random point on it
            NavMesh navmesh = machine.EnemySpawner.transform.GetComponentInParent<NavMesh>();
            Vector3 destination;
            if(navmesh!=null)
            {
                //Fetches random face from navmesh as destination
                destination = machine.EnemySpawner.transform.parent.TransformPoint(navmesh.faces[Random.Range(0, navmesh.faces.Length - 1)].Origin);
                machine.PathFinder.MoveTo(destination);
            }
            else
            {
                //If navmesh isn't found, goes idle
                Debug.LogError("Entity " + machine.transform.name + " could not find navmesh in spawnRoom!");
                machine.AnimController.SetBool("Running", false);
                machine.SetState(new WanderState(machine));
            }
        }

        public override void Run()
        {
            machine.CheckProxy();

            //If no path, go idle
            if(!machine.PathFinder.HasPath)
            {
                machine.SetState(new WanderState(machine));
            }
        }

        public override void Exit()
        {
            machine.AnimController.SetBool("Running", false);
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
            //Setup
            machine.CurrentStateName = "Wander";

            //Fetches random destination close to spawner
            destination = machine.EnemySpawner.GetNearbyDestination();
            machine.PathFinder.MoveTo(destination);
            
        }

        public override void Run()
        {
            machine.CheckProxy();

            //If no path, restart
            if(!machine.PathFinder.HasPath)
            {
                machine.SetState(new WanderState(machine));
            }
        }

        public override void Exit()
        {
        }
    }

    //-------------------------------------------------------------

    public class SeekState : State
    {
        private StateMachine machine;
        private Vector3 destination;

        public SeekState(StateMachine machine, Vector3 destination)
        {
            this.machine = machine;
            this.destination = destination;
        }
        public override void Enter()
        {
            machine.CurrentStateName = "Seek";
            machine.AnimController.SetBool("Running", true);
            machine.PathFinder.MoveTo(destination);
        }

        public override void Run()
        {
            machine.CheckProxy();
            if(!machine.PathFinder.HasPath)
            {
                machine.SetState(new WanderState(machine));
            }
        }

        public override void Exit()
        {
            machine.AnimController.SetBool("Running", false);
        }
    }
}
