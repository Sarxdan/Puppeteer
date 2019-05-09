using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using MinionStates;
/*
 * AUTHOR:
 * Carl Appelkvist
 * 
 * DESCRIPTION:
 * A manager that spawns npc's from GameObjects in the scene with a variable spawn speed
 * Any prefab can be spawned and any amount of prefabs can be spawned
 * 
 * CODE REVIEWED BY:
 * Ludvig Björk Förare
 * 
 * CONTRIBUTORS:
 * Ludvig Björk Förare (Integration to game)
 */

public class EnemySpawner : NetworkBehaviour
{
    public GameObject EnemyPrefab;
    public int MaxEnemyCount;
    public int MinDelay = 5;
    public int MaxDelay = 10;
    public List<StateMachine> LocalMinions = new List<StateMachine>();
    public static List<StateMachine> AllMinions = new List<StateMachine>();
    public static Transform MinionContainerObject;

    private SnapFunctionality snapFunctionality;

    public Transform spawnPoint;

    public float ChooseThisChance = .3f;


    public void Start()
    {
        if(MinionContainerObject == null){
            MinionContainerObject = GameObject.Find("MinionContainer").transform;
        }
        snapFunctionality = GetComponent<SnapFunctionality>();
        StartCoroutine("Spawn");
        HealthComponent hpComponent = GetComponent<HealthComponent>();
        hpComponent.AddDeathAction(OnDeath);
        hpComponent.AddOnDamageAction(CmdOnTakeDamage);

    }


    //Spawn is a modified Update with a set amount of time (SpawnRate) between runs
    private IEnumerator Spawn()
    { 
        while(true){
            if(isServer && snapFunctionality.Placed){
                //Check if max amount of enemies has been reached
                if (LocalMinions.Count < MaxEnemyCount && MaxEnemyCount > 0)
                {
                    //If not then create a GameObject from attached prefab at the spawners position and make them children of the "folder" created earlier
                    GameObject npcEnemy = Instantiate(EnemyPrefab, spawnPoint.position, transform.rotation, MinionContainerObject) as GameObject;
                    StateMachine machine = npcEnemy.GetComponent<StateMachine>();
                    machine.Spawner = this;
                    NetworkServer.Spawn(npcEnemy);

                    //Adds 
                    AllMinions.Add(machine);
                    LocalMinions.Add(machine);
                }
            }
            yield return new WaitForSeconds(Random.Range(MinDelay, MaxDelay));
        }
    }

    //Returns a random point from a room somewhat close to the spawners room
    public Vector3 GetNearbyDestination(){
        
        AnchorPoint currentDoor = null;
        DoorReferences doorReferences = transform.GetComponentInParent<DoorReferences>();
        while(doorReferences != null){
            //Checks if this room is to be chosen
            if(Random.Range(0.0f,1.0f) <= ChooseThisChance){
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

    [Command]
    public void CmdOnTakeDamage(){
        foreach(StateMachine enemy in LocalMinions){
            enemy.SetState(new ReturnToSpawnerState(enemy));
        }
    }

    public void OnDeath(){
        foreach(StateMachine enemy in LocalMinions){
            enemy.Die();
        }
        Destroy(gameObject);
    }
}