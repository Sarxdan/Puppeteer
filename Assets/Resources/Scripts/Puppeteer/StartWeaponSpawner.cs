using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/*
* AUTHOR:
* Ludvig "Kät" Björk Förare
*
* DESCRIPTION:
* Script that spawns in weapons for players joining the game. Runs for 10 sec then destroys itself
*
* CODE REVIEWED BY:
*
* CONTRIBUTORS: 
*
*/
public class StartWeaponSpawner : NetworkBehaviour
{
    public GameObject StartWeapon;
    void Start()
    {
        if(!isServer)
            Destroy(this);
        else
            Destroy(this, 10);
    }
    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if(pc.CurrentWeapon == null && pc.HasSpawned)
            {
                GameObject spawnedWeapon = Instantiate(StartWeapon, Vector3.zero, transform.rotation);
                NetworkServer.Spawn(spawnedWeapon);
                spawnedWeapon.GetComponent<NetworkIdentity>().AssignClientAuthority(player.GetComponent<NetworkIdentity>().connectionToClient);
                spawnedWeapon.GetComponent<WeaponComponent>().RpcPickupWeapon(spawnedWeapon, player);
            }
        }
    }
}
