using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund, Anton "Knugen" Jonsson
*
* DESCRIPTION:
* Component used for getting position of roomColliders when building level.
*
* CODE REVIEWED BY:
* Sandra "Sanders" Andersson (16/4)
*
* CONTRIBUTORS:
*/

public class RoomCollider : MonoBehaviour
{
	// returns the Position of the AnchorPoint as a Vector of rounded Ints to avoid Unity float errors.
	public Vector3 GetPosition()
	{
		return new Vector3(Mathf.RoundToInt(transform.position.x * 1000) / 1000.0f, Mathf.RoundToInt(transform.position.y * 1000) / 1000.0f, Mathf.RoundToInt(transform.position.z * 1000) / 1000.0f);
	}
}
