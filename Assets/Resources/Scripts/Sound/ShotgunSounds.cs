using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the following objects prefabs for sound:
 * Shotgun
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class ShotgunSounds : WeaponSounds
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Pump;

    public void Pump()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Pump, transform.position);
    }
}
