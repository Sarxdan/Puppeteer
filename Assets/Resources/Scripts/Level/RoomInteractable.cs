using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund, Anton "Knugen" Jonsson
*
* DESCRIPTION:
* Script to determine what rooms should do when interacted with.
*
* CODE REVIEWED BY:
* 
*
* CONTRIBUTORS:
*/

public class RoomInteractable : Interactable
{
	public bool CanBePickedUp = true;

	public override void OnInteractBegin(GameObject interactor)
	{
		
	}

	public override void OnInteractEnd(GameObject interactor)
	{

	}

	// Returns true if there is a player in the room
	public bool RoomContainsPlayer()
	{
		foreach (BoxCollider collider in GetComponents<BoxCollider>())
		{
			foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
			{
				// Check if any player is within any collider on the room.
				if (collider.bounds.Contains(player.transform.position))
				{
					return true;
				}
			}
		}
		return false;
	}
}
