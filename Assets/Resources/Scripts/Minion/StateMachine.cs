using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MinionStates;
using Mirror;

#pragma warning disable IDE1006 // Naming Styles

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * Carl Appelkvist
 * 
 * DESCRIPTION:
 * A finite state machine used to drive minion AI.
 * Requires separate class for states 
 *
 * CODE REVIEWED BY:
 * 
 */

public enum EnemyType
{
    Minion,
    Tank
};


public class StateMachine : NetworkBehaviour
{

    [Header("General settings")]
    public uint tickRate = 10;
    //States
    public string CurrentStateName;
    public State CurrentState;
    public EnemyType MinionType;

    //References
    [HideInInspector]
    public EnemySpawner Spawner;
    [HideInInspector]
    public GameObject TargetEntity;
    [HideInInspector]
    public Animator AnimController;
    [HideInInspector]
    public PathfinderComponent PathFinder;
    [HideInInspector]
    public List<GameObject> Puppets;

    //ta bort senare
    public bool Corunning;


    [Header("Normal attack settings")]
    public float AttackCooldown;
    public uint AttackDamage;
    public float AttackRange;
    [HideInInspector]
    public bool CanAttack;
    [Header("Charge attack settings")]
    public bool ChargeStopped;
    public float ChargeAccelerationSpeed;
    public float CurrentChargeSpeed;
    public float StartChargeSpeed;
    public float MaxChargeSpeed;
    public int ChargeCharge;


    [Header("Aggro settings")]
    public float AggroDropTime;
    public float InstantAggroRange;
    public float ConeAggroRange;
    public float FOVConeAngle;
    public bool AtkPrioRunning;
    public bool AssholeMode;
    private float PreThreat;
    private float PostThreat;

    [Header("Idle settings")]

    public float MinIdleTime;
    public float MaxIdleTime;

    [Header("Misc. settings")]
    public Vector3 RaycastOffset; //Safety offset so raycast doesn't hit ground instantly
    public bool debug;

    [Range(0,1)]
    public float ChooseCurrentRoomChance = .3f;

    private int layerMask;

    public void Start()
    {
        layerMask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact"));
        AnimController = GetComponent<Animator>();

        //TODO: remove when prefab gets changed from Acceleration 0
        

        GetComponent<HealthComponent>().AddDeathAction(Die);
        //If not server, disable self
        if(!isServer)
        {
            this.enabled = false;
            AnimController.applyRootMotion = false;
            return;
        }

        PathFinder = GetComponent<PathfinderComponent>();
        //If regular Minion or Tank
        if (MinionType == EnemyType.Minion)
        {
            SetState(new WanderState(this));
        }
        else
        {
            SetState(new IdleState(this));
        }

        CanAttack = true;
        PostThreat = Mathf.NegativeInfinity;
    }

