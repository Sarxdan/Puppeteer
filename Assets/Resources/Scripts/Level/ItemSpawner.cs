using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund
*
* DESCRIPTION:
* Point holding info for placing traps.
*
* CODE REVIEWED BY:
*
* CONTRIBUTORS:
* 
*/

public class ItemSpawner : NetworkBehaviour
{
	public uint NumberOfSpawns;
	private List<SnapPointBase> spawners;
	private GameObject level;

	// Refrences to all prefabs that can spawn.
	public GameObject[] WeaponList;
	public GameObject AmmoItem;
	public GameObject PowerUpItem;

	// Saves refrence to SnapPoints.
	public List<SnapPointBase> SnapPoints;

	// Finds snap point and saves for later use.
	public List<SnapPointBase> FindSnapPoints()
	{
		if (SnapPoints.Count == 0)
			SnapPoints.AddRange(GetComponentsInChildren<SnapPointBase>());

		return SnapPoints;
	}

	// Calculates the correct percentage of all spawnable entitys and then randoms a percentile, spawning its corresponding entity type.
	public void SpawnItems()
  	{
		// Get all points.
		spawners = FindSnapPoints();
		List<SnapPointBase> itemSpawnPoint = new List<SnapPointBase>();
		// Separates item and trap points.
		foreach (var snapPoint in spawners)
		{
			if (snapPoint is ItemSnapPoint)
			{
				itemSpawnPoint.Add(snapPoint);
			}
		}

		// Gets x number of indexes that is going to spawn item.
		if (NumberOfSpawns > itemSpawnPoint.Count)
		{
			NumberOfSpawns = (uint)itemSpawnPoint.Count;
		}

		var randomList = GetRandom(0, itemSpawnPoint.Count, NumberOfSpawns);
		// Calls Spawn on the random spawner and using the itemSpawnPoint internal spawn values. 
		foreach (var index in randomList)
		{
			var spawner = itemSpawnPoint[index].GetComponent<ItemSnapPoint>();

			spawner.AltStart();

			var weaponPercent = spawner.GetChanceOfWeapon();
			var ammoPercent = spawner.GetChanceOfAmmo();
			var powerPercent = spawner.GetChanceOfPowerUp();

			int chance = Random.Range(0, 100);
			if (chance < weaponPercent)
			{
				SpawnWeapon(itemSpawnPoint[index].gameObject);
			}
			else if (chance > weaponPercent & chance < (weaponPercent + ammoPercent))
			{
				SpawnAmmo(itemSpawnPoint[index].gameObject);
			}
			else if (chance > weaponPercent + ammoPercent)
			{
				SpawnPowerUp(itemSpawnPoint[index].gameObject);
			}
		}
	}

	// Gets a num long list of random but unice int that is not the same 
	public List<int> GetRandom(int min, int max, uint num)
	{
		List<int> result = new List<int>();
		for (int i = 0; i < num; i++)
		{
			int randomNum = Random.Range(min, max);

			if (result.Contains(randomNum))
			{
				i--;
				continue;
			}
			else
				result.Add(randomNum);
		}

		if (result.Count == num)
		{
			return result;
		}
		else
		{
			Debug.Log("ERROR: List not correct size.");
			return null;
		}
	}

	// Randoms a weapon to spawn (Might what to add a rarity to weapons.. then internal values like above would work nice.)
	public void SpawnWeapon(GameObject spawner)
	{
		int WeaponIndex = Random.Range(0, WeaponList.Length);
		SpawnItem(spawner, WeaponList[WeaponIndex]);
	}
	// Spawns a Ammo prefab.
	public void SpawnAmmo(GameObject spawner)
	{
		SpawnItem(spawner, AmmoItem);
	}
	// Spawns a PowerUp prefab.
	public void SpawnPowerUp(GameObject spawner)
	{
		SpawnItem(spawner, PowerUpItem);
	}
	// Previusly a [Command] now just does the spawning.
	public void SpawnItem(GameObject spawner, GameObject item)
	{
		spawner.GetComponent<ItemSnapPoint>().Used = true;
		var spawnableObject = Instantiate(item, spawner.transform);
		NetworkServer.Spawn(spawnableObject);
	}

}
