using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Anton Jonsson, Filip Renman
*
* DESCRIPTION:
* Struct used to send information about interacted gameobjects over network
*
* CODE REVIEWED BY:
* 
*
* CONTRIBUTORS:
*/

public struct InteractStruct
{
	public GameObject Source;
	public GameObject Target;

	public InteractStruct(GameObject source, GameObject target)
	{
		Source = source;
		Target = target;
	}
}
