using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Holds a charge for any player powerup. 
 * May only be picked up if player's power is not already charged.
 * 
 * CODE REVIEWED BY:
 * Anton Jonsson (24/4)
 *
 * CONTRIBUTORS:
 * Sandra Andersson (Implemented with animation)
 * Kristoffer Lundgren
 * 
 *CLEANED
 */

public class PowerupRecharge : Interactable
{
    public override void OnInteractBegin(GameObject interactor)
    {
        var power = interactor.GetComponent<PowerupBase>();
        Animator anim = GetComponentInChildren<Animator>();

        if (power.isServer && power.isLocalPlayer)
        {
            //Attempt to pickup recharge
            if (power != null && power.PercentageLeft == 0.0f)
            {
                power.Charged = true;
            }
        }
        else
        {
            if (power != null)
            {
                power.RpcBoostPowerup();
            }
        }
        Destroy(gameObject);
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        //Empty
    }
    //Used to show the interact tooltip
    public override void OnRaycastEnter(GameObject interactor)
    {
        if (!interactor.GetComponent<PowerupBase>().Charged)
            ShowTooltip(interactor);
    }
}