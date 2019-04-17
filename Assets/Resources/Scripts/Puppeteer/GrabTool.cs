using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund, Anton "Knugen" Jonsson
*
* DESCRIPTION:
* Tool used for grabbing and dropping rooms as puppeteer
*
* CODE REVIEWED BY:
* 
*
* CONTRIBUTORS:
* Philip Stenmark
*/

public class GrabTool : MonoBehaviour
{
	private GameObject level;

	private GameObject sourceObject;
	private GameObject selectedObject;
	private GameObject guideObject;

	private RoomInteractable lastHit;

    void Start()
    {
		level = GameObject.Find("Level");
    }

    void Update()
    {
		if (selectedObject == null)
		{
			RaycastHit hit;
			// TODO: Add raycast limit?
			int layerMask = 1 << LayerMask.NameToLayer("Puppeteer Interact");
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Collide))
			{
				GameObject hitObject = hit.transform.gameObject;

				// Start and stop glow
				RoomInteractable interactable = hitObject.GetComponent<RoomInteractable>();
				if (interactable != null)
				{
					if (interactable != lastHit)
					{
						lastHit.OnRaycastExit();
						lastHit = interactable;
						if (lastHit.CanBePickedUp)
						{
							lastHit.OnRaycastEnter();
						}
					}
					// If pickup button is pressed, call pickup method.
					if (Input.GetButtonDown("Fire"))
					{
						Pickup(hitObject);
					}
				}
				else
				{
					// If raycast doesn't hit a valid object
					lastHit.OnRaycastExit();
					lastHit = null;
				}
			}
			else
			{
				// If raycast doesn't hit any objects
				lastHit.OnRaycastExit();
				lastHit = null;
			}
		}
		else
		{
			if (Input.GetButtonUp("Fire"))
			{
				Drop();
			}
			else if (Input.GetButtonDown("Aim"))
			{
				selectedObject.transform.Rotate(new Vector3(0, 90, 0));
			}
		}
    }

	private void Pickup(GameObject pickupObject)
	{
		sourceObject = pickupObject;

		selectedObject = Instantiate(sourceObject);
		selectedObject.name = "SelectedObject";

		guideObject = Instantiate(sourceObject);
		guideObject.name = "GuideObject";

		foreach (AnchorPoint door in guideObject.GetComponentsInChildren<AnchorPoint>())
		{
			Destroy(door.gameObject);
		}
	}

	private void Drop()
	{
		// selectedObject = null
	}
}


