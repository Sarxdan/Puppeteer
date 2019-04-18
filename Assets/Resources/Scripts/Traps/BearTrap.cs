using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrap : TrapComponent
{
    public override void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision!");
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Puppet added" + Puppets.Count);
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
    
    public override void ActivateTrap()
    {
        //TODO: Disable walking for the puppet
        GameObject target = Puppets[0];
        target.GetComponent<HealthComponent>().Damage(Damage);

        Debug.Log("Ultimate Bang");

        StartCoroutine("DestroyTimer");
    }
    public override void DestroyTrap()
    {
        //TODO: Enable walking for the puppet
        Destroy(gameObject);
    }
}
