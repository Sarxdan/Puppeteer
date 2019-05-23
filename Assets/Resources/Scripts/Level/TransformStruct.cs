using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Anton Jonsson
*
* DESCRIPTION:
* Struct used to send position and rotation of an object over network
*
* CODE REVIEWED BY:
* Ludvig Björk Förare (190430)
*
* CONTRIBUTORS:
* 
* 
* CLEANED
*/

public struct TransformStruct
{
	public Vector3 Position;
	public Quaternion Rotation;

	public TransformStruct(Vector3 pos, Quaternion rot)
	{
		Position = pos;
		Rotation = rot;
	}
}
