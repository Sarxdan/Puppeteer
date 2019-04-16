using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Script is placed on the weapons for all weapon logic.
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * 
 */
public class WeaponComponent : Interactable
{
    //Determines how much liquid this weapon can hold
    public int Capacity;

    public int LiquidLeft;
    public int LiquidPerRound;

    //Weapon attributes
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

    public static float RecoilRecovery = 20.0f;
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
                //Make Damage
                var health = hitInfo.transform.GetComponent<HealthComponent>();
                if(health != null)
                {
                    health.Damage(this.Damage);
                }
            }
        }

        //Adds recoil and cooldown, and subtracts ammo left
        recoil += RecoilAmount;
        cooldown += FiringSpeed;
        LiquidLeft -= LiquidPerRound;
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

        // perform recoil
        recoil = Mathf.Clamp(recoil - RecoilRecovery * Time.deltaTime, 0.0f, 45.0f);
        HeadTransform.localEulerAngles += Vector3.left * recoil;
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
