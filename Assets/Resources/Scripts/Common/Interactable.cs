using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * The script may be inherited by scripts for items, weapons etc. in order for the player to interact with them in the level.
 * 
 * CODE REVIEWED BY:
 * Ludvig Björk Förare
 * 
 */

[RequireComponent(typeof(Glowable))]
public abstract class Interactable : NetworkBehaviour
{
    //Called once when an interaction has started
    public abstract void OnInteractBegin(GameObject interactor);
    //Called once when an interaction has ended
    public abstract void OnInteractEnd(GameObject interactor);

    public void OnRaycastEnter()
    {
        // enable outline effect
        var glow = GetComponent<Glowable>();
        if (glow)
        {
            glow.Toggle(true);
        }
    }

    public void OnRaycastExit()
    {
        // disable outline effect
        var glow = GetComponent<Glowable>();
        if(glow)
        {
            glow.Toggle(false);
        }
    }

    [ClientRpc]
    public void RpcPickupWeapon(GameObject weaponObject, GameObject userObject){
        
        WeaponComponent newWeapon = weaponObject.GetComponent<WeaponComponent>();
        PlayerController user = userObject.GetComponent<PlayerController>();


        //Disables new weapons collider
        newWeapon.GetComponent<CapsuleCollider>().enabled = false; 

        GameObject CurrentWeaponObject = user.CurrentWeapon;
        //If carrying a weapon, detach it and place it on new weapons location
        if(CurrentWeaponObject != null && CurrentWeaponObject.transform != transform)
        {
            WeaponComponent CurrentWeapon = CurrentWeaponObject.GetComponent<WeaponComponent>();
            CurrentWeapon.HeadTransform = null;
            CurrentWeapon.transform.SetPositionAndRotation(transform.position, transform.rotation);
            CurrentWeapon.transform.SetParent(null);
            CurrentWeapon.GetComponent<Collider>().enabled = true;
        }
        
        //Attaches new weapon to player
        user.CurrentWeapon = newWeapon.gameObject;
        newWeapon.HeadTransform = user.HeadTransform;
        user.SetWeaponAnimation(newWeapon.AnimationIndex);

        if(userObject.GetComponent<PlayerController>().isLocalPlayer)
        {
            newWeapon.transform.SetParent(user.FPVHandTransform);
        }
        else
        {
            newWeapon.transform.SetParent(user.HandTransform);
        }

        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = newWeapon.HoldRotation;

    }
}
