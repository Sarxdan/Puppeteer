using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * DESCRIPTION:
 * A container class for holding references to the doors in a room
 * (Basically saves getComponentsInChildren as cache in the prefab)
 *
 * CODE REVIEWED BY:
 * 
 */

public class DoorReferences : MonoBehaviour
{
    public AnchorPoint[] doors;

    public void GetDoors(){
        doors = transform.GetComponentsInChildren<AnchorPoint>();
    }
}
