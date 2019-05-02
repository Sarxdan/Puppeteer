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
    //[FMODUnity.EventRef] public string opening;
    
    public GameObject Target;

    public override void OnTriggerEnter(Collider other)
    {

        Debug.Log("Entered");
        if (other.gameObject.tag == "Player")
        {
            if (Puppets.Count <= 0)
            {
                Debug.Log("Start Timer");
                StartCoroutine("TrapTimer");
                Anim.SetBool("HasTarget",true);
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

        if (Puppets.Count == 0)
        {
            Anim.SetBool("HasTarget", false);
        }
    }
    
    //Stun and damage the puppet that last entered the trap
    //and enable interaction with it for releasing puppet
    //TODO: Maybe snap the puppet to center of bear trap?
    //TODO: Play Opening sound
    public override void ActivateTrap()
    {
        if (Puppets.Count > 0)
        {
            Target = Puppets[0];
            Target.GetComponent<HealthComponent>().Damage(Damage);
            Target.GetComponent<PlayerController>().Stunned();

            gameObject.GetComponent<BearInteract>().Activated = true;
        }
    }
}
