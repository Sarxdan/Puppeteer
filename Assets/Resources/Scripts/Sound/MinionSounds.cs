using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the following objects prefabs for sound:
 * Minion
 * Tanks
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class MinionSounds : CharacterSounds
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Punch;
    [FMODUnity.EventRef] public string s_Thud;
    [FMODUnity.EventRef] public string s_Spawn;
    [FMODUnity.EventRef] public string s_Idle;

    public void Punch()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Punch, transform.position);
    }

    public void DeathThud()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Thud, transform.position);
    }
    
    public void Spawn()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Spawn, transform.position);
    }

    public void Idle()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Idle, transform.position);
    }
}
