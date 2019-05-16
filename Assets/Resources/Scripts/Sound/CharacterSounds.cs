using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is used for inheritance for scripts with sound for puppets and minions
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class CharacterSounds : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Hit; // Played from HealthComponent
    [FMODUnity.EventRef] public string s_Die; // Played from animation
    public FMOD.Studio.EventInstance die;
    [FMODUnity.EventRef] public string s_Footstep;
    [FMODUnity.EventRef] public string s_RunFootstep;


    public void Step()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Footstep, transform.position);
    }
    public void RunStep()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_RunFootstep, transform.position);
    }

    public void Damage()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Hit, transform.position);
    }

    public void Death()
    {
        die = FMODUnity.RuntimeManager.CreateInstance(s_Die);
        die.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        die.start();
    }
}
