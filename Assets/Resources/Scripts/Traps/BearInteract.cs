using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [FMODUnity.EventRef] 
    public string opening;
    FMOD.Studio.EventInstance open;

    private void Start()
    {
        anim = gameObject.GetComponent<Animator>();
    }

    //Start release timer and open animation
    public override void OnInteractBegin(GameObject interactor)
    {
        if (!Activated)
        {
            return;
        }

        anim.SetBool("Releasing", true);
        this.interactor = interactor;
        open = FMODUnity.RuntimeManager.CreateInstance(opening);
        open.setParameterByName("Stop", 0f);
        open.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        open.start();
    }

    //Stop release timer and close animation
    public override void OnInteractEnd(GameObject interactor)
    {
        if (!Activated)
        {
            return;
        }

        anim.SetBool("Releasing", false);
        open.setParameterByName("Stop", 1f);
        open.release();
    }

    //Release the puppet from the trap after the interaction timer is full
    //TODO: Play opening sound
    public void ReleaseFromTrapTest()
    {
        GameObject target = gameObject.GetComponent<BearTrap>().Target;
        target.GetComponent<PlayerController>().UnStunned();

        //If the puppet is releasing itself, do damage
        if (interactor == target)
        {
            target.GetComponent<HealthComponent>().Damage(ReleaseDamage);
        }

        gameObject.GetComponent<BearTrap>().DestroyTrap();
    }
}
