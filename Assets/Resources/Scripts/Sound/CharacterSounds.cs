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
        FMODUnity.RuntimeManager.PlayOneShot(s_Die, transform.position);
    }
}
