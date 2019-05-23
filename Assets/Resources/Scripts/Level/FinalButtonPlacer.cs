using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund
*
* DESCRIPTION:
* Component used to determine where Final door is and in which directions it is pointed.
*
* CODE REVIEWED BY:
*
* CONTRIBUTORS:
* 
* 
* CLEANED
*/

public class FinalButtonPlacer : NetworkBehaviour
{
	// Needs to be spawnable prefab.
	public GameObject Button;
	public GameObject Door;

	public GameObject DoorNode;
	public GameObject ButtonNode;
}
