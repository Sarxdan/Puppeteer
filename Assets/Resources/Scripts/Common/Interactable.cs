using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * The script may be inherited by scripts for items, weapons etc. in order for the player to interact with them in the level.
 * 
 * CODE REVIEWED BY:
 * Ludvig Björk Förare
 * 
 */

[RequireComponent(typeof(Glowable))]
public abstract class Interactable : MonoBehaviour
{
    //Called once when an interaction has started
    public abstract void OnInteractBegin(GameObject interactor);
    //Called once when an interaction has ended
    public abstract void OnInteractEnd(GameObject interactor);

    public void OnRaycastEnter()
    {
        // enable outline effect
        GetComponent<Glowable>().Toggle(true);
    }

    public void OnRaycastExit()
    {
        // disable outline effect
        GetComponent<Glowable>().Toggle(false);
    }
}
