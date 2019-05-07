using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund
*
* DESCRIPTION:
* Point holding info for placing traps.
*
* CODE REVIEWED BY:
*
* CONTRIBUTORS:
* 
*/

public class FinalRoomInteract : Interactable
{
	public int TimeLeft;
	public float LerpFactor;
	public GameObject DoorToOpen;

	public override void OnInteractBegin(GameObject interactor)
	{
		StartCoroutine("FinalCountDown");
	}

	public override void OnInteractEnd(GameObject interactor)
	{
		Debug.Log("Ended interact");
	}

	public IEnumerator FinalCountDown()
	{
		
		while(true)
		{
			if (TimeLeft > 0)
			{
				TimeLeft -= 1;
			}
			else
			{
				StartCoroutine("OpenDoor");
				break;
			}
			yield return new WaitForSeconds(1);
		}
	}

	public IEnumerator OpenDoor()
	{
		while(DoorToOpen.transform.rotation.y > 13.0f)
		{
			DoorToOpen.transform.rotation = Quaternion.Lerp(DoorToOpen.transform.rotation, new Quaternion(0, 13, 0, 0), Time.deltaTime * LerpFactor);

			yield return new WaitForFixedUpdate();
		}
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
