﻿using System.Collections;
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
            GameObject.Find("GameTimer").GetComponent<MatchTimer>().PuppetEscaped();
            Debug.Log("Interacting when door open.");
			RpcTurnOff(interactor);
			Destroy(interactor);
		}
	}

	public override void OnInteractEnd(GameObject interactor)
	{
		Debug.Log("End");
	}

	public override void OnRaycastEnter(GameObject interactor)
	{
		var button = GameObject.Find("FinalButton(Clone)");
		var buttonScript = button.GetComponent<FinalRoomInteract>();
		if(buttonScript.Opened)
			ShowTooltip(interactor);
	}
	public override void OnRaycastExit(GameObject interactor)
	{
		HideToolTip(interactor);
	}

	[ClientRpc]
	public void RpcTurnOff(GameObject interactor)
	{
		if (interactor.GetComponent<InteractionController>().isLocalPlayer)
		{
			var canvas = GameObject.FindObjectOfType<Spectator>().gameObject;
			canvas.GetComponent<Spectator>().SpectatorScreen.SetActive(true);
			canvas.GetComponent<Spectator>().StartSpectating();
		}
	}
}
