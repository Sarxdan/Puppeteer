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
 */
public class PowerupRecharge : Interactable
{
    public override void OnInteractBegin(GameObject interactor)
    {
        var power = interactor.GetComponent<PowerupBase>();

        if (power.isServer && power.isLocalPlayer)
        {
            // attempt to pickup recharge
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
        // empty
    }

    public override void OnRaycastEnter(GameObject interactor)
    {
        if(!interactor.GetComponent<PowerupBase>().Charged)
            ShowTooltip(interactor);
    }

}
