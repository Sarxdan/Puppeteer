using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 * AUTHOR:
 * Sandra "Sanders" Andersson
 * 
 * DESCRIPTION:
 * This script is placed on the bear trap when it is activated to enable interacting with it to release the trapped puppet.
 * 
 * CODE REVIEWED BY:
 * 
 * CONTRIBUTOR:
 * Kristoffer Lundgren
 */

public class BearInteract : Interactable
{
    public uint ReleaseDamage;  //The amount of damage dealt to the puppet if it releases itself
    public bool Activated = false;
    public GameObject interactor;
    public Animator anim;

    // (KL) Total time of animation state
    private float totalTime = 1.3f;
    // (KL) Reference to hud script to scale interaction progress
    public HUDScript HudScript;
    [SyncVar]
    private bool interacting;

    public float interactionProgress;

    public BearTrapSounds sounds;

    private void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        sounds = gameObject.GetComponent<BearTrapSounds>();
    }

    private void Update()
    {
        if(interacting)
        {
            interactionProgress += Time.deltaTime;
            HudScript.ScaleInteractionProgress((interactionProgress)/totalTime);
        }
    }
    // (KL) Shows the interactable tooltip if the trap has been activated
    public override void OnRaycastEnter(GameObject interactor)
    {
        if(Activated)
            ShowTooltip(interactor);
    }

    //Start release timer and open animation on server
    public override void OnInteractBegin(GameObject interactor)
    {
        if (!Activated)
        {
            return;
        }
        anim.SetBool("Releasing", true);
        var interactionController = interactor.GetComponent<InteractionController>();
        HudScript = interactor.GetComponent<HUDScript>();
        if(interactionController.isServer && interactionController.isLocalPlayer)
        {
            interacting = true;
        }
        else
        {
            RpcEnableInteracting(interactor);
        }
        interacting = true;
        this.interactor = interactor;
        sounds.Release();
    }

    //Stop release timer and close animation on server
    public override void OnInteractEnd(GameObject interactor)
    {
        if (!Activated)
        {
            return;
        }
        var interactionController = interactor.GetComponent<InteractionController>();
        if(interactionController.isServer)
        {
            RpcDisableInteracting(interactor);
            HudScript.RpcScaleZero();

        }
        interacting = false;
        anim.SetBool("Releasing", false);
        sounds.ReClose();
        interactionProgress = 0;

    }

    //Release the puppet from the trap after the interaction timer is full
    public void ReleaseFromTrapTest()
    {
        if (isServer)
        {
            GameObject target = gameObject.GetComponent<BearTrap>().Target;
            target.GetComponent<PlayerController>().UnStunned();
            RpcCallUnstuck(target);
            target.GetComponent<HealthComponent>().RemoveDeathAction(GetComponent<BearTrap>().DestroyTrap);
            //If the puppet is releasing itself, do damage
            if (interactor == target)
            {
                target.GetComponent<HealthComponent>().Damage(ReleaseDamage);
            }
            // (KL) Scale back the interaction bar to zero on client
            HudScript.ScaleInteractionProgress(0);
            // (KL) Scale back the interaction var to zero on remote client
            HudScript.RpcScaleZero();
            gameObject.GetComponent<BearTrap>().DestroyTrap();
        }
    }

    [ClientRpc]
    public void RpcEnableInteracting(GameObject interactor)
    {
        if(interactor.GetComponent<InteractionController>().isLocalPlayer)
        {
            // (KL) Set the HUD script for the local player in order to scale the interaction progress
            HudScript = interactor.GetComponent<HUDScript>();
            interacting = true;
        }
    }
    

    [ClientRpc]
    public void RpcDisableInteracting(GameObject interactor)
    {
        if(interactor.GetComponent<InteractionController>().isLocalPlayer)
        {
            interacting = false;
        }
    }


    [ClientRpc]
    public void RpcCallUnstuck(GameObject target)
    {
        target.GetComponent<PlayerController>().UnStunned();
    }
}
