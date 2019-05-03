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
	void Start()
    {
		//level = transform.parent.gameObject;
		if (!isServer)
		{
			return;
		}
		Spawners = GetComponent<SnapPointContainer>().FindSnapPoints();
		foreach (var snapPoint in Spawners)
		{
			if (snapPoint is TrapSnapPoint)
			{
				Spawners.Remove(snapPoint);
			}
		}
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
		CmdSpawnItem(spawner, WeaponList[WeaponIndex]);

	}
	// Spawns a Ammo prefab.
	public void SpawnAmmo(GameObject spawner)
	{
		Debug.Log("Spawn a Ammo");

		CmdSpawnItem(spawner, AmmoItem);
	}
	// Spawns a PowerUp prefab.
	public void SpawnPowerUp(GameObject spawner)
	{
		Debug.Log("Spawn a PowerUp");

		CmdSpawnItem(spawner, PowerUpItem);
	}
	[Command]
	public void CmdSpawnItem(GameObject spawner, GameObject item)
	{
		Debug.Log("Item Spawned");
		spawner.GetComponent<ItemSnapPoint>().Occupied = true;
		var spawnableObject = Instantiate(item, spawner.transform);
		NetworkServer.Spawn(spawnableObject);
	}

}
