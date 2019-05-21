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
    [FMODUnity.EventRef] public string s_Reviving;
    FMOD.Studio.EventInstance ress;

    public Music music;

    void Start()
    {
        music = GetComponent<Music>();
    }
    
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

    public void ReviveStart()
    {
        ress = FMODUnity.RuntimeManager.CreateInstance(s_Reviving);
        ress.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        ress.start();
    }
    
    public void Revive()
    {
        music.Revived();

        ress.setParameterByName("Ress", 1);
        ress.release();
        die.release();
    }

    public void ReviveEnd()
    {
        ress.release();
        ress.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}