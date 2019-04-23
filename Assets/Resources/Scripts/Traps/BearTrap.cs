using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra "Sanders" Andersson
 * 
 * DESCRIPTION:
 * This script is placed on the bear trap for specific attributes.
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * 
 */

public class BearTrap : TrapComponent
{
    public GameObject target;

    public override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (Puppets.Count <= 0)
            {
                StartCoroutine("TrapTimer");
            }

            Puppets.Add(other.gameObject);
            //Action for deleting trap when the player dies inside of it
            other.gameObject.GetComponent<HealthComponent>().AddDeathAction(DestroyTrap);
        }
    }

    //Makes sure the puppet will not take damage from trap when he exits the aoe
    //and the trap won't get removed if the player dies
    public override void OnTriggerExit(Collider other)
    {
        other.gameObject.GetComponent<HealthComponent>().RemoveDeathAction(DestroyTrap);
        Puppets.Remove(other.gameObject);
    }
    
    //Stun and damage the puppet that last entered the trap
    //and enable interaction with it for releasing puppet
    public override void ActivateTrap()
    {
        target = Puppets[0];
        target.GetComponent<HealthComponent>().Damage(Damage);
        target.GetComponent<PlayerController>().Stunned();

        gameObject.GetComponent<BearInteract>().enabled = true;
    }

    public override void DestroyTrap()
    {
        target.GetComponent<PlayerController>().UnStunned();

        Destroy(gameObject);
    }
}
