using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
	[FMODUnity.EventRef]
	public string GunShot;
	[FMODUnity.EventRef]
	public string Reload;
	[FMODUnity.EventRef]
	public string EmptyClip;
	[FMODUnity.EventRef]
	public string Cock;
	[FMODUnity.EventRef]
	public string Silent;
	FMOD.Studio.EventInstance GunShotEvent;
	FMOD.Studio.EventInstance ReloadEvent;
	FMOD.Studio.EventInstance EmptyClipEvent;
	FMOD.Studio.EventInstance CockEvent;
	FMOD.Studio.EventInstance SilentEvent;

    // Start is called before the first frame update
    void Start()
    {
		//GunShotEvent = FMODUnity.RuntimeManager.CreateInstance(GunShot);
		//ReloadEvent = FMODUnity.RuntimeManager.CreateInstance(Reload);
		//EmptyClipEvent = FMODUnity.RuntimeManager.CreateInstance(EmptyClip);
		//CockEvent = FMODUnity.RuntimeManager.CreateInstance(Cock);
		//SilentEvent = FMODUnity.RuntimeManager.CreateInstance(Silent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

		//FMODUnity.RuntimeManager.AttachInstanceToGameObject(GunShotEvent, GetComponent<Transform>(), GetComponent<Rigidbody>());
	public void PlaySoundGunShot()
	{
		FMOD.Studio.PLAYBACK_STATE PlaybackState;
		GunShotEvent.getPlaybackState(out PlaybackState);
		if (PlaybackState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
		{
			GunShotEvent.start();
		}
	}
	public void PlaySoundEmptyClip()
	{
		EmptyClipEvent.start();
	}
	public void PlaySoundReload()
	{
		ReloadEvent.start();
	}
	public void PlaySoundCock()
	{
		CockEvent.start();
	}
	public void PlaySoundSilent()
	{
		SilentEvent.start();
	}



}
