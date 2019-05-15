using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StartWeaponSpawner : NetworkBehaviour
{
    // Start is called before the first frame update
    public GameObject StartWeapon;
    void Start()
    {
        if(isServer)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in players)
            {
                GameObject spawnedWeapon = Instantiate(StartWeapon, Vector3.zero, transform.rotation);
                NetworkServer.Spawn(spawnedWeapon);
                spawnedWeapon.GetComponent<WeaponComponent>().RpcPickupWeapon(spawnedWeapon, player);
            }
        }
    }
}
