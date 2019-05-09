using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is used for when the puppeteer manipulates the rooms
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class PuppeteerRoomSounds : PuppeteerItemSounds
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Rotate;
    
    public void Rotate()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Rotate, transform.position);
    }
}
