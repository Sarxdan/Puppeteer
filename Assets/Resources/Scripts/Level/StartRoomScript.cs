using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Anton Jonsson, Filip Renman
*
* DESCRIPTION:
* Script to automatically rename the startroom to make it easy to find.
*
* CODE REVIEWED BY:
* Ludvig Björk Förare (190430)
*
* CONTRIBUTORS:
*/

public class StartRoomScript : MonoBehaviour
{
    void Start()
    {
		gameObject.name = "startRoom";
    }

}
