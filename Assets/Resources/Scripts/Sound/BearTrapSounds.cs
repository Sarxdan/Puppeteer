using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the following objects prefabs for sound:
 * Bear Trap
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class BearTrapSounds : TrapSounds
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Release;
    FMOD.Studio.EventInstance open;

    //Called from Bearinteract
    public void Release()
    {
        open = FMODUnity.RuntimeManager.CreateInstance(s_Release);
        open.setParameterByName("Stop", 0f);
        open.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        open.start();
    }

    //Called from BearInteract for playing closing sound, after cancelling interaction for release
    public void ReClose()
    {
        open.setParameterByName("Stop", 1f);
        open.release();
    }
}
