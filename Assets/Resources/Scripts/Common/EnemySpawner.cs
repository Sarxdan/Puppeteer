using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
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
    public List<GameObject> SpawnedEnemies = new List<GameObject>();

    private TrapBaseFunctionality trapBase;

    public float ChooseThisChance = .3f;


    public void Start()
    {
        trapBase = GetComponent<TrapBaseFunctionality>();
        if(isServer){
            StartCoroutine("Spawn");
        }

    }


    //Spawn is a modified Update with a set amount of time (SpawnRate) between runs
    private IEnumerator Spawn()
    { 
        while(true){
            if(trapBase.Placed){
                //Check if max amount of enemies has been reached
                if (SpawnedEnemies.Count < MaxEnemyCount && MaxEnemyCount > 0)
                {
                    //If not then create a GameObject from attached prefab at the spawners position and make them children of the "folder" created earlier
                    GameObject npcEnemy = Instantiate(EnemyPrefab, transform.position, transform.rotation, transform) as GameObject;
                    npcEnemy.GetComponent<StateMachine>().EnemySpawner = this;
                    NetworkServer.Spawn(npcEnemy);
                    SpawnedEnemies.Add(npcEnemy);
                    Noise.Minions.Add(npcEnemy.GetComponent<StateMachine>());
                }
            }
            yield return new WaitForSeconds(Random.Range(MinDelay, MaxDelay));
        }
    }

    //Returns a random point from a room somewhat close to the spawners room
    public Vector3 GetNearbyDestination(){
        
        Transform currentRoom = transform.parent.parent;
        AnchorPoint currentDoor = null;
        DoorReferences doorReferences = currentRoom.GetComponent<DoorReferences>();
        while(currentRoom != null){
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
            doorReferences = currentDoor.transform.parent.parent.GetComponent<DoorReferences>();
            if(doorReferences == null) break;
            currentRoom = currentDoor.transform.parent.parent;
        }

        NavMesh navMesh = currentRoom.GetComponent<NavMesh>();
        return currentRoom.TransformPoint(navMesh.faces[Random.Range(0,navMesh.faces.Length-1)].Origin);

    }
}