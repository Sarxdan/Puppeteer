using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class AnchorPoint : MonoBehaviour
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
	public Vector3Int GetPosition()
	{
		return new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
	}

	public void ConnectDoor(AnchorPoint to)
	{
		if (to.GetPosition() == GetPosition())
		{
			to.Connected = true;
			to.ConnectedTo = this;
			Connected = true;
			ConnectedTo = to;
		}
	}
	public void DisconnectDoor()
	{
		if (Connected)
		{
			ConnectedTo.Connected = false;
			ConnectedTo.ConnectedTo = null;
			Connected = false;
			ConnectedTo = null;
		}
	}
	public void ReConnectTo(AnchorPoint to)
	{
		DisconnectDoor();
		ConnectDoor(to);
	}

}
