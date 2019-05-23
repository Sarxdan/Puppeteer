using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//CLEANED

public class SpawnRemote : MonoBehaviour
{
    private EnemySpawner spawner;
    void Start()
    {
        spawner = GetComponentInParent<EnemySpawner>();
    }

    void Spawn()
    {
        spawner.Spawn();
    }
}
