using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the players for playing and changing mood of music
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class Music : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Music;
    FMOD.Studio.EventInstance music;

    public void Start()
    {
        music = FMODUnity.RuntimeManager.CreateInstance(s_Music);
        music.start();
        FindObjectOfType<MatchTimer>().music = this.music;
    }
    
    public void Downed()
    {
        music.setParameterByName("Downed", 1);
    }

    public void Revived()
    {
        music.setParameterByName("Downed", 0);
    }

    public void ButtonPressed()
    {
        music.setParameterByName("Progression", 2);
    }

    public void Escaped()
    {
        music.setParameterByName("Progression", 3);
    }

    public void EndMatch()
    {
        music.release();
    }

}
