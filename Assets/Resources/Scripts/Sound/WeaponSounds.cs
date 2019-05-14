﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the following objects prefabs for sound:
 * Pistol
 * Rifle
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class WeaponSounds : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Pickup;
    [FMODUnity.EventRef] public string s_Shoot;
    FMOD.Studio.EventInstance shoot;
    [FMODUnity.EventRef] public string s_OutOfAmmo;
    [FMODUnity.EventRef] public string s_Reload;
    [FMODUnity.EventRef] public string s_Foley;
    
    public void Pickup()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Pickup, transform.position);
    }

    public void Shoot(float ammoLeft)
    {
        shoot = FMODUnity.RuntimeManager.CreateInstance(s_Shoot);
        shoot.setParameterByName("Ammo", ammoLeft);
        shoot.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        shoot.start();
        shoot.release();
    }

    public void OutOfAmmo()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_OutOfAmmo, transform.position);
    }

    public void Reload()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Reload, transform.position);
    }

    public void Foley()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Foley, transform.position);
    }
}
