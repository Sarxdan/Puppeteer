using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinionStates
{
    public class AttackingState : State
    {

        private StateMachine machine;
        private int mask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact"));
        private float maxLostTime = 5;
        private float lostTime = 0;
        private float lastTime = 0;

        public AttackingState(StateMachine machine)
        {
            this.machine = machine;
        }
        

        public override void Enter()
        {
            Debug.Log("Attack state entered!");
            machine.AnimController.SetBool("Running", true);
        }

        public override void Run()
        {
            if(machine.TargetEntity == null) machine.SetState(new WanderState(machine));

            Ray attackRay = new Ray(machine.transform.position + new Vector3(0,.5f,0), machine.TargetEntity.transform.position - machine.transform.position + new Vector3(0,.5f,0));
            if (machine.eDebug == true) Debug.DrawRay(machine.transform.position, Vector3.forward * machine.AttackRange, Color.green, 0.2f);

            //machine.transform.position = Vector3.MoveTowards(machine.transform.position, machine.TargetEntity.transform.position, 0.3f);
            machine.PathFinder.MoveTo(machine.TargetEntity.transform.position);
            
            if (Physics.Raycast(attackRay, out RaycastHit target, machine.AttackRange, mask))
            {
                if (target.collider.tag == ("Player"))
                {
                    //machine.TargetEntity = target.transform.gameObject;
                    machine.AnimController.SetTrigger("Attack");
                }
            }

            //If player is lost
            RaycastHit hit;
            if (!Physics.Raycast(attackRay, out hit, machine.ConeAggroRange, mask) || hit.transform.tag != ("Player"))
            {
                Debug.Log("Lost: " + lostTime);
                lostTime += (Time.time - lastTime);
                if(lostTime > maxLostTime)
                {
                    machine.StopCoroutine("AttackRoutine");
                    machine.CoRunning = false;
                    machine.SetState(new SeekState(machine, machine.TargetEntity.transform.position));
                    machine.TargetEntity = null;
                    return;
                }
            }
            else
            {
                Debug.DrawRay(hit.point, Vector3.up, Color.magenta, 1);
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
            machine.StartCoroutine("ProxyRoutine");
            machine.AnimController.SetBool("Running", true);
            NavMesh navmesh = machine.transform.parent.GetComponent<NavMesh>();
            Vector3 destination;
            if(navmesh!=null){
                destination = navmesh.faces[Random.Range(0, navmesh.faces.Length - 1)].Origin;
                machine.PathFinder.MoveTo(destination);
            }else{
                machine.AnimController.SetBool("Running", false);
                machine.SetState(new WanderState(machine));
            }
        }

        public override void Run()
        {
            //if (machine.EnemySpawner.GetComponent<HealthComponent>().Health == 0 || machine.EnemySpawner == null);
            if(!machine.PathFinder.HasPath){
                machine.SetState(new WanderState(machine));
            }

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
            Debug.Log("Wander state entered!");
            destination = machine.EnemySpawner.GetNearbyDestination();
            machine.AnimController.SetBool("Walking", true);
            machine.PathFinder.MoveTo(destination);
        }

        public override void Run()
        {
            machine.StartCoroutine("ProxyRoutine");
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
        private Vector3 destination;

        public SeekState(StateMachine machine, Vector3 destination)
        {
            this.machine = machine;
            this.destination = destination;
        }
        public override void Enter()
        {
            Debug.Log("Seek state entered!");
            machine.AnimController.SetBool("Running", true);
            machine.PathFinder.MoveTo(destination);
        }

        public override void Run()
        {
            machine.StartCoroutine("ProxyRoutine");
            if(!machine.PathFinder.HasPath){
                machine.SetState(new WanderState(machine));
            }
        }

        public override void Exit()
        {
            machine.AnimController.SetBool("Running", false);
        }
    }
}
