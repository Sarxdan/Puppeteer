using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Anton Jonsson
*
* DESCRIPTION:
* Script used for returning game objects to the startRoom when they have fallen off the level.
*
* CODE REVIEWED BY:
* Sandra "Sanders" Andersson (02/05-2019)
*/

public class LevelReturn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter(Collider other)
	{
		other.gameObject.transform.position = new Vector3(0, 1.0f, 0);
	}
}