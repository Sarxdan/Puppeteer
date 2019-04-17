using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Script is placed on entities that will have HP
 * 
 * CODE REVIEWED BY:
 * Ludvig Björk Förare
 * 
 * 
 */

public class HealthComponent : MonoBehaviour
{
    //Callback function used when health reaches zero
    public delegate void OnZeroHealth();

    public uint Health;
    public uint MaxHealth;

    [Range(0.0f, 1.0f)]
    public float MaxRegenRatio;

    [Range(0.0f, 1.0f)]
    public float MaxReviveRatio;

    public uint RegenSpeed;
    public uint RegenDelay;     //Time before regen starts
    public bool AllowRegen;     //Can I regen?

    private OnZeroHealth zeroHealthAction;

    public void Damage(uint damage)
    {
        if (Health <= 0)
            return;

        StopCoroutine("RegenRoutine");

        //Cap the HP so it doesn't go below 0
        Health = (uint)Mathf.Max(0, Health -= damage);
        if (Health == 0)
        {
            // perform death actions
            this.zeroHealthAction();
            AllowRegen = false;
        }
        else if (AllowRegen)
        {
            StartCoroutine("RegenRoutine");
        }
    }
    
    //Starts regenerate HP after delay, up to the max amount of regen
    private IEnumerator RegenRoutine()
    {
        yield return new WaitForSeconds(RegenDelay);

        while(Health < (MaxHealth * MaxRegenRatio))
        {
            Health++;
            yield return new WaitForSeconds(RegenSpeed);
        }
    }

    //Revives the player instantly to the given max revive health
    public void Revive()
    {
        Health = (uint)(MaxHealth * MaxReviveRatio);
        AllowRegen = true;
    }

    //Registers a new zero health delegate
    public void AddDeathAction(OnZeroHealth action)
    {
        this.zeroHealthAction += action;
    }
}
