using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using MinionStates;

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
* 
* CLEANED
*/

public class FinalRoomInteract : Interactable
{
	[SyncVar]
	public int TimeLeft;
	[SyncVar]
	public float width;

	public float RotationSpeed;
	public GameObject DoorToOpen;
	private GameObject endGameDisplay;
	public RectTransform ProgressTransform;
	public Transform LeverTransform;
	public int LeverFrames;

	private float openAngle;
	[SyncVar]
	public bool Opened = false;
    [SyncVar]
    public bool ButtonPressed = false;

	public static bool isEndGame;

	private void Start()
	{
		endGameDisplay = GameObject.Find("EndGameDisplay");
		endGameDisplay.SetActive(false);
	}

	// Only runs on server. (by default)
	public override void OnInteractBegin(GameObject interactor)
	{
        if (!ButtonPressed)
        {
			endGameDisplay.SetActive(true);
			RpcPlayMusic();
		    DoorToOpen = GameObject.Find("ST_Final_DoorFrame_03");
            ProgressTransform = GameObject.Find("ProgressBar").GetComponent<RectTransform>();
			LeverTransform = GameObject.Find("ST_lever_low").GetComponent<Transform>();
		    StartCoroutine("FinalCountDown");
			StartCoroutine("LeverDown");
            ButtonPressed = true;
        }
	}

	public override void OnInteractEnd(GameObject interactor)
	{

	}
	// Used to show the interact tooltip
	public override void OnRaycastEnter(GameObject interactor)
	{
		if(!ButtonPressed)
			ShowTooltip(interactor);
	}

	IEnumerator LeverDown()
	{
		var z = LeverTransform.localEulerAngles.z;
		var end_z = 35;

		var diff = (z - end_z) / LeverFrames;
		while (LeverTransform.localEulerAngles.z <= 359)
		{
			LeverTransform.localEulerAngles = new Vector3(
				LeverTransform.localEulerAngles.x,
				LeverTransform.localEulerAngles.y,
				LeverTransform.localEulerAngles.z + diff);
			RpcMoveLever(LeverTransform.rotation);
			yield return new WaitForEndOfFrame();
		}

		LeverTransform.localEulerAngles = new Vector3(
				LeverTransform.localEulerAngles.x,
				LeverTransform.localEulerAngles.y,
				0);
		RpcMoveLever(LeverTransform.rotation);
		while (LeverTransform.localEulerAngles.z <= end_z)
		{
			LeverTransform.localEulerAngles = new Vector3(
				LeverTransform.localEulerAngles.x,
				LeverTransform.localEulerAngles.y,
				LeverTransform.localEulerAngles.z + diff);
			RpcMoveLever(LeverTransform.rotation);
			yield return new WaitForEndOfFrame();
		}
	}

	// Only runs on server.
	IEnumerator FinalCountDown()
	{
		isEndGame = true;
		EnemySpawner dummySpawner = GameObject.Find("DummySpawner").GetComponent<EnemySpawner>();
		foreach(StateMachine minion in EnemySpawner.AllMinions)
		{
			minion.Spawner = dummySpawner;
			if(minion.TargetEntity == null)
			{
				minion.SetState(new ReturnToSpawnerState(minion));
			}
		}
		var diffInWidth = ((ProgressTransform.sizeDelta.x - 160) / TimeLeft);
		while(true)
		{
			if (TimeLeft == 0)
			{
				StartCoroutine("OpenDoor");
				Opened = true;
				break;
			}
			TimeLeft--;
			RpcShowProgress(new Vector2(ProgressTransform.sizeDelta.x - diffInWidth, ProgressTransform.sizeDelta.y));

			yield return new WaitForSeconds(1);
		}
	}

    [ClientRpc]
    public void RpcPlayMusic()
    {
        FindObjectOfType<Music>().ButtonPressed();
    }

	[ClientRpc]
	public void RpcMoveLever(Quaternion rotation)
	{
		if (isServer)
			return;
		if (LeverTransform == null)
			LeverTransform = GameObject.Find("ST_lever_low").GetComponent<Transform>();
		LeverTransform.rotation = rotation;
	}

	[ClientRpc]
	public void RpcShowProgress(Vector2 size)
	{
		if (!endGameDisplay.activeInHierarchy)
			endGameDisplay.SetActive(true);
		if (ProgressTransform == null)
			ProgressTransform = GameObject.Find("ProgressBar").GetComponent<RectTransform>();

		ProgressTransform.sizeDelta = size;
	}

	// Only run on server.
	IEnumerator OpenDoor()
	{
		if (DoorToOpen == null)
		{
			DoorToOpen = GameObject.Find("ST_Final_Door_01 (1)");
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
