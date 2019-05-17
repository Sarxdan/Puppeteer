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
 * Charge attack state (in MinionStates) and corutine for the tanks special moves,
 * made by Carl Appelkvist
 *
 * CODE REVIEWED BY:
 * Ludvig Björk Förare (Charge attack 190514)
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
    public HealthComponent TargetEntity;
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

    public float AttackEscapeDistance;
    [HideInInspector]
    public bool CanAttack;
    [Header("Charge attack settings")]
    public bool ChargeStopped;
    public float ChargeAccelerationSpeed;
    public float CurrentChargeSpeed;
    public float StartChargeSpeed;
    public int ChargeCharge;
    [HideInInspector]
    public Collider[] HitColliders;
    //[HideInInspector]
    //public Collider[] HitPillars;


    [Header("Aggro settings")]
    public float AggroDropTime;
    public float InstantAggroRange;
    public float ConeAggroRange;
    public float FOVConeAngle;
    [HideInInspector]
    public bool AtkPrioRunning;
    [HideInInspector]
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
        layerMask = ~(1 << (LayerMask.NameToLayer("Puppeteer Interact")) | (LayerMask.NameToLayer("Ignore Raycast")));
        AnimController = GetComponent<Animator>();
        PathFinder = GetComponent<PathfinderComponent>();

        //TODO: remove when prefab gets changed from Acceleration 0
        
        CanAttack = true;
        PostThreat = Mathf.NegativeInfinity;

        GetComponent<HealthComponent>().AddDeathAction(Die);
        //If not server, disable self
        if(!isServer)
        {
            this.enabled = false;
            AnimController.applyRootMotion = false;
            return;
        }

        if(FinalRoomInteract.isEndGame)
        {
            SetState(new ReturnToSpawnerState(this));
        }
        else
        {
            //If regular Minion or Tank
            if (MinionType == EnemyType.Minion)
            {
                SetState(new IdleState(this, 1));
            }
            else
            {
                SetState(new IdleState(this));
            }
        }
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
                    if(!player.GetComponent<HealthComponent>().Downed)
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

    public void LateUpdate()
    {
        if(!CanAttack && TargetEntity != null && !PathFinder.HasPath)
        {
            transform.LookAt(RemoveY(TargetEntity.transform.position));
        }
    }

    //Method to check if players are within proximity
    public void CheckProximity()
    {
        //Finds closest puppet
        foreach (GameObject puppet in Puppets)
        {   
            if (puppet != null)
            {
                //If within cone range and not obscured
                if (WithinCone(transform, puppet.transform, FOVConeAngle, ConeAggroRange, InstantAggroRange))
                {     //Attack player
                    TargetEntity = puppet.GetComponent<HealthComponent>();
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

        if(isServer)
        {
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
        if(isServer);
        EnemySpawner.AllMinions.Remove(this);
        if(Spawner !=null) Spawner.LocalMinions.Remove(this);
        Destroy(this.gameObject);
    }

    //Runs during attack animation, deals damage to player
    public void Attack()
    {
        if(!isServer) return;
        
        if(TargetEntity == null)
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
        
        if (!TargetEntity.Downed && Vector3.Distance(transform.position, RemoveY(TargetEntity.transform.position)) < AttackEscapeDistance)
        {
            TargetEntity.Damage(AttackDamage);
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
        float distance = Vector3.Distance(source.position, target.position);
        if (distance <= coneLength && Physics.Raycast(source.position + RaycastOffset, target.position - source.position, out RaycastHit hit, coneLength, layerMask))
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

    //Removes the y axis of input vector
    public static Vector3 RemoveY(Vector3 input)
    {
        Vector3 ret = input;
        ret.y = 0;
        return ret;
    }

    private IEnumerator chargeRoutine()
    {
        while (Corunning)
        {
            Corunning = true;

            //Accelerate if charging up to a speed limit
            if (AnimController.GetBool("IsCharging") == true && AnimController.GetFloat("ChargeSpeed") < 1)
            {
                CurrentChargeSpeed = CurrentChargeSpeed += ChargeAccelerationSpeed;
                AnimController.SetFloat("ChargeSpeed", CurrentChargeSpeed);
            }

            //HitPillars = Physics.OverlapSphere(gameObject.transform.position, 1f);

            //foreach (Collider pillar in HitPillars)
            //{
            //    if (pillar.name == "ST_Pillar_Full_01")
            //    {
            //        pillar.gameObject.SetActive(false);
            //    }
            //}

            //Hit cone, triggered when the target is within the parameters
            if (WithinCone(transform, TargetEntity.transform, 80f, 2f, 0f))
            {
                //Checks for players in range and deals damage to them aswell
                HitColliders = Physics.OverlapSphere(gameObject.transform.position, 2f);

                foreach(Collider coll in HitColliders)
                {
                    if (coll.tag == "Player")
                    {
                        //Deals damage to the players in range based on charge speed
                        float chargeDamage = CurrentChargeSpeed * 5;
                        uint uChargeDamage = (uint)chargeDamage;
                        if (debug) Debug.Log("Damage dealt: " + chargeDamage + " Damage in uint: " + uChargeDamage + " Target hit = " + coll);
                        TargetEntity.Damage(uChargeDamage);
                    }
                    //else if (coll.name == "ST_Pillar_Full_01")
                    //{
                    //    coll.gameObject.SetActive(false);
                    //}
                }
                //Set variables back to default on routine exit
                //HitPillars = null;
                HitColliders = null;
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

    public Vector3 GetNearbyDestination()
    {
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
        else
        {
            Debug.LogWarning("Minion error: couldn't find nearby destination, going idle");
            return Vector3.positiveInfinity;
        }


        while(doorReferences != null)
        {
            //Checks if this room is to be chosen
            if(Random.Range(0.0f,1.0f) <= ChooseCurrentRoomChance)
            {
                break;
            }

            List<AnchorPoint> availableDoors = new List<AnchorPoint>();
            if(doorReferences == null) break;
            foreach(AnchorPoint door in doorReferences.doors)
            {
                if(door.Connected && door != currentDoor && door.ConnectedTo != null)
                { //TODO remove nullprodection
                    availableDoors.Add(door);
                }
            }
            
            if(availableDoors.Count == 0)
            {
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
            Debug.LogWarning("EnemySpawner tried to send minion to a room which had no navmesh: " + currentDoor.GetComponentInParent<NavMesh>().transform.name);
            return GetNearbyDestination(); //Attempts again
        }
        return doorReferences.transform.TransformPoint(navMesh.Faces[Random.Range(0,navMesh.Faces.Length-1)].Origin);
    }

    public HealthComponent RoomContainsPlayer()
    {
        Physics.Raycast(transform.position + RaycastOffset, Vector3.down, out RaycastHit hit, 3, layerMask);
        foreach (BoxCollider collider in hit.transform.GetComponentInParent<NavMesh>().GetComponents<BoxCollider>())
        {
            foreach (GameObject player in Puppets)
            {
                //Check if any player is within any collider on the room and if they are alive
                if (collider.bounds.Contains(player.transform.position) && player.GetComponent<HealthComponent>().Health > 0)
                {
                    return player.GetComponent<HealthComponent>();
                }
            }
        }
        return null;
    }
}



//-------------------------------------------------------------
public abstract class State
{
    
    protected int layerMask = ~(1 << (LayerMask.NameToLayer("Puppeteer Interact")) | (LayerMask.NameToLayer("Ignore Raycast")));
    public abstract void Enter();

    public abstract void Run();
    
    public abstract void Exit();
}
