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
* OtherName McOtherNameson
*
* CONTRIBUTORS:
*/

public class RoomCollider : MonoBehaviour
{
	// returns the Position of the AnchorPoint as a Vector of rounded Ints to avoid Unity float errors.
	public Vector3Int GetPosition()
	{
		return new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
	}
}
