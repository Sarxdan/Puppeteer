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
 * Benjamin Vesterlund
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
    [Range(0.0f, 0.2f)]
    public float Spread;
    [Range(0.0f, 4.0f)]
    public float RecoilAmount;
    [Range(0.0f, 1.0f)]
    public float DamageDropoff;

    // time required before weapon is ready to fire (i.e gatling gun spinning up)
    [Range(0.0f, 4.0f)]
    public float ChargeTime;

    public static float RecoilRecovery = 20.0f;
    public Transform HeadTransform;

    //Time left until weapon can be used again
    private float cooldown;
    private float recoil;
    private float charge;

    //Attemps to fire the weapon
    public void Use()
    {
        charge += Time.deltaTime;

        if (cooldown != 0 || LiquidLeft < LiquidPerRound || charge < ChargeTime)
        {
            // unable to fire
            return;
        }

        for(int i = 0; i < NumShots; i++)
        {
            // calculate spread
            Vector3 offset = Random.insideUnitSphere * Spread;

            RaycastHit hitInfo;
            if(Physics.Raycast(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), Camera.main.transform.forward + offset, out hitInfo))
            {
                // deal damage to target if possible
                var health = hitInfo.transform.GetComponent<HealthComponent>();
                if(health != null)
                {
                    uint damage = (uint)(this.Damage * Mathf.Pow(DamageDropoff, hitInfo.distance / 10.0f));
                    health.Damage(damage);
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
        // reload not possible if recently fired, currently reloading or weapon too charged
        if (cooldown != 0 || (ChargeTime != 0 && charge > ChargeTime))
            return;

        int amount = Mathf.Min(Capacity - LiquidLeft, liquidInput);
        liquidInput -= amount;
        LiquidLeft += amount;

        // disallow firing while reloading
        cooldown += ReloadTime;
    }

    void Update()
    {
        // decrease cooldown constantly
        cooldown = Mathf.Max(0.0f, cooldown -= Time.deltaTime);
        // decrease weapon charge
        charge = Mathf.Max(0.0f, charge -= Time.deltaTime * 0.5f);

        // perform recoil
        if (HeadTransform != null)
        {
            recoil = Mathf.Clamp(recoil - RecoilRecovery * Time.deltaTime, 0.0f, 45.0f);

            // rotate head according to the recoil amount
            var rotation = HeadTransform.localEulerAngles + Vector3.left * recoil;
            HeadTransform.localEulerAngles = rotation;
            rotation.y = 180.0f;
            transform.localEulerAngles = -rotation;
        }
    }

    private void OnGUI()
    {
        // temporary crosshair 
        GUI.Box(new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 10, 10), "");
    }

    public override void OnInteractBegin(GameObject interactor)
    {
        var weapon = interactor.GetComponentInChildren<WeaponComponent>();

        // swap weapon with current
        if(weapon != null && weapon.transform != transform)
        {
            weapon.GetComponent<WeaponComponent>().HeadTransform = null;
            weapon.transform.SetPositionAndRotation(transform.position, transform.rotation);
            weapon.transform.SetParent(null);
        }

        this.HeadTransform = interactor.GetComponentInChildren<Camera>().transform;
        interactor.GetComponent<PlayerController>().CurrentWeapon = gameObject;
        transform.SetParent(interactor.transform);
        // TODO: attach to player
        transform.localPosition = new Vector3(0.4f, -0.4f, 0.6f);
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        // empty
    }
}
