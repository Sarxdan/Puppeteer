﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Used for objects that may be revived upon reaching zero health. 
 * 
 * CODE REVIEWED BY:
 * Benjamin Vesterlund
 * 
 * 
 */
public class ReviveComponent : Interactable
{
    // delay until revive is complete
    public int ReviveDelay;
    // delay until the object may no longer be revived
    public int DeathDelay;
    // determines if the revive requires a medkit
    public bool RequireMedkit = true;

    // Krig interact progress stuff
    private HUDScript hudScript;

    private HealthComponent healthComponent;

    void Start()
    {
        // register death action
        healthComponent = GetComponent<HealthComponent>();
        healthComponent.AddDeathAction(OnZeroHealth);
    }

    // an object has started to interact this object
    public override void OnInteractBegin(GameObject interactor)
    {
        var interactionController = interactor.GetComponent<InteractionController>();
        hudScript = interactor.GetComponent<HUDScript>();
        if(interactionController.isServer && interactionController.isLocalPlayer)
        {
            
        }
        else
        {
            RpcGetHudScript(interactor);
        }
        StartCoroutine("ReviveRoutine", interactor);
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        StopCoroutine("ReviveRoutine");
        var interactionController = interactor.GetComponent<InteractionController>();
        if(!interactionController.isServer && !interactionController.isLocalPlayer)
        {
            hudScript.RpcScaleZero();
        }
        hudScript.ScaleInteractionProgress(0);
    }

    // called when the health of this object reaches zer zo
    private void OnZeroHealth()
    {
        //hudScript.ScaleInteractionProgress(0);
        StartCoroutine("DeathRoutine");
    }

    private IEnumerator DeathRoutine()
    {
        int time = 0;
        while(++time < DeathDelay)
        {
            if (healthComponent.Health != 0)
            {
                // someone has revived!
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
        // TODO: perform death action across network
        Destroy(gameObject);
    }

    private IEnumerator ReviveRoutine(GameObject reviver)
    {
        if (RequireMedkit && !reviver.GetComponent<PlayerController>().HasMedkit)
        {
            // no medkit available
            yield break;
        }

        float time = 0;
        while(time < ReviveDelay)
        {
            if (healthComponent.Health != 0)
            {
                // someone has revived already
                yield break;
            }
            time += 0.1f;
            //hudScript.ScaleInteractionProgress(time/ReviveDelay);
            RpcScaleProgress(time/ReviveDelay);
            yield return new WaitForSeconds(0.1f);
        }

        // revive successful

        healthComponent.Revive();
        var revivingPlayer = reviver.GetComponent<InteractionController>();
        if(revivingPlayer.isLocalPlayer && revivingPlayer.isServer)
        {
            hudScript.RpcScaleZero();

        }
        hudScript.RpcScaleZero();
        if(RequireMedkit)
        {
            // consume medkit if required
            reviver.GetComponent<PlayerController>().HasMedkit = false;
        }
    }
    [ClientRpc]
    public void RpcGetHudScript(GameObject interactor)
    {
        if(interactor.GetComponent<HealthComponent>().isLocalPlayer)
        {
            hudScript = interactor.GetComponent<HUDScript>();
        }
    }

    [ClientRpc]
    public void RpcScaleProgress(float scale)
    {
        if(hudScript != null)
        {
            hudScript.ScaleInteractionProgress(scale);
        }
    }
}


