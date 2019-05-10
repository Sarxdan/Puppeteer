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

public class FinalRoomInteract : Interactable
{
	[SyncVar]
	public int TimeLeft;

	public float RotationSpeed;
	public GameObject DoorToOpen;

	private float openAngle;
	[SyncVar]
	public bool Opened = false;

	// Only runs on server. (by default)
	public override void OnInteractBegin(GameObject interactor)
	{
		DoorToOpen = GameObject.Find("ST_Final_DoorFrame_03");
		StartCoroutine("FinalCountDown");
	}

	public override void OnInteractEnd(GameObject interactor)
	{

	}

	// Only runs on server.
	IEnumerator FinalCountDown()
	{
		while(true)
		{
			if (TimeLeft == 0)
			{
				StartCoroutine("OpenDoor");
				Opened = true;
				break;
			}
			TimeLeft--;
			Debug.Log(TimeLeft);
			yield return new WaitForSeconds(1);
		}
	}

	// Only run on server.
	IEnumerator OpenDoor()
	{
		if (DoorToOpen == null)
		{
			DoorToOpen = GameObject.Find("ST_Final_DoorFrame_03");
		}
		openAngle = DoorToOpen.transform.eulerAngles.y + 13;
		while (true)
		{
			if (DoorToOpen.transform.eulerAngles.y == openAngle)
			{
				break;
			}
			DoorToOpen.transform.rotation = Quaternion.RotateTowards(
				DoorToOpen.transform.rotation,
				Quaternion.Euler(0, openAngle, 0), RotationSpeed);
			yield return new WaitForFixedUpdate();
		}
	}
}
