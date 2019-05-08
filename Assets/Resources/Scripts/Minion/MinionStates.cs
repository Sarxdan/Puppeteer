using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * Carl Appelkvist
 * 
 * DESCRIPTION:
 * A class containing minion states. Used by StateMachine
 *
 * CODE REVIEWED BY:
 * 
 */

namespace MinionStates
{
    public class AttackState : State
    {

        private StateMachine machine;
        private int mask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact")); //Layer mask to ignore puppeteer interact colliders

        //Timekeeping (TODO move somewhere more accessible)
        private float maxLostTime = 5;
        private float playerLostTime = 0;
        private float lastSeenTime = 0;

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

            //Debug ray for attack range
            if (machine.debug) Debug.DrawRay(machine.transform.position, Vector3.forward * machine.AttackRange, Color.green, 0.2f);

            //Tests if player is in front
            if (Physics.Raycast(machine.transform.position + new Vector3(0,.5f,0), machine.transform.forward, out RaycastHit target, machine.AttackRange, mask))
            {
                if (target.transform == machine.TargetEntity.transform)
                {
                    //If canAttack, perform attack. Otherwise stop moving (so minions don't push around the player)
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
                else
                {
                    //Moves towards player
                    machine.PathFinder.MoveTo(machine.TargetEntity.transform.position);
                }
            }
            else
            {
                //Moves towards player
                machine.PathFinder.MoveTo(machine.TargetEntity.transform.position);
            }
            

            //Tests if player is concealed
            RaycastHit hit;
            if (!Physics.Raycast(machine.transform.position + new Vector3(0,.5f,0), machine.TargetEntity.transform.position - machine.transform.position + new Vector3(0,.5f,0), out hit, machine.ConeAggroRange, mask) || hit.transform.tag != ("Player"))
            {
                //Counts seconds since player was lost, goes idle if past threshold 
                playerLostTime += (Time.time - lastSeenTime);
                if(playerLostTime > maxLostTime)
                {
                    machine.SetState(new IdleState(machine));
                    machine.TargetEntity = null;
                }
            }
            else
            {
                playerLostTime = 0;
            }
            lastSeenTime = Time.time;
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
                machine.SetState(new IdleState(machine));
            }
        }

        public override void Run()
        {
            machine.CheckProximity();

            //If no path, go idle
            if(!machine.PathFinder.HasPath)
            {
                machine.SetState(new IdleState(machine));
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
            machine.CheckProximity();

            //If no path, go idle
            if(!machine.PathFinder.HasPath)
            {
                machine.SetState(new IdleState(machine));
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
            //Setup
            machine.CurrentStateName = "Seek";
            machine.AnimController.SetBool("Running", true);
            machine.PathFinder.MoveTo(destination);
        }

        public override void Run()
        {
            //Checks for nearby players
            machine.CheckProximity();

            //If done moving, wander randomly
            if(!machine.PathFinder.HasPath)
                machine.SetState(new IdleState(machine));
        }

        public override void Exit()
        {
            machine.AnimController.SetBool("Running", false);
        }
    }

//---------------------------------------------------------------------------------
    public class IdleState : State
    {
        private StateMachine machine;

        private float waitTime;
        private float currentWaitTime;
        private float lastTime;

        public IdleState(StateMachine machine)
        {
            this.machine = machine;
            this.waitTime = Random.Range(0,7.0f);
        }

        public override void Enter()
        {
            //Setup
            machine.CurrentStateName = "Idle";
            machine.PathFinder.Stop();
            machine.AnimController.SetBool("Running", false);
        }

        public override void Run()
        {
            //Checks for nearby players
            machine.CheckProximity();

            //Counts seconds standing idle
            currentWaitTime += (Time.time - lastTime);

            //If idletime has expended, wander around. As for Tanks, they stay idle
            if(currentWaitTime > waitTime && machine.MinionType == EnemyType.Minion)
            {
                machine.SetState(new WanderState(machine));
            }
        }

        public override void Exit()
        {

        }
    }
    //---------------------------------------------------------------------------------
    //BIG BOY STATES
    //WORK IN PROGRESS
    //---------------------------------------------------------------------------------
    public class BigAttackState : State
    {
        private StateMachine machine;
        private int mask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact")); //Layer mask to ignore puppeteer interact colliders

        public BigAttackState(StateMachine machine)
        {
            this.machine = machine;
        }

        public override void Enter()
        {
            machine.CurrentStateName = "BigAttack";
            machine.AnimController.SetBool("Running", true);
        }

        public override void Run()
        {
            //If no target, go idle
            if (machine.TargetEntity == null) machine.SetState(new IdleState(machine));

            //Debug ray for attack range
            if (machine.debug) Debug.DrawRay(machine.transform.position, Vector3.forward * machine.AttackRange, Color.green, 0.2f);


            if (machine.WithinCone(machine.transform, machine.TargetEntity.transform, 75f, 2f, 0f))
            {
                if (machine.CanAttack)
                {
                    machine.StartCoroutine("attackTimer");
                    //Insert animations & attack types
                    //Placeholders from regular minion code:
                    machine.AnimController.SetInteger("RandomAnimationIndex", Random.Range(0, 6));
                    machine.AnimController.SetTrigger("Attack");
                }
                else
                {
                    machine.PathFinder.Stop();
                }
            }
            else
            {
                machine.PathFinder.MoveTo(machine.TargetEntity.transform.position);
            }
        }

        public override void Exit()
        {
            machine.AnimController.SetBool("Running", false);
        }
    }

    public class ChargeAttackState : State
    {
        private StateMachine machine;

        public ChargeAttackState(StateMachine machine)
        {
            this.machine = machine;
        }

        public override void Enter()
        {
            machine.CurrentStateName = "ChargeAttack";
            machine.AnimController.SetBool("Running", true);
            machine.ChargeStopped = false;
        }

        public override void Run()
        {
            //float dist = Vector3.Distance(machine.TargetEntity.transform.position, machine.transform.position);
            if (machine.ChargeStopped)
            {
                machine.StopCoroutine("chargeRoutine");
            }
            if (machine.WithinCone(machine.transform, machine.TargetEntity.transform, 30f, 15f, 0f) && )
            {
                machine.StartCoroutine("chargeRoutine");
            }
            //machine.PathFinder.MoveTo(machine.TargetEntity.transform.position);
        }

        public override void Exit()
        {
            machine.AnimController.SetBool("Running", false);
        }
    }
}
