using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
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
 *
 */
public class PowerupRecharge : Interactable
{
    public override void OnInteractBegin(GameObject interactor)
    {
        var power = interactor.GetComponent<PowerupBase>();
        Animator anim = GetComponentInChildren<Animator>();

        if (power.isServer && power.isLocalPlayer)
        {
            // attempt to pickup recharge
            if (power != null && power.PercentageLeft == 0.0f)
            {
                anim.SetTrigger("Consume");
                power.Charged = true;
            }
        }
        else
        {
            if (power != null)
            {
                anim.SetTrigger("Consume");
                power.RpcBoostPowerup();
            }
        }
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        // empty
    }
    // (KL) Used to show the interact tooltip
    public override void OnRaycastEnter(GameObject interactor)
    {
        if (!interactor.GetComponent<PowerupBase>().Charged)
            ShowTooltip(interactor);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}