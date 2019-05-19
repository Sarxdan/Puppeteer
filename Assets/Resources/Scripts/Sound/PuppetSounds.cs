using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the following objects prefabs for sound:
 * Froggo
 * Doggo
 * Pekko
 * Gekko
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class PuppetSounds : CharacterSounds
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Jump;
    [FMODUnity.EventRef] public string s_Land;
    [FMODUnity.EventRef] public string s_Revive;
    [FMODUnity.EventRef] public string s_UseMedkit;

    public Music music;
    
    public void Jump()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Jump, transform.position);
    }

    public void Land()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Land, transform.position);
    }
    public void Death()
    {
        music.Downed();

        die = FMODUnity.RuntimeManager.CreateInstance(s_Die);
        die.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        die.start();
    }

    public void Revive()
    {
        music.Revived();

        die.release();
        FMODUnity.RuntimeManager.PlayOneShot(s_Revive, transform.position);
    }

    public void UseMedkit()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_UseMedkit, transform.position);
    }
}