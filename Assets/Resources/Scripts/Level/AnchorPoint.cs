using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund, Anton "Knugen" Jonsson
*
* DESCRIPTION:
* Component used to determine where doors are and in which directions they are pointed.
*
* CODE REVIEWED BY:
* Sandra "Sanders" Andersson (16/4)
*
* CONTRIBUTORS:
*/

public class AnchorPoint : NetworkBehaviour
{
	// bool to see if AnchorPoint is connected to another AnchorPoint.
	public bool Connected = false;
	public AnchorPoint ConnectedTo;

    // Update rotation of doors to point away from the room plane.
    void Awake()
    {
		transform.LookAt(transform.parent);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);
    }

    // Draw position and diretion of doors
    void Update()
    {

		//Code to show doors and door connections in scene editor
		if (Connected)
		{
			Debug.DrawLine(transform.position, transform.position + transform.forward * 2, Color.blue);
			Debug.DrawLine(transform.position, transform.position + transform.up * 2, Color.green);
		}
		else
		{
			Debug.DrawLine(transform.position, transform.position - transform.forward * 2, Color.black);
			Debug.DrawLine(transform.position, transform.position + transform.up * 2, Color.black);
		}

	}

	// Returns the Position of the AnchorPoint as a Vector of rounded Ints to avoid Unity float errors.
	public Vector3 GetPosition()
	{
		return new Vector3(Mathf.RoundToInt(transform.position.x * 1000) / 1000.0f, Mathf.RoundToInt(transform.position.y * 1000) / 1000.0f, Mathf.RoundToInt(transform.position.z * 1000) / 1000.0f);
	}

	// Connect two doors together
	public void ConnectDoor(AnchorPoint to)
	{
		if (to.GetPosition() == GetPosition())
		{
			DoorComponent thisDoor = GetComponentInChildren<DoorComponent>();
			DoorComponent toDoor = to.GetComponentInChildren<DoorComponent>();

			// Destory one of the doors to avoid two doors in the same doorframe.

			NetworkServer.Destroy(toDoor.gameObject);
			Destroy(toDoor.gameObject);

			thisDoor.Locked = false;

			to.Connected = true;
			to.ConnectedTo = this;
			Connected = true;
			ConnectedTo = to;
		}
	}

	// Disconnects a door from whatever it was connected with.
	public void DisconnectDoor()
	{
		if (Connected)
		{
			// If either door was previously removed, readd it and lock it.
			DoorComponent thisDoor = GetComponentInChildren<DoorComponent>();
			DoorComponent toDoor = ConnectedTo.GetComponentInChildren<DoorComponent>();

			if (thisDoor == null)
			{
				GameObject door = Instantiate(GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelBuilder>().Door, transform);
				NetworkServer.Spawn(door);
				thisDoor = door.GetComponent<DoorComponent>();
				door.transform.localEulerAngles = new Vector3(0, 0, 0);
				door.transform.position = transform.position + transform.rotation * thisDoor.adjustmentVector;
				thisDoor.defaultAngle = 0;
			}
			else if (toDoor == null)
			{
				GameObject door = Instantiate(GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelBuilder>().Door, ConnectedTo.transform);
				NetworkServer.Spawn(door);
				toDoor = door.GetComponent<DoorComponent>();
				door.transform.localEulerAngles = new Vector3(0, 0, 0);
				door.transform.position = ConnectedTo.transform.position + ConnectedTo.transform.rotation * toDoor.adjustmentVector;
				toDoor.defaultAngle = 0;
			}

			thisDoor.Locked = true;
			toDoor.Locked = true;

			ConnectedTo.Connected = false;
			ConnectedTo.ConnectedTo = null;
			Connected = false;
			ConnectedTo = null;
		}
	}

	// Reconnects a door to a new door.
	public void ReConnectTo(AnchorPoint to)
	{
		DisconnectDoor();
		ConnectDoor(to);
	}

}
