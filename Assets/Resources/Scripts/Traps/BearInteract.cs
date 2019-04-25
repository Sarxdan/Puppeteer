﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra "Sanders" Andersson
 * 
 * DESCRIPTION:
 * This script on the bear trap when it is activated to enable interacting with it to release the trapped puppet.
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * 
 */

public class BearInteract : Interactable
{
    public float ReleaseTimer;  //Time it takes to release a puppet from trap
    public uint ReleaseDamage;  //The amount of damage dealt to the puppet if it releases itself
    public bool Activated = false;

    private void Start()
    {
        
    }

    //Start release timer
    public override void OnInteractBegin(GameObject interactor)
    {
        if (!Activated)
        {
            return;
        }

        StartCoroutine("ReleaseFromTrap", interactor);
    }

    //Stop release timer
    public override void OnInteractEnd(GameObject interactor)
    {
        if (!Activated)
        {
            return;
        }
        StopCoroutine("ReleaseFromTrap");
    }

    //Release the puppet from the trap after the interaction timer is full
    public IEnumerator ReleaseFromTrap(GameObject interactor)
    {
        //Timer for releasing the puppet
        float time = 0;
        while (++time < ReleaseTimer)
        {
            yield return new WaitForSeconds(1);
        }

        GameObject target = gameObject.GetComponent<BearTrap>().Target;
        target.GetComponent<PlayerController>().UnStunned();

        //If the puppet is releasing itself, do damage
        if(interactor == target)
        {
            target.GetComponent<HealthComponent>().Damage(ReleaseDamage);
        }
       
        gameObject.GetComponent<BearTrap>().DestroyTrap();
    }
}
