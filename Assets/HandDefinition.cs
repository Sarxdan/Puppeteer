using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandDefinition : MonoBehaviour
{
    public Transform Right;
    public Transform Left;

    public FluidSimulation HeldMagazine;

    private PlayerController player;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    public void ReleaseMag()
    {
        if(!enabled) return;
        player.AnimController.SetBool("Reload", false);
        player.FPVAnimController.SetBool("Reload", false);

        if(player.CurrentWeapon == null) return;

        HeldMagazine.MaxLiquidAmount = player.CurrentWeaponComponent.LiquidScript.MaxLiquidAmount;
        HeldMagazine.CurrentLiquidAmount = player.CurrentWeaponComponent.LiquidScript.CurrentLiquidAmount;

        HeldMagazine.gameObject.SetActive(true);
        player.CurrentWeaponComponent.HideMagazine();
    }

    public void AttachMag()
    {
        if(!enabled) return;
        if(player.CurrentWeapon == null) return;

        HeldMagazine.CurrentLiquidAmount = HeldMagazine.MaxLiquidAmount;

        player.CurrentWeaponComponent.ShowMagazine();
        HeldMagazine.gameObject.SetActive(false);
        player.CurrentWeaponComponent.StopCooldown();
    }
}
