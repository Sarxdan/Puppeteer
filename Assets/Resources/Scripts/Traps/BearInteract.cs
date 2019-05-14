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
 * 
 * 
 */

public class BearInteract : Interactable
{
    public uint ReleaseDamage;  //The amount of damage dealt to the puppet if it releases itself
    public bool Activated = false;
    public GameObject interactor;
    public Animator anim;
    
    public float totalTime;
    public HUDScript HudScript;
    [SyncVar]
    private bool interacting;

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
            var currentTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            Debug.Log("Current time " + currentTime * totalTime);
            HudScript.ScaleInteractionProgress((currentTime*totalTime)/totalTime);
        }
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
            var clip = anim.GetCurrentAnimatorClipInfo(0);
            totalTime = clip[0].clip.length;
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

            HudScript.ScaleInteractionProgress(0);
            HudScript.RpcScaleZero();
            gameObject.GetComponent<BearTrap>().DestroyTrap();
        }
    }

    [ClientRpc]
    public void RpcEnableInteracting(GameObject interactor)
    {
        if(interactor.GetComponent<InteractionController>().isLocalPlayer)
        {
            HudScript = interactor.GetComponent<HUDScript>();
            var clip = anim.GetCurrentAnimatorClipInfo(0);
            totalTime = clip[0].clip.length;
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