    public void SetState(State newState)
    {
        if (CurrentState != null) CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        if (System.Environment.TickCount % tickRate == 0) //Runs current state every 'tickRate' ticks
        {
            //TODO make it work with invisible puppet, for now the tag changes from player when it becomes invisible and reverts after
            Puppets.Clear(); //TODO move this to Start(). Currently in update for dev purposes
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in players)
            {
                try
                {
                    if(player.GetComponent<HealthComponent>().Health > 0)
                    {
                        Puppets.Add(player);
                    }
                }
                catch(System.Exception e)
                {
                    
                }
                
            }
            //CurrentChargeSpeed = AnimController.GetFloat("ChargeSpeed");
            if (CurrentState != null) CurrentState.Run();
        }
    }

    //Method to check if players are within proximity
    public void CheckProximity()
    {
        int mask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact"));
        foreach (GameObject puppet in Puppets)
        {   
            //Finds closest alive puppet
            if (puppet != null && puppet.GetComponent<HealthComponent>().Health > 0)
            {
                //If within cone range and not obscured
                if (WithinCone(transform, puppet.transform, FOVConeAngle, ConeAggroRange, InstantAggroRange))
                {     //Attack player
                    TargetEntity = puppet.gameObject;
                    if (MinionType == EnemyType.Minion)
                    {
                        SetState(new AttackState(this));
                    }
                    else
                    {
                        SetState(new BigAttackState(this));
                    }
                        
                    
                }
            }
            else if (puppet == null)
            {
                Puppets.Remove(puppet);
            }
        }
    }

    //Runs when health = 0
    public void Die()
    {
        this.GetComponent<Collider>().enabled = false;

        if(isServer){
            this.enabled = false;
            PathFinder.Stop();
            PathFinder.enabled = false;
        }

        AnimController.SetTrigger("Death");
        AnimController.SetInteger("RandomAnimationIndex", Random.Range(0,3));
    }

    //Runs when death animation is complete, despawns object
    public void Despawn()
    {
        EnemySpawner.AllMinions.Remove(this);
        if(Spawner !=null) Spawner.LocalMinions.Remove(this);
        Destroy(this.gameObject);
    }

    //Runs during attack animation, deals damage to player
    public void Attack()
    {
        if(!isServer) return;
        HealthComponent health = TargetEntity.GetComponent<HealthComponent>();
        if(health == null)
        {
            TargetEntity = null;
            if (MinionType == EnemyType.Minion)
            {
                SetState(new WanderState(this));
            }
            else
            {
                SetState(new IdleState(this));
            }
            return;
        }
        if (health.Health > 0)
        {
            health.Damage(AttackDamage);
        }
    }

    //Starts attack cooldown
    private IEnumerator attackTimer()
    {
        CanAttack = false;
        yield return new WaitForSeconds(AttackCooldown);
        CanAttack = true;
    }

    public bool WithinCone(Transform source, Transform target, float coneAngle, float coneLength, float coneIgnoreRadius)
    {
        int mask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact"));
        float distance = Vector3.Distance(source.position, target.position);
        if (distance <= coneLength && Physics.Raycast(source.position + RaycastOffset, target.position - source.position, out RaycastHit hit, coneLength, mask))
        {
            if (hit.transform.tag.Equals("Player"))
            {
                //If inside instant-aggro range or within vision cone
                if (distance < coneIgnoreRadius || Vector3.Angle(source.forward, target.position - source.position) <= coneAngle)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator chargeRoutine()
    {
        while (true)
        {
            if (!PathFinder.HasPath)
            {
                PathFinder.RotationSpeed = 2f;
                PathFinder.NodeArrivalMargin = 0.5f;
            }

            Corunning = true;

            if (AnimController.GetBool("IsCharging") == true && AnimController.GetFloat("ChargeSpeed") < 1)
            {
                CurrentChargeSpeed = CurrentChargeSpeed += ChargeAccelerationSpeed;
                AnimController.SetFloat("ChargeSpeed", CurrentChargeSpeed);
            }

            //foreach (GameObject pupp in Puppets)
            //{
            //    HealthComponent health = pupp.GetComponent<HealthComponent>();
            //    if (WithinCone(transform, pupp.transform, 80f, 2f, 0f))
            //    {
            //        float chargeDamage = CurrentChargeSpeed * 5;
            //        uint uChargeDamage = (uint)chargeDamage;
            //        Debug.Log("Damage dealt: " + chargeDamage + " Damage in uint: " + uChargeDamage);
            //        health.Damage(uChargeDamage);
            //        ChargeStopped = true;
            //        Corunning = false;
            //        yield break;
            //    }
            //    else
            //    {
            //        yield return new WaitForSeconds(0.1f);
            //    }
            //}
            //GameObject.FindGameObjectsWithTag("Player");

            if (WithinCone(transform, TargetEntity.transform, 80f, 2f, 0f))
            {
                HealthComponent health = TargetEntity.GetComponent<HealthComponent>();
                float chargeDamage = CurrentChargeSpeed * 5;
                uint uChargeDamage = (uint)chargeDamage;

                if (debug) Debug.Log("Damage dealt: " + chargeDamage + " Damage in uint: " + uChargeDamage);

                health.Damage(uChargeDamage);
                ChargeStopped = true;
                Corunning = false;
                yield break;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }

        }
    }


    //Isn't used at the time this file is reviewed

    private IEnumerator attackPriority()
    {
        AtkPrioRunning = true;

        foreach (GameObject pupp in Puppets)
        {
            float puppDist = Vector3.Distance(pupp.transform.position, gameObject.transform.position);
            float puppH = pupp.GetComponent<HealthComponent>().Health;
            float puppA = pupp.GetComponent<PlayerController>().Ammunition;
            float puppR = pupp.GetComponent<ReviveComponent>().DeathDelay;

            if (puppA == 0f) puppA = 150f;
            if (pupp.GetComponent<HealthComponent>().Health == 0 || puppDist > ConeAggroRange * 1.5f)
            {
                AtkPrioRunning = false;
                yield break;
            }

            if (AssholeMode)
            {
                if (pupp.GetComponent<PlayerController>().HasMedkit)
                {
                    PreThreat = ((-puppA / -puppH) * 3 - puppR) / puppDist;
                }
                else
                {
                    PreThreat = ((-puppA / -puppH) - puppR) / puppDist;
                }

            }
            else
            {
                if (pupp.GetComponent<PlayerController>().HasMedkit)
                {
                    PreThreat = (150 - puppH) / puppDist;
                }
                else
                {
                    PreThreat = (100 - puppH) / puppDist;
                }
            }

            if (PostThreat > PreThreat)
            {
                PostThreat = PreThreat;
            }
        }
        yield return new WaitForSeconds(0.1f);
    }

    public Vector3 GetNearbyDestination(){
        
        AnchorPoint currentDoor = null;
        DoorReferences doorReferences = null;
        
        if(Spawner != null)
        {
            doorReferences = Spawner.GetComponentInParent<DoorReferences>();
        }
        else if(Physics.Raycast(transform.position + RaycastOffset,  Vector3.down, out RaycastHit hit, 1, layerMask))
        {
            doorReferences = hit.transform.GetComponentInParent<DoorReferences>();
        }


        while(doorReferences != null){
            //Checks if this room is to be chosen
            if(Random.Range(0.0f,1.0f) <= ChooseCurrentRoomChance){
                break;
            }

            List<AnchorPoint> availableDoors = new List<AnchorPoint>();
            if(doorReferences == null) break;
            foreach(AnchorPoint door in doorReferences.doors){
                if(door.Connected && door != currentDoor && door.ConnectedTo != null){ //TODO remove nullprodection
                    availableDoors.Add(door);
                }
            }
            
            if(availableDoors.Count == 0){
                break;
            }

            currentDoor = availableDoors[Random.Range(0,availableDoors.Count-1)].ConnectedTo;
            doorReferences = currentDoor.GetComponentInParent<DoorReferences>();
            if(doorReferences == null) break;
        }

        NavMesh navMesh = null;

        try
        {
            navMesh = doorReferences.GetComponent<NavMesh>();
        }
        catch(System.NullReferenceException e)
        {
            Debug.LogWarning("EnemySpawner tried to send minion to a room which had no navmesh: " + doorReferences.name);
            return GetNearbyDestination(); //Attempts again
        }

        return doorReferences.transform.TransformPoint(navMesh.Faces[Random.Range(0,navMesh.Faces.Length-1)].Origin);

    }

}



//-------------------------------------------------------------
public abstract class State
{
    public abstract void Enter();

    public abstract void Run();
    
    public abstract void Exit();
}
