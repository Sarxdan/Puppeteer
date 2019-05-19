using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Music;
    FMOD.Studio.EventInstance music;

    public void StartMatch()
    {
        music = FMODUnity.RuntimeManager.CreateInstance(s_Music);
        music.start();
    }

    public void Progression()
    {
        music.setParameterByName("Progression", 1);
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
