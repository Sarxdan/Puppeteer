using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSounds : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string PowerUp;
    FMOD.Studio.EventInstance power;

    public void PowerUpStart()
    {
        power = FMODUnity.RuntimeManager.CreateInstance(PowerUp);
        power.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        power.start();
    }

    public void PowerUpEnd()
    {
        power.release();
    }
}
