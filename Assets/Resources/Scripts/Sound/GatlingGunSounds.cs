using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the following objects prefabs for sound:
 * Gatling Gun
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class GatlingGunSounds : WeaponSounds
{
    // Sound Events
    [FMODUnity.EventRef] public string s_SpinUp;
    [FMODUnity.EventRef] public string s_SpinDown;
    
    public void SpinUp()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_SpinUp, transform.position);
    }

    public void SpinDown()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_SpinDown, transform.position);
    }
}
