using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund
*
* DESCRIPTION:
* Point holding info for placing traps.
*
* CODE REVIEWED BY:
*
* CONTRIBUTORS:
* 
*/

public class FinalDoorInteractable : Interactable
{
	// only runs on server.
	public override void OnInteractBegin(GameObject interactor)
	{
		Debug.Log("Interact with Door.");
		var button = GameObject.Find("FinalButton(Clone)");
		var buttonScript = button.GetComponent<FinalRoomInteract>();
		if (buttonScript.Opened)
		{
			Debug.Log("Interacting when door open.");
			RpcTurnOff(interactor);
		}
	}

	public override void OnInteractEnd(GameObject interactor)
	{
		Debug.Log("End");
	}

	[ClientRpc]
	public void RpcTurnOff(GameObject interactor)
	{
		Destroy(interactor);
		var playerList = GameObject.FindGameObjectsWithTag("Player");
		var camera = playerList[0].GetComponentInChildren<Camera>();
		camera.enabled = true;
	}
}
