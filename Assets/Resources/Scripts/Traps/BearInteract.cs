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
    
    public float totalTime;
    public HUDScript HudScript;
    private bool interacting;

    [FMODUnity.EventRef] 
    public string opening;
    FMOD.Studio.EventInstance open;

    private void Start()
    {
        anim = gameObject.GetComponent<Animator>();
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

    //Start release timer and open animation
    public override void OnInteractBegin(GameObject interactor)
    {
        if (!Activated)
        {
            return;
        }
        anim.SetBool("Releasing", true);
        var temp = anim.GetCurrentAnimatorClipInfo(0);
        totalTime = temp[0].clip.length;
        Debug.Log("total time of clip" + totalTime);
        HudScript = interactor.GetComponentInChildren<HUDScript>();
        interacting = true;

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
        interacting = false;
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

        HudScript.ScaleInteractionProgress(0);
        gameObject.GetComponent<BearTrap>().DestroyTrap();
    }
}
