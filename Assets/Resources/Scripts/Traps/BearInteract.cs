using System.Collections;
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
    public uint ReleaseDamage;  //Damage on when puppet when releasing itself


    public override void OnInteractBegin(GameObject interactor)
    {
        StartCoroutine("ReleaseFromTrap", interactor);
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        StopCoroutine("ReleaseFromTrap");
    }

    public IEnumerator ReleaseFromTrap(GameObject interactor)
    {
        //Timer for releasing the puppet
        float time = 0;
        while (++time < ReleaseTimer)
        {
            yield return new WaitForSeconds(1);
        }

        GameObject target = gameObject.GetComponent<BearTrap>().target;
        target.GetComponent<PlayerController>().UnStunned();

        //Damage the puppet, if the releaser is also the trapped one
        if (interactor == target)
        {
            target.GetComponent<HealthComponent>().Damage(ReleaseDamage);
        }

        gameObject.GetComponent<BearTrap>().DestroyTrap();
    }
}
