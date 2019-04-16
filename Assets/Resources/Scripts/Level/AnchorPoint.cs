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
* OtherName McOtherNameson
*
* CONTRIBUTORS:
*/

public class AnchorPoint : MonoBehaviour
{
	// bool to see if AnchorPoint is connected to another AnchorPoint.
	public bool Connected = false;

    // Update rotation of doors to point away from the room plane.
    void Awake()
    {
		transform.LookAt(transform.parent);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);
    }

    // Draw position and diretion of doors
    void Update()
    {
		//Debug.DrawLine(transform.position, transform.position + transform.up * 2, Color.yellow);

		if (Connected)
		{
			//Debug.DrawLine(transform.position, transform.position + transform.forward * 2, Color.blue);
		}
		
    }

	// returns the Position of the AnchorPoint as a Vector of rounded Ints to avoid Unity float errors.
	public Vector3Int GetPosition()
	{
		return new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
	}
}
