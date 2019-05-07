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
	public GameObject DoorToOpen;

	public override void OnInteractBegin(GameObject interactor)
	{
		StartCoroutine("FinalCountDown");
	}

	public override void OnInteractEnd(GameObject interactor)
	{
		throw new System.NotImplementedException();
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
				// DoorToOpen.transform.Rotate(Mathf.Lerp);
			}
			yield return new WaitForSeconds(1);
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
