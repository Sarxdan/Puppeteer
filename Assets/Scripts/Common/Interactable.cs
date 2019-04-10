using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Interface for all interactable in-level objects, such as doors, pickups and players.
 * This component also internally handles object highlighting using the raycast enter and exit functions.
 * In order customize the outline of the highlighted object, a custom line color and width may be provided.
 * 
 * CODE REVIEWED BY:
 * 
 */
public abstract class Interactable : MonoBehaviour
{
    // the width of the outline
    [Range(0.0f, 10.0f)]
    public float OutlineWidth = 1.0f;

    // the color of the outline
    public Color OutlineColor = Color.white;

    // performs player interaction logic on this object
    public abstract void OnInteract(in GameObject interactor);

    public void OnRaycastEnter()
    {
        // TODO: enable outline of object
        // will probably require a outline shader with a toggable property
        // that also uses the provided outline color and width
    }

    public void OnRaycastExit()
    {
        // TODO: disable outline of object
    }
}
