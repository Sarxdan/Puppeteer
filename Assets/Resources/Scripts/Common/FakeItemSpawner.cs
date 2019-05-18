using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * DESCRIPTION:
 * 
 * 
 * CODE REVIEWED BY:
 *
 */

public class FakeItemSpawner : NetworkBehaviour
{
    public GameObject[] Spawnables;
    private SnapFunctionality snapFunctionality;


    public void Start()
    {   
        snapFunctionality = GetComponent<SnapFunctionality>();
    }


    //Spawn is a modified Update with a set amount of time (SpawnRate) between runs
    public void Update()
    { 
        
        if(snapFunctionality.Placed)
        {
            if(!isServer){
                Destroy(gameObject);
                return;
            }
            //If not then create a GameObject from attached prefab at the spawners position and make them children of the "folder" created earlier
            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit) && hit.transform.GetComponentInParent<NavMesh>() != null)
            {
                GameObject item = Instantiate(Spawnables[Random.Range(0, Spawnables.Length - 1)], transform.position, transform.rotation, hit.transform) as GameObject;
                NetworkServer.Spawn(item);
            }
            else
            {
                Debug.LogWarning("Fake item spawner did not find which room it was placed in!");
            }
            Destroy(gameObject);
        }
    }
}