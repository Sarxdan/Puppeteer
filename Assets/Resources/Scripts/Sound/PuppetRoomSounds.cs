using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is used for sounds for when the puppet interacts with doors
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class PuppetRoomSounds : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Open;
    [FMODUnity.EventRef] public string s_Close;
    [FMODUnity.EventRef] public string s_Locked;
    
    public void Open()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Open, transform.position);
    }
    
    public void Close()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Close, transform.position);
    }

    public void Locked()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Locked, transform.position);
    }

}
