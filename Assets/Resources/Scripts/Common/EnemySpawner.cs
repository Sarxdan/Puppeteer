using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
 * 
 */

public class EnemySpawner : MonoBehaviour
{
    public GameObject EnemyPrefab;
    private GameObject spawnedEnemies;
    public float SpawnRate;
    public int MaxEnemyCount;
    public Transform[] SpawnPoints;
    public List<GameObject> EnemyCount = new List<GameObject>();


    public void Start()
    {
        //A little snippet to make sure the spawnrate isnt negative and it sets it to 3 wich is "default"
        if (SpawnRate <= 0)
        {
            SpawnRate = 3;
        }
        gameObject.tag = "EnemySpawner";
        //Creates Spawn and defines the start delay and time between runs
        InvokeRepeating("Spawn", SpawnRate, SpawnRate);
        //Create an empty GameObject to use as a folder for the npc prefabs
        spawnedEnemies = new GameObject
        {
            name = "Spawned Enemies"
        };
    }

    //Spawn is a modified Update with a set amount of time (SpawnRate) between runs
    public void Spawn()
    { 
        //Steps through all spawners in order to spawn enemies
        for (int i = 0; i < SpawnPoints.Length; i++)
        { 
            //Check if max amount of enemies has been reached
            if (EnemyCount.Count >= MaxEnemyCount && MaxEnemyCount > 0)
            {
                return;
            }
            //If not then create a GameObject from attached prefab at the spawners position and make them children of the "folder" created earlier
            GameObject npcEnemy = Instantiate(EnemyPrefab, SpawnPoints[i].position, SpawnPoints[i].rotation) as GameObject;
            npcEnemy.transform.parent = GameObject.Find("Spawned Enemies").transform;

            npcEnemy.tag = "npcEnemies";
            EnemyCount.Add(npcEnemy);
        }
    }
}