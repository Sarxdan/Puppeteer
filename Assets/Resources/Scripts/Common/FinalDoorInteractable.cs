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
* Kristoffer Lundgren
*/

public class FinalDoorInteractable : Interactable
{
    public Music music;

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
	// (KL) Used to show the interact tooltip
	public override void OnRaycastEnter(GameObject interactor)
	{
		var button = GameObject.Find("FinalButton(Clone)");
		var buttonScript = button.GetComponent<FinalRoomInteract>();
		if(buttonScript.Opened)
			ShowTooltip(interactor);
	}


	[ClientRpc]
	public void RpcTurnOff(GameObject interactor)
	{
		if (interactor.GetComponent<InteractionController>().isLocalPlayer)
		{
            music.Escaped();
            var canvas = GameObject.FindObjectOfType<Spectator>().gameObject;
			canvas.GetComponent<Spectator>().SpectatorScreen.SetActive(true);
			canvas.GetComponent<Spectator>().StartSpectating();
		}
	}
}
