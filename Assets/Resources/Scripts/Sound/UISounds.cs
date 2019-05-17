using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.UI;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is used for playing sounds when navigating the UI, and handles volume
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

    // FMOD Buses
    public string masterBusString = "bus:/Master";
    FMOD.Studio.Bus masterBus;
    public string sfxBusString = "bus:/Master/SFX";
    FMOD.Studio.Bus sfxBus;
    public string musicBusString = "bus:/Master/Music";
    FMOD.Studio.Bus musicBus;

    // UI Sliders
    public Slider master;
    public Slider sfx;
    public Slider music;

    
    // Setup for the FMOD buses for volume changing
    private void Start()
    {
        masterBus = FMODUnity.RuntimeManager.GetBus(masterBusString);
        sfxBus = FMODUnity.RuntimeManager.GetBus(sfxBusString);
        musicBus = FMODUnity.RuntimeManager.GetBus(musicBusString);
        
        master.onValueChanged.AddListener(delegate {UpdateMasterVolume(); });
        sfx.onValueChanged.AddListener(delegate {UpdateSFXVolume(); });
        music.onValueChanged.AddListener(delegate {UpdateMusicVolume(); });

        if (PlayerPrefs.HasKey("MasterVolume"))
            master.value = PlayerPrefs.GetFloat("MasterVolume");

        if (PlayerPrefs.HasKey("SfxVolume"))
            sfx.value = PlayerPrefs.GetFloat("SfxVolume");

        if (PlayerPrefs.HasKey("MusicVolume"))
            music.value = PlayerPrefs.GetFloat("MusicVolume");


    }

    // Update volume according to the sliders in the main menu
    public void UpdateMasterVolume()
    {
        masterBus.setVolume(master.value);
        PlayerPrefs.SetFloat("MasterVolume", master.value);
    }

    public void UpdateSFXVolume()
    {
        sfxBus.setVolume(sfx.value);
        PlayerPrefs.SetFloat("SfxVolume", sfx.value);
    }

    public void UpdateMusicVolume()
    {
        musicBus.setVolume(music.value);
        PlayerPrefs.SetFloat("MusicVolume", music.value);
    }

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
