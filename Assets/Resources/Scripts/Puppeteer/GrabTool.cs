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
	private Vector3 grabOffset = new Vector3();

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
						if (lastHit != null)
						{
							lastHit.OnRaycastExit();
						}
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
			else
			{
				if (Input.GetButtonDown("Rotate"))
				{
					selectedObject.transform.RotateAround(new Vector3(0,0,0), selectedObject.transform.up, 90);
					//selectedObject.transform.position = MouseToWorldPosition();
				}
				UpdatePositions();
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

		grabOffset = sourceObject.transform.position - MouseToWorldPosition();

		foreach (AnchorPoint door in guideObject.GetComponentsInChildren<AnchorPoint>())
		{
			Destroy(door.gameObject);
		}
	}

	private void Drop()
	{
		sourceObject.transform.position = guideObject.transform.position;
		sourceObject.transform.rotation = guideObject.transform.rotation;
		// TODO: Connect doors before removing selected and guide objects

		Destroy(selectedObject);
		selectedObject = null;
		Destroy(guideObject);
		guideObject = null;
	}

	private void UpdatePositions()
	{
		selectedObject.transform.position = MouseToWorldPosition() + grabOffset;
	}

	private Vector3 MouseToWorldPosition()
	{
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.WorldToScreenPoint(selectedObject.transform.position).z;
		return Camera.main.ScreenToWorldPoint(mousePos);
	}
}


