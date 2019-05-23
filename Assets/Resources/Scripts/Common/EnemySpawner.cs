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
 * 
 * CLEANED
 */

public class EnemySpawner : NetworkBehaviour
{
    public GameObject EnemyPrefab;
    public int MaxEnemyCount;
	 
    public int MinDelay = 5;
    public int MaxDelay = 10;
    public bool TankSpawner;
    public Animator SpawnerAnim;
    public Animator MinionAnim;
    public List<StateMachine> LocalMinions = new List<StateMachine>();
    public static List<StateMachine> AllMinions = new List<StateMachine>();
    public static Transform MinionContainer;

    private SnapFunctionality snapFunctionality;

    public Transform spawnPoint;

    private EnemySpawner finalRoomDummy;

    public void Start()
    {   
        try
        {
            finalRoomDummy = GameObject.Find("DummySpawner").GetComponent<EnemySpawner>();
            if(finalRoomDummy == this) return;
            
            if(MinionContainer == null)
            {
                MinionContainer = GameObject.Find("MinionContainer").transform;
            }
        }
        catch(System.NullReferenceException e)
        {
            Debug.LogWarning("Minion spawner did not find dummy! Has endroom been spawned?");
        }

        snapFunctionality = GetComponent<SnapFunctionality>();
        StartCoroutine("SpawnRoutine");

        HealthComponent hpComponent = GetComponent<HealthComponent>();
        hpComponent.AddDeathAction(OnDeath);
        hpComponent.AddOnDamageAction(CmdOnTakeDamage);
    }


    //Spawn is a modified Update with a set amount of time (SpawnRate) between runs
    private IEnumerator SpawnRoutine()
    { 
        while(true)
        {
            if(isServer && snapFunctionality.Placed)
            {
                //Check if max amount of enemies has been reached
                if (LocalMinions.Count < MaxEnemyCount && MaxEnemyCount > 0)
                {
                    
                    //Adds 
                    if(!TankSpawner)
                    {
                        RpcPlayAnimation();
                    }
                    else
                    {
                        Spawn();
                        NetworkServer.Destroy(gameObject);
                        yield break;
                    }
                }
            }
            yield return new WaitForSeconds(Random.Range(MinDelay, MaxDelay));
        }
    }

    public void Spawn()
    {
        if(!isServer) return;
        //If not then create a GameObject from attached prefab at the spawners position and make them children of the "folder" created earlier
        GameObject npcEnemy = Instantiate(EnemyPrefab, spawnPoint.position, transform.rotation * Quaternion.Euler(0, 180, 0), MinionContainer) as GameObject;

        StateMachine machine = npcEnemy.GetComponent<StateMachine>();

        if(!TankSpawner)
        {
            AllMinions.Add(machine);
            LocalMinions.Add(machine);
        } 
        machine.Spawner = FinalRoomInteract.isEndGame ? finalRoomDummy : this;
        NetworkServer.Spawn(npcEnemy);
    }

    [Command]
    public void CmdOnTakeDamage()
    {
        foreach(StateMachine minion in LocalMinions)
        {
            if(minion.TargetEntity == null)
                minion.SetState(new ReturnToSpawnerState(minion));
        }
    }

    [ClientRpc]
    public void RpcPlayAnimation()
    {
        SpawnerAnim.SetTrigger("Spawn"); //Runs spawn through animation event
        MinionAnim.SetTrigger("Spawn");
    }

    public void OnDeath()
    {
        foreach(StateMachine minion in LocalMinions)
        {
            minion.Spawner = null;
        }
        Destroy(gameObject);
    }
}