using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 *CLEANED
 */
public class ChanceToSpawn : MonoBehaviour
{
	public bool SpawningWeapons;
	public bool SpawningAmmo;
	public bool SpawningPowerUps;

	public uint ChanceToSpawnWeapon;
	public uint ChanceToSpawnAmmo;
	public uint ChanceToSpawnPowerUp;

	private float realChanceToSpawnWeapon;
	private float realChanceToSpawnAmmo;
	private float realChanceToSpawnPowerUp;

	public void AltStart()
	{
		uint percent = 0;

		if (SpawningWeapons)
			percent += ChanceToSpawnWeapon;
		if (SpawningAmmo)
			percent += ChanceToSpawnAmmo;
		if (SpawningPowerUps)
			percent += ChanceToSpawnPowerUp;

		float newPercent = (100.0f / percent);

		if (SpawningWeapons)
			realChanceToSpawnWeapon = newPercent * ChanceToSpawnWeapon;
		if (SpawningAmmo)
			realChanceToSpawnAmmo = newPercent * ChanceToSpawnAmmo;
		if (SpawningPowerUps)
			realChanceToSpawnPowerUp = newPercent * ChanceToSpawnPowerUp;
	}

	public float GetChanceOfWeapon()
	{
		return realChanceToSpawnWeapon;
	}
	public float GetChanceOfAmmo()
	{
		return realChanceToSpawnAmmo;
	}
	public float GetChanceOfPowerUp()
	{
		return realChanceToSpawnPowerUp;
	}
}
