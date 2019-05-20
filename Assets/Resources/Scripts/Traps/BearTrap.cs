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
    public GameObject Target;

    public void Start()
    {
        Puppets = new List<GameObject>();
        Anim = gameObject.GetComponent<Animator>();
        GetComponent<Glowable>().enabled = false;
    }

    public void FixedUpdate()
    {
        Debug.Log(Puppets[0]);
        if(Target != null)
        {
            Target.transform.position = Vector3.Lerp(Target.transform.position, transform.position, 0.5f);
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.tag == "Player")
        {
            if (Puppets.Count <= 0)
            {
                StartCoroutine("TrapTimer");
                Anim.SetBool("HasTarget",true);
            }

            Puppets.Add(other.gameObject);
            //Action for deleting trap when the player dies inside of it
            if(isServer)
            {
                other.gameObject.GetComponent<HealthComponent>().AddDeathAction(DestroyTrap);
            }
        }
    }

    //Makes sure the puppet will not take damage from trap when he exits the aoe
    //and the trap won't get removed if the player dies
    public override void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if(isServer)
            {
                other.gameObject.GetComponent<HealthComponent>().RemoveDeathAction(DestroyTrap);
            }
            Puppets.Remove(other.gameObject);

            if (Puppets.Count == 0)
            {
                Anim.SetBool("HasTarget", false);
            }
        }
    }
    
    //Stun and damage the puppet that last entered the trap
    //and enable interaction with it for releasing puppet
    public override void ActivateTrap()
    {

        if (Puppets.Count > 0)
        {
            Target = Puppets[0];
            //Prevents damage from being dealt twise.
            if (isServer)
                Target.GetComponent<HealthComponent>().Damage(Damage);

            Target.GetComponent<PlayerController>().Stunned();

            GetComponent<Glowable>().enabled = true;
            gameObject.GetComponent<BearInteract>().Activated = true;
        }
        //Destroy the trap if it closes empty
        else
        {
            StartCoroutine("DestroyTimer");
        }
    }
}
