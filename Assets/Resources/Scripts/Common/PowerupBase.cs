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
 */
public abstract class PowerupBase : MonoBehaviour
{
    // the duration of the powerup
    public float Duration;
    public bool Charged;

    public IEnumerator Run()
    {
        // unable to activate powerup
        if (!Charged)
        {
            yield break;
        }

        float time = 0.0f;
        Charged = false;

        // activate power
        this.OnActivate();

        while(++time < Duration)
        {
            yield return new WaitForSeconds(1.0f);
        }

        // finish power
        this.OnComplete();
    }
    
    // called once when the powerup is activated
    public abstract void OnActivate();

    // called once when the powerup is completed
    public abstract void OnComplete();
}
