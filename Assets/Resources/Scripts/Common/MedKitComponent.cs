﻿using System.Collections;
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
*/

public class MedKitComponent : Interactable
{
    //Override from Interactable component
    public override void OnInteractBegin(GameObject interactor)
    {
        PlayerController playerController = interactor.GetComponent<PlayerController>();


            if (!playerController.HasMedkit)
                playerController.HasMedkit = true;

            playerController.RpcAddMedkit();
        
        Destroy(gameObject);
    }
    //Overrides from Interactable component, adds the medkit to the players inventory.
    public override void OnInteractEnd(GameObject interactor)
    {   
        // Empty
    }

    public override void OnRaycastEnter(GameObject interactor)
    {
        ShowTooltip(interactor);
    }

    public override void OnRaycastExit(GameObject interactor)
    {
        HideToolTip(interactor);
    }
}
