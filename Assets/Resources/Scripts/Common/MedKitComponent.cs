using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedKitComponent : Interactable
{
    [Header("Heal Amount")]
    public uint HealAmount;

    public override void OnInteractBegin(GameObject interactor)
    {
        
    }

    public override void OnInteractEnd(GameObject interactor)
    {   
        PlayerController playerController = interactor.GetComponent<PlayerController>();
        if(playerController.HasMedkit)
        {
            Destroy(gameObject);
            return;
        }else
        {
            playerController.HasMedkit = true;
            Destroy(gameObject);

        }
    }
}
