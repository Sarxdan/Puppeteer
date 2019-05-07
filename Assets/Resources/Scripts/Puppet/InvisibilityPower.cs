using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * When activated, the owner will become invisible for all observers except other players.
 * 
 * CODE REVIEWED BY:
 * 
 */
public class InvisibilityPower : PowerupBase
{
    public GameObject MeshContainer;


    public override void OnActivate()
    {
        //Change rendering layers so the puppeteers camera ignores the mask
        setLayer(1);
        //Change so AI ignores the player when they are looking for him
    }

    public override void OnComplete()
    {
        //Revert all changes when the powerup is done
        setLayer(9);
    }

    private void setLayer(int layer)
    {
        gameObject.layer = layer;
        foreach (Transform item in MeshContainer.transform)
        {
            item.gameObject.layer = layer;
        }
    }
}
