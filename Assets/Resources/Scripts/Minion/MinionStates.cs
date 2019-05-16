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
 * Charge attack state and corutine (in StateMachine) for the tanks special moves,
 * made by Carl Appelkvist
 *
 * CODE REVIEWED BY:
 * Ludvig Björk Förare (Charge attack 190514)
 * 
 */

namespace MinionStates
{
    public class AttackState : State
    {

        private StateMachine machine;
        
        //Timekeeping (TODO move somewhere more accessible)
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
            if(machine.TargetEntity == null){
                machine.SetState(new WanderState(machine));
                return;

            }

            //Debug ray for attack range
            if (machine.debug) Debug.DrawRay(machine.transform.position, Vector3.forward * machine.AttackRange, Color.green, 0.2f);

            //Tests if player is in front
            if (Vector3.Distance(machine.transform.position, machine.TargetEntity.transform.position) < machine.AttackRange)
            {
                if(machine.TargetEntity.Downed)
                {
                    machine.SetState(new WanderState(machine));
                    machine.TargetEntity = null;
                    return;
                }
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
            

            //Tests if player is concealed
            RaycastHit hit;
            if (!Physics.Raycast(machine.transform.position + new Vector3(0,.5f,0), machine.TargetEntity.transform.position - machine.transform.position + new Vector3(0,.5f,0), out hit, machine.ConeAggroRange, layerMask) || hit.transform.tag != ("Player"))
            {
                //Counts seconds since player was lost, goes idle if past threshold 
                playerLostTime += (Time.time - lastSeenTime);
                if(playerLostTime > machine.AggroDropTime)
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
        private NavMesh navmesh;

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
            navmesh = machine.Spawner.transform.GetComponentInParent<NavMesh>();
            Vector3 destination;
            if(navmesh!=null)
            {
                //Fetches random face from navmesh as destination
                destination = machine.Spawner.transform.parent.TransformPoint(navmesh.Faces[Random.Range(0, navmesh.Faces.Length - 1)].Origin);
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
                if(Physics.Raycast(machine.transform.position + machine.RaycastOffset, Vector3.down, out RaycastHit hit, 1, layerMask) 
                && hit.transform.GetComponentInParent<NavMesh>() == navmesh)
                {
                    machine.SetState(new IdleState(machine));
                }
                else
                {
                    machine.SetState(new ReturnToSpawnerState(machine));
                }
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
            destination = machine.GetNearbyDestination();

            //If no destination found, go idle
            if(destination == Vector3.positiveInfinity)
            {
                machine.SetState(new IdleState(machine));
                return;
            }
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
            this.waitTime = Random.Range(machine.MinIdleTime, machine.MaxIdleTime);
        }

        public IdleState(StateMachine machine, float waitTime)
        {
            this.machine = machine;
            this.waitTime = waitTime;
        }

        public override void Enter()
        {
            //Setup
            machine.CurrentStateName = "Idle";
            machine.PathFinder.Stop();
            machine.AnimController.SetBool("Running", false);
            this.lastTime = Time.time;
        }

        public override void Run()
        {
            if (machine.MinionType == EnemyType.Tank)
            {
                if (machine.RoomContainsPlayer() == null)
                {
                    if (machine.debug) Debug.Log("No players in room");
                    return;
                }
                else if (machine.TargetEntity != null)
                {
                    if (machine.debug) Debug.Log("Already have a target");
                    return;
                }
                else
                {
                    if (machine.debug) Debug.Log("Player found and targeted");
                    machine.TargetEntity = machine.RoomContainsPlayer();
                    machine.SetState(new BigAttackState(machine));
                }
            }

            //Checks for nearby players if regular Minion
            if (machine.MinionType == EnemyType.Minion) machine.CheckProximity();

            //Counts seconds standing idle
            currentWaitTime += (Time.time - lastTime);

            //If idletime has expended, wander around. As for Tanks, they stay idle
            if(currentWaitTime > waitTime && machine.MinionType == EnemyType.Minion)
            {
                machine.SetState(new WanderState(machine));
            }
            
            this.lastTime = Time.time;
        }

        public override void Exit()
        {

        }
    }
    //---------------------------------------------------------------------------------
    //TANK STATES
    //the idle state above is shared between tanks and minions
    //---------------------------------------------------------------------------------
    public class BigAttackState : State
    {
        private StateMachine machine;
        
        public BigAttackState(StateMachine machine)
        {
            this.machine = machine;
        }

        public override void Enter()
        {
            machine.CurrentStateName = "BigAttack";
            machine.AnimController.SetBool("Running", true);
            machine.ChargeCharge = 0;
        }

        public override void Run()
        {
            float dist = Vector3.Distance(machine.transform.position, machine.TargetEntity.transform.position);

            machine.ChargeCharge++;

            //If the target has no healt remove it from TargetEntity
            if (machine.TargetEntity.Health <= 0) machine.TargetEntity = null;

            //If no target, go idle
            if (machine.TargetEntity == null) machine.SetState(new IdleState(machine));

            //Debug ray for attack range
            if (machine.debug) Debug.DrawRay(machine.transform.position, Vector3.forward * machine.AttackRange, Color.green, 0.2f);

            //Look if the target is within the charge range
            if (dist <= 30f && dist >= 10f && machine.ChargeCharge > 100)
            {
                machine.SetState(new ChargeAttackState(machine));
            }

            //Tank has a wide cone for regular attack but it only hits one player
            if (machine.WithinCone(machine.transform, machine.TargetEntity.transform, 90f, 2f, 0f))
            {
                if (machine.CanAttack)
                {
                    //TODO: fix for the tank animations and model
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
            machine.ChargeCharge = 0;
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
            machine.AnimController.SetBool("IsCharging", true);
            machine.ChargeStopped = false;
            machine.PathFinder.RotationSpeed = 2f;
            machine.PathFinder.NodeArrivalMargin = 0.5f;
        }

        public override void Run()
        {
            //If the charge stops go through all relevant variables and set them to default
            if (machine.ChargeStopped)
            {
                machine.AnimController.SetFloat("ChargeSpeed", 0);
                machine.CurrentChargeSpeed = 0;
                machine.StopCoroutine("chargeRoutine");
                machine.ChargeStopped = false;
                machine.SetState(new BigAttackState(machine));
            }
            //If within a narrow long cone start the Charge Routine
            if (machine.WithinCone(machine.transform, machine.TargetEntity.transform, 30f, 15f, 0f))
            {
                if (machine.Corunning)
                {
                    return;
                }
                else
                {
                    machine.Corunning = true;
                    machine.StartCoroutine("chargeRoutine");
                }
            }
            machine.PathFinder.MoveTo(machine.TargetEntity.transform.position);
        }

        public override void Exit()
        {
            //TODO: fix so it works for tank model and size
            machine.PathFinder.RotationSpeed = 20;
            machine.PathFinder.NodeArrivalMargin = 0.5f;
            machine.AnimController.SetBool("IsCharging", false);
        }
    }
}
