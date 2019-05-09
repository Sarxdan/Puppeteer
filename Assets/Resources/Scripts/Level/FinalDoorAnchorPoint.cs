using System.Collections;
using System.Collections.Generic;
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
*/

public class FinalDoorAnchorPoint : MonoBehaviour
{
	// Needs to be spawnable prefab.
	public GameObject FinalDoor;
	public GameObject DoorToOpen;

	void Awake()
	{
		transform.LookAt(transform.parent);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
