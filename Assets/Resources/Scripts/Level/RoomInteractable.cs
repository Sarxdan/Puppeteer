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
}
