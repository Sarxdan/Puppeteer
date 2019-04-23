using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
 */
public abstract class PowerupBase : MonoBehaviour
{
    // the duration of the powerup
    public int Duration;
    // checks if the power is charged and ready to use
    public bool Charged;

    // get percentage left of powerup
    public float PercentageLeft
    {
        get
        {
            return Charged ? 1.0f : 1.0f - ((float)timeLeft / (float)Duration);
        }
    }

    private int timeLeft;

    // attempts to start and consume the powerup
    public IEnumerator Run()
    {
        if (!Charged)
        {
            // unable to activate powerup
            yield break;
        }

        timeLeft = 0;
        Charged = false;

        // activate power
        this.OnActivate();

        while(++timeLeft < Duration)
        {
            yield return new WaitForSeconds(1);
        }

        // deactivate power
        this.OnComplete();
    }
    
    // called once when the powerup is activated
    public abstract void OnActivate();

    // called once when the powerup is completed
    public abstract void OnComplete();
}
