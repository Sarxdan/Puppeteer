using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the following objects prefabs for sound:
 * Spikes
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class TrapSounds : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Activate;

    public void Activate()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Activate, transform.position);
    }
}
