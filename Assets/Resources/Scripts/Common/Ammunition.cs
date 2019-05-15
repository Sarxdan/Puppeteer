using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using Mirror;

/*
*
* AUTHOR:
* Sandra Andersson
 *
*
* DESCRIPTION:
* This component adds ammunition to the players inventory
* 
*
* CODE REVIEWED BY:
* Kristoffer Lundgren
 *
 *
* CONTRIBUTORS
 * Anton Jonsson
 *
*/


public class Ammunition : Interactable
{
    public int Liquid; //Amount of liquid added to ammo

    // Add ammunition to the puppet interacting with the ammunition container
    public override void OnInteractBegin(GameObject interactor)
    {
        PlayerController playerCont = interactor.GetComponent<PlayerController>();

        // If the puppet is the host
        if (playerCont.isServer && playerCont.isLocalPlayer)
        {
            playerCont.Ammunition += Liquid;
        }
        // If the puppet is a client
        else
        {
            playerCont.RpcAddAmmo(Liquid);
        }
        
        Destroy(gameObject);
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        // Empty
    }

    public override void OnRaycastEnter(GameObject interactor)
    {
        ShowTooltip(interactor);
    }


}
