﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/*
* AUTHOR:
* Kristoffer Lundgren
*
* DESCRIPTION:
* This class is placed on the parent of a door with a collider attached, the door then be opened with the use key
*
* CODE REVIEWED BY:
* Philip Stenmark
*
* CONTRIBUTORS:
* Anton Jonsson
* Ludvig Björk Förare
* 
* 
* CLEANED
*/

public class DoorComponent : Interactable
{
	[SerializeField]
	[SyncVar(hook = nameof(LockingDoor))]

    private bool locked;

    public PuppetRoomSounds sounds;
    
    public bool Locked
    {
        get
        {
            return locked;
        }
        set
        {
			locked = value;
			if(locked)
			{
                transform.GetChild(0).gameObject.SetActive(false);
				transform.GetChild(1).gameObject.SetActive(true);

				transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, defaultAngle, transform.localEulerAngles.z);
			}
			else
			{
				transform.GetChild(0).gameObject.SetActive(true);
				transform.GetChild(1).gameObject.SetActive(false);
			}
		}
    }

	public void LockingDoor(bool lockstatus)
	{
		if (lockstatus)
		{
			transform.GetChild(0).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(true);
		}
		else
		{
			transform.GetChild(0).gameObject.SetActive(true);
			transform.GetChild(1).gameObject.SetActive(false);
		}
	}

	public GameObject Icon;
    
    public float RotationSpeed;
    // How much the door should open
    public float OpenAngle = 90;
	public float defaultAngle = 0;
	// Vector used for offsetting door position into doorframe correctly.
	public Vector3 adjustmentVector = new Vector3(0.6f, 0, 0);

	private float currentAngle = 0;

	public bool Open = false;

	public float closeTime;
	
	private IEnumerator closeRoutineInstance;

    // Save the start angle of the door
    void Start()
    {
		currentAngle = defaultAngle;
        sounds = GetComponent<PuppetRoomSounds>();
		Icon.transform.up = Vector3.forward;
    }

    // When the used key is pressed the direction the door should open is calculated
    public override void OnInteractBegin(GameObject interactor)
    {
		if (!locked)
        {
			RpcPlaySoundOpen();
			float dotProduct = Vector3.Dot(transform.forward, interactor.transform.forward);
			currentAngle = OpenAngle * Mathf.Sign(dotProduct);
			Open = !Open;
			if(Open)
			{
				closeRoutineInstance = AutoClose();
				StartCoroutine(closeRoutineInstance);
			}
			else
			{
				StopCoroutine(closeRoutineInstance);
			}
		}
        else
        {
			RpcPlaySoundLocked();
        }
	}

    public override void OnInteractEnd(GameObject interactor){}
	// Used to show the interact tooltip
	public override void OnRaycastEnter(GameObject interactor)
	{
		ShowTooltip(interactor);
	}

	[ClientRpc]
	public void RpcPlaySoundOpen()
	{
		sounds.Open();
	}

	[ClientRpc]
	public void RpcPlaySoundLocked()
	{
		sounds.Locked();
	}

    // Closes and opens the door
    void FixedUpdate()
    {	
        if(!locked)
        {
            if(Open)
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(transform.localEulerAngles.x , defaultAngle + currentAngle, transform.localEulerAngles.z), RotationSpeed);
            }
            else
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(transform.localEulerAngles.x , defaultAngle, transform.localEulerAngles.z), RotationSpeed);
            }
        }
    }

	private IEnumerator AutoClose()
	{
		yield return new WaitForSeconds(closeTime);
		Open = false;
        sounds.Close();
	}
}
