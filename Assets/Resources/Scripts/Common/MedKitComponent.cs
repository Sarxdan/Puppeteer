using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
*
* AUTHOR:
* Kristoffer Lundgren
*
* DESCRIPTION:
* This component adds a medkit to a players inventoty if the player does not already have one.
* 
*
* CODE REVIEWED BY:
* Philip Stenmark
* 
* CONTRIBUTORS:
* Kristoffer Lundgren
* 
* CLEANED
*/

public class MedKitComponent : Interactable
{
    //Override from Interactable component
    public override void OnInteractBegin(GameObject interactor)
    {
        PlayerController playerController = interactor.GetComponent<PlayerController>();

        if (!playerController.HasMedkit)
        {
            playerController.HasMedkit = true;
            playerController.RpcAddMedkit();
            Destroy(gameObject);
        }
    }

    //Overrides from Interactable component, adds the medkit to the players inventory.
    public override void OnInteractEnd(GameObject interactor)
    {   
        // Empty
    }

    //Used to show the interact tooltip
    public override void OnRaycastEnter(GameObject interactor)
    {
        PlayerController playerController = interactor.GetComponent<PlayerController>();
        if(!playerController.HasMedkit)
            ShowTooltip(interactor);
    }
}
