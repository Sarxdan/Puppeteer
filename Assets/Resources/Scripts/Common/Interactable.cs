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
 * 
 */

public abstract class Interactable : MonoBehaviour
{
    private Material outlineMat;
    private void Start()
    {
        var materials = GetComponent<Renderer>().materials;
        foreach(var mat in materials)
        {
            if(mat.FindPass("Outline") != -1)
            {
                outlineMat = mat;
            }
        }

        if(outlineMat == null)
        {
            Debug.LogError("Interactable objects requires an outline material");
        }
    }

    //Called once when an interaction has started
    public abstract void OnInteractBegin(GameObject interactor);
    //Called once when an interaction has ended
    public abstract void OnInteractEnd(GameObject interactor);

    public void OnRaycastEnter()
    {
        if (outlineMat != null)
        {
            // enable outline
            outlineMat.SetFloat("_EnableOutline", 1);
        }
    }

    public void OnRaycastExit()
    {
        if (outlineMat != null)
        {
            // disable outline
            outlineMat.SetFloat("_EnableOutline", 0);
        }
    }
}
