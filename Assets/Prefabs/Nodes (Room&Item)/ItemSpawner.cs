using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{

	public GameObject[] Spawners;

	// Input to setup the spawnrate and chance to spawn.
	public bool Weapon;
	public uint PercentOfDropWeapon;
	public bool Ammo;
	public uint PercentOfDropAmmo;
	public bool PowerUp;
	public uint PercentOfDropPowerUp;


	// internal values (incase invalid percentage is entered)
	public uint percent;
	public float weaponPercent;
	public float ammoPercent;
	public float powerPercent;

	// Refrences to all prefabs that can spawn.
	public GameObject[] WeaponList;
	public GameObject AmmoItem;
	public GameObject PowerUpItem;


	// Calculates the correct percentage of all spawnable entitys and then randoms a percentile, spawning its corresponding entity type.
	void Start()
    {
		if (!isServer)
		{
			return;
		}

		Debug.Log("Snopp");
		if (Weapon)
			percent += PercentOfDropWeapon;
		if (Ammo)
			percent += PercentOfDropAmmo;
		if (PowerUp)
			percent += PercentOfDropPowerUp;

		float newPercent = (100.0f / percent);

		if (Weapon)
			weaponPercent = newPercent * PercentOfDropWeapon;
		if (Ammo)
			ammoPercent = newPercent * PercentOfDropAmmo;
		if (PowerUp)
			powerPercent = newPercent * PercentOfDropPowerUp;

		int chance = Random.Range(0, 100);
		if (chance < weaponPercent & Weapon)
		{
			Debug.Log("W spawned");
			SpawnWeapon();
		}
		else if (chance > weaponPercent & chance < (weaponPercent + ammoPercent) & Ammo)
		{
			Debug.Log("A spawned");
			SpawnAmmo();
		}
		else if (chance > weaponPercent + ammoPercent & PowerUp)
		{
			Debug.Log("P spawned");
			SpawnPowerUp();
		}
	}

	void Update()
	{
	}

	// Randoms a weapon to spawn (Might what to add a rarity to weapons.. then internal values like above would work nice.)
	public void SpawnWeapon()
	{
		Debug.Log("Spawn a Weapon");

		int WeaponIndex = Random.Range(0, WeaponList.Length);
		CmdSpawnItem(WeaponList[WeaponIndex]);

	}
	// Spawns a Ammo prefab.
	public void SpawnAmmo()
	{
		Debug.Log("Spawn a Ammo");

		CmdSpawnItem(AmmoItem);
	}
	// Spawns a PowerUp prefab.
	public void SpawnPowerUp()
	{
		Debug.Log("Spawn a PowerUp");

		CmdSpawnItem(PowerUpItem);
	}
	[Command]
	public void CmdSpawnItem(GameObject item)
	{
		Debug.Log("Weapon spawned");
		Instantiate(item);
		NetworkServer.Spawn(item);
	}

}
