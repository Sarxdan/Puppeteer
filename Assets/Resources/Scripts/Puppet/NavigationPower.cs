using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Displays the goal on the compass display when activated.
 * Requires that one object in the scene has the 'Finish' tag.
 * 
 * CODE REVIEWED BY:
 * 
 */
public class NavigationPower : PowerupBase
{
    public override void OnActivate()
    {
        GameObject.FindObjectOfType<Compass>().AddTarget(GameObject.FindGameObjectWithTag("Finish").transform);
    }

    public override void OnComplete()
    {
        GameObject.FindObjectOfType<Compass>().RemoveTarget(GameObject.FindGameObjectWithTag("Finish").transform);
    }
}