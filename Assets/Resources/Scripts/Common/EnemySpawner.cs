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

    public float ChooseThisChance = .3f;


    public void Start()
    {
        gameObject.tag = "EnemySpawner";

        if(isServer){
            StartCoroutine("Spawn");
        }

    }

    public void Update(){
        if(Input.GetKey(KeyCode.P)){
            Debug.DrawRay(GetNearbyDestination(), Vector3.up * 5, Color.cyan, 2);
        }
    }

    //Spawn is a modified Update with a set amount of time (SpawnRate) between runs
    private IEnumerator Spawn()
    { 
        while(true){
            //Check if max amount of enemies has been reached
            if (SpawnedEnemies.Count < MaxEnemyCount && MaxEnemyCount > 0)
            {
                //If not then create a GameObject from attached prefab at the spawners position and make them children of the "folder" created earlier
                GameObject npcEnemy = Instantiate(EnemyPrefab, transform.position, transform.rotation, transform) as GameObject;
                NetworkServer.Spawn(npcEnemy);
                SpawnedEnemies.Add(npcEnemy);
                npcEnemy.GetComponent<StateMachine>().EnemySpawner = this;
            }

            yield return new WaitForSeconds(Random.Range(MinDelay, MaxDelay));
        }
    }

    public Vector3 GetNearbyDestination(){
        
        Transform currentRoom = transform.parent;
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