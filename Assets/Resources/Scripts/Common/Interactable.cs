﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * The script may be inherited by scripts for items, weapons etc. in order for the player to interact with them in the level.
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * 
 */

public abstract class Interactable : MonoBehaviour
{
    //Called once when an interaction has started
    public abstract void OnInteractBegin(GameObject interactor);
    //Called once when an interaction has ended
    public abstract void OnInteractEnd(GameObject interactor);

    public void OnRaycastEnter()
    {
        // TODO: enable outline of object
    }

    public void OnRaycastExit()
    {
        // TODO: disable outline of object
    }
}
