using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the following objects prefabs for sound:
 * Ammo
 * Medkit
 * Power-up Boost
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class ItemSounds : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Pickup;

    public void PickUp()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Pickup, transform.position);
    }
}
