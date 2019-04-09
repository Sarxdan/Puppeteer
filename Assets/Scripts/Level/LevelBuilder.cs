using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public GameObject[] Modules;
    public GameObject StartModule;
    public GameObject GoalModule;

    public int NumModules = 20;

    void Start()
    {
        // temporary code
        for(int i = 0; i < NumModules; i++)
        {
            var module = Instantiate(Modules[Random.Range(0, Modules.Length)], transform);
            module.transform.position = new Vector3(Random.Range(-8, 8), 0.0f, Random.Range(-8, 8));
        }
    }
}
