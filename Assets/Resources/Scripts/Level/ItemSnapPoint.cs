using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund
*
* DESCRIPTION:
* Point holding info for placing items (pickups).
*
* CODE REVIEWED BY:
* Sandra "Sanders" Andersson
*
* CONTRIBUTORS:
* Sandra Andersson (Added impl. for medkit)
* 
* CLEANED
*/

public class ItemSnapPoint : SnapPointBase
{
	[Header("What should spawn here.")]
    public bool SpawningWeapons;
	public bool SpawningAmmo;
	public bool SpawningPowerUps;
	public bool SpawningMedKit;

	[Header("percent chance to spawn. (silent clamp)")]
	public uint ChanceToSpawnWeapon;
	public uint ChanceToSpawnAmmo;
	public uint ChanceToSpawnPowerUp;
	public uint ChanceToSpawnMedKit;

	//The clamped chance of spawn.
	private float realChanceToSpawnWeapon;
	private float realChanceToSpawnAmmo;
	private float realChanceToSpawnPowerUp;
	private float realChanceToSpawnMedKit;

	public void AltStart()
	{
		uint percent = 0;

		if (SpawningWeapons)
			percent += ChanceToSpawnWeapon;
		if (SpawningAmmo)
			percent += ChanceToSpawnAmmo;
		if (SpawningPowerUps)
			percent += ChanceToSpawnPowerUp;
        if (SpawningMedKit)
            percent += ChanceToSpawnMedKit;

        float newPercent = (100.0f / percent);

		if (SpawningWeapons)
			realChanceToSpawnWeapon = newPercent * ChanceToSpawnWeapon;
		if (SpawningAmmo)
			realChanceToSpawnAmmo = newPercent * ChanceToSpawnAmmo;
		if (SpawningPowerUps)
			realChanceToSpawnPowerUp = newPercent * ChanceToSpawnPowerUp;
        if (SpawningMedKit)
            realChanceToSpawnMedKit = newPercent * ChanceToSpawnMedKit;
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
    public float GetChanceOfMedKit()
    {
        return realChanceToSpawnMedKit;
    }
}