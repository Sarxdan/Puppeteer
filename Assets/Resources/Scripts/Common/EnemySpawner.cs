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

    private EnemySpawner finalRoomDummy;


    public void Start()
    {   
        finalRoomDummy = GameObject.Find("DummySpawner").GetComponent<EnemySpawner>();
        if(finalRoomDummy == gameObject) return;
        
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
                    
                    machine.Spawner = FinalRoomInteract.isEndGame ? finalRoomDummy : this;
                    
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
    

    [Command]
    public void CmdOnTakeDamage(){
        foreach(StateMachine minion in LocalMinions){
            if(minion.TargetEntity == null)
                minion.SetState(new ReturnToSpawnerState(minion));
        }
    }

    public void OnDeath(){
        foreach(StateMachine minion in LocalMinions){
            minion.Spawner = null;
        }
        Destroy(gameObject);
    }
}