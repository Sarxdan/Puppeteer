﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponComponent : Interactable
{
    //Determines how much liquid this weapon can hold
    public int Capacity;

    public int LiquidLeft;
    public int LiquidPerRound;
    public uint Damage;
    public uint NumShots;

    [Range(0.0f, 1.0f)]
    public float FiringSpeed;
    [Range(0.0f, 4.0f)]
    public float ReloadTime;
    [Range(0.0f, 1.0f)]
    public float Spread;
    [Range(0.0f, 10.0f)]
    public float RecoilAmount;

    public Transform HeadTransform;

    //Time left until weapon can be used again
    private float cooldown;
    private float recoil = 0.0f;

    //Attemps to fire the weapon
    public void Use()
    {
        if (cooldown != 0 || LiquidLeft == 0)
            return;

        for(int i = 0; i < NumShots; i++)
        {
            // calculate spread
            Vector3 offset = Random.insideUnitSphere * Spread;

            RaycastHit hitInfo;
            if(Physics.Raycast(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), Camera.main.transform.forward + offset, out hitInfo))
            {
                var health = hitInfo.transform.GetComponent<HealthComponent>();
                if(health != null)
                {
                    health.Damage(this.Damage);
                }
                Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, 1.0f);
            }
        }

        recoil += RecoilAmount;
        cooldown += FiringSpeed;
        LiquidLeft -= LiquidPerRound;
    }

    public void Recoil()
    {
        recoil = Mathf.Clamp(recoil - 10.0f * Time.deltaTime, 0.0f, 45.0f);
        HeadTransform.localEulerAngles += Vector3.left * recoil;
    }

    //Attemps to reload the weapon to its maximum capacity by the given input amount
    public void Reload(ref int liquidInput)
    {
        if (cooldown != 0)
            return;

        int amount = Mathf.Min(Capacity - LiquidLeft, liquidInput);
        liquidInput -= amount;
        LiquidLeft += amount;

        cooldown += ReloadTime;
    }

    void Update()
    {
        cooldown = Mathf.Max(0.0f, cooldown -= Time.deltaTime);
        Recoil();
    }

    public override void OnInteractBegin(GameObject interactor)
    {
        var player = interactor.GetComponent<PlayerController>();

        if(player.CurrentWeapon != null)
        {  
            // TODO: attach weapon to player
        }
        else
        {
            // TODO: attach weapon to player
        }
    }

    public override void OnInteractEnd(GameObject interactor)
    {
    }
}
