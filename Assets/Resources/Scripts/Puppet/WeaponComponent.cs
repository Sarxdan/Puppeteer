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
                Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.black, 1.0f);
                Debug.DrawRay(transform.position, -transform.forward * 100.0f, Color.red, 0.2f);
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
        if(HeadTransform != null)
        {
            recoil = Mathf.Clamp(recoil - RecoilRecovery * Time.deltaTime, 0.0f, 45.0f);
            var rotation = HeadTransform.localEulerAngles + Vector3.left * recoil;

            HeadTransform.localEulerAngles = rotation;
            rotation.y = 180.0f;
            transform.localEulerAngles = -rotation;
        }
    }

    public override void OnInteractBegin(GameObject interactor)
    {
        var weapon = interactor.GetComponentInChildren<WeaponComponent>();

        if(weapon != null && weapon.transform != transform)
        {
            weapon.GetComponent<WeaponComponent>().HeadTransform = null;
            weapon.transform.SetPositionAndRotation(transform.position, transform.rotation);
            weapon.transform.SetParent(null);
        }

        this.HeadTransform = interactor.GetComponentInChildren<Camera>().transform;

        interactor.GetComponent<PlayerController>().CurrentWeapon = gameObject;
        transform.SetParent(interactor.transform);
        transform.localPosition = new Vector3(0.5f, -0.4f, 0.5f);
    }

    public override void OnInteractEnd(GameObject interactor)
    {
    }
}
