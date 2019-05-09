﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra "Sanders" Andersson
 * 
 * DESCRIPTION:
 * Script for both spikes and chandelier trap.
 * 
 * CODE REVIEWED BY:
 * Philip Stenmark
 * 
 * 
 */

public class BasicTrap : TrapComponent
{
    [FMODUnity.EventRef] public string activate;

    //Add only puppets to those who are to take damage
    public override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (Puppets.Count <= 0)
            {
                StartCoroutine("TrapTimer");
            }
            Puppets.Add(other.gameObject);
        }
    }

    //Remove puppet that walked outside of the aoe
    public override void OnTriggerExit(Collider other)
    {
        Puppets.Remove(other.gameObject);
    }

    //Damage all puppets inside aoe
    public override void ActivateTrap()
    {
        
        foreach (GameObject puppet in Puppets)
        {
            puppet.GetComponent<HealthComponent>().Damage(Damage);
        }

        FMODUnity.RuntimeManager.PlayOneShot(activate, transform.position);

        StartCoroutine("StunPuppet");
        StartCoroutine("DestroyTimer");
    }

    //Stun puppet to make clear that spike was activated
    public IEnumerator StunPuppet()
    {
        foreach (GameObject puppet in Puppets)
        {
            puppet.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }

        yield return new WaitForSeconds(.7f);

        foreach (GameObject puppet in Puppets)
        {
            puppet.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
        }
    }
}