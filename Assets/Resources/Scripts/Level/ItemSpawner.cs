using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
	public uint NumberOfSpawns;
	private List<SnapPointBase> Spawners;
	private GameObject level;

	// Refrences to all prefabs that can spawn.
	public GameObject[] WeaponList;
	public GameObject AmmoItem;
	public GameObject PowerUpItem;


	// Calculates the correct percentage of all spawnable entitys and then randoms a percentile, spawning its corresponding entity type.
	public void SpawnItems()
  	{
		//level = transform.parent.gameObject;

		Spawners = GetComponent<SnapPointContainer>().FindSnapPoints();
		List<SnapPointBase> itemSpawnPoint = new List<SnapPointBase>();
		foreach (var snapPoint in Spawners)
		{
			if (snapPoint is TrapSnapPoint)
			{
				//Spawners.Remove(snapPoint);

			}
			else
			{
				itemSpawnPoint.Add(snapPoint);
			}
		}
		Spawners = itemSpawnPoint;
		if (NumberOfSpawns > Spawners.Count)
		{
			NumberOfSpawns = (uint)Spawners.Count;
		}
		var randomList = GetRandom(0, Spawners.Count, NumberOfSpawns);
		foreach (var index in randomList)
		{
			var spawner = Spawners[index].GetComponent<ItemSnapPoint>();

			spawner.AltStart();

			var weaponPercent = spawner.GetChanceOfWeapon();
			var ammoPercent = spawner.GetChanceOfAmmo();
			var powerPercent = spawner.GetChanceOfPowerUp();

			int chance = Random.Range(0, 100);
			if (chance < weaponPercent)
			{
				SpawnWeapon(Spawners[index].gameObject);
			}
			else if (chance > weaponPercent & chance < (weaponPercent + ammoPercent))
			{
				SpawnAmmo(Spawners[index].gameObject);
			}
			else if (chance > weaponPercent + ammoPercent)
			{
				SpawnPowerUp(Spawners[index].gameObject);
			}
		}
	}

	void Update()
	{
	}

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
		Debug.Log("Spawn a Ammo");

		SpawnItem(spawner, AmmoItem);
	}
	// Spawns a PowerUp prefab.
	public void SpawnPowerUp(GameObject spawner)
	{
		Debug.Log("Spawn a PowerUp");

		SpawnItem(spawner, PowerUpItem);
	}
	//[Command]
	public void SpawnItem(GameObject spawner, GameObject item)
	{
		Debug.Log("Item Spawned");
		spawner.GetComponent<ItemSnapPoint>().Used = true;
		var spawnableObject = Instantiate(item, spawner.transform);
		NetworkServer.Spawn(spawnableObject);
	}

}
