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

public class UISounds : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Click;
    [FMODUnity.EventRef] public string s_Hover;
    [FMODUnity.EventRef] public string s_Forward;
    [FMODUnity.EventRef] public string s_Backward;

    public void Click()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Click, transform.position);
    }

    public void Hover()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Hover, transform.position);
    }

    public void Forward()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Forward, transform.position);
    }

    public void Backward()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Backward, transform.position);
    }

}
