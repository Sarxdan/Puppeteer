using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Base class for all powerups. 
 * A powerup is started using the Run coroutine, which in turn manages the activate and complete functions
 * 
 * CODE REVIEWED BY:
 * Benjamin Vesterlund
 *
 * CONTRIBUTORS:
 * Sandra Andersson (Sound)
 * 
 * 
 * CLEANED
 */
public abstract class PowerupBase : NetworkBehaviour
{
    //The duration of the powerup
    public int Duration;
    //Checks if the power is charged and ready to use
    public bool Charged;
    //Script for playing sounds
    public PowerUpSounds sounds;

    private float timeLeft;

    void Start()
    {
        sounds = GetComponent<PowerUpSounds>();
    }

    //Get percentage left of powerup
    public float PercentageLeft
    {
        get
        {
            return Charged ? 1.0f : (timeLeft == 0.0f ? 0.0f : (1.0f - timeLeft / Duration));
        }
    }


    //Attempts to start and consume the powerup
    public IEnumerator Run()
    {
        if (!Charged)
        {
            //Unable to activate powerup
            yield break;
        }

        timeLeft = 0.0f;
        Charged = false;

        //Activate power
        this.OnActivate();
        sounds.PowerUpStart();

        while(timeLeft < Duration)
        {
            timeLeft += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Deactivate power
        sounds.PowerUpEnd();
        timeLeft = Duration;
        this.OnComplete();
    }
    
    //Called once when the powerup is activated
    public abstract void OnActivate();

    //Called once when the powerup is completed
    public abstract void OnComplete();

    [ClientRpc]
    public void RpcBoostPowerup()
    {
        if (isLocalPlayer && PercentageLeft == 0.0f)
        {
            Charged = true;
        }
    }
}
