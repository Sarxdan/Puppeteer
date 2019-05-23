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
    [FMODUnity.EventRef] public string s_Spin;
    FMOD.Studio.EventInstance spin;
    
    
    public void SpinUp()
    {
        spin = FMODUnity.RuntimeManager.CreateInstance(s_Spin);
        spin.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        spin.start();
    }

    public void SpinDown()
    {
        spin.setParameterByName("Stop", 1);
        spin.release();
    }
}
