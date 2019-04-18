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
	private LevelBuilder level;

	// the maximum distance for snapping modules
	public int SnapDistance = 150;

	// the lift height when grabbing an object
	public float LiftHeight = 2.0f;

	// the lift speed when grabbing an object
	public float LiftSpeed = 0.1f;

	// enables camera movement using mouse scroll
	public bool EnableMovement = true;

	private GameObject sourceObject;
	private GameObject selectedObject;
	private GameObject guideObject;

	private AnchorPoint bestSrcPoint;
	private AnchorPoint bestDstPoint;

	private RoomInteractable lastHit;
	private Vector3 grabOffset = new Vector3();

    void Start()
    {
		level = GameObject.Find("Level").GetComponent<LevelBuilder>();
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
						if (interactable.CanBePickedUp)
						{
							Pickup(hitObject);
						}
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
				if (lastHit != null)
				{
					lastHit.OnRaycastExit();
					lastHit = null;
				}
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
		sourceObject.name = "currentSourceObject";

		selectedObject = Instantiate(sourceObject);
		selectedObject.name = "SelectedObject";

		guideObject = Instantiate(sourceObject);
		guideObject.name = "GuideObject";

		grabOffset = sourceObject.transform.position - MouseToWorldPosition();

		// NOT NEEDED ANYMORE.
		//foreach (AnchorPoint door in guideObject.GetComponentsInChildren<AnchorPoint>())
		//{
		//	Destroy(door.gameObject);
		//}
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

		level.ConnectDoorsInRoomIfPossible(sourceObject);
	}

	private void UpdatePositions()
	{
		selectedObject.transform.position = MouseToWorldPosition() + grabOffset;
		var doorsInSelectedRoom = selectedObject.GetComponentsInChildren<AnchorPoint>();
		float bestDist = Mathf.Infinity;

		bestDstPoint = null;

		foreach (var ownDoor in doorsInSelectedRoom)
		{
			var nearestDoor = FindNearestOpenDoor(ownDoor, ref bestDist);
			if (nearestDoor != null)
			{
				bestSrcPoint = ownDoor;
				bestDstPoint = nearestDoor;
				
				Debug.DrawLine(bestDstPoint.transform.position, bestSrcPoint.transform.position, Color.yellow);		
			}
		}

		if (bestDstPoint != null)
		{
			guideObject.transform.rotation = selectedObject.transform.rotation;
			guideObject.transform.position = selectedObject.transform.position - (bestSrcPoint.transform.position - bestDstPoint.transform.position);
		}
		else
		{
			guideObject.transform.rotation = sourceObject.transform.rotation;
			guideObject.transform.position = sourceObject.transform.position;
		}


	}

	private AnchorPoint FindNearestOpenDoor(in AnchorPoint doorIn, ref float bestDist)
	{
		var doors = new List<AnchorPoint>();
		var rooms = level.GetRooms();
		foreach (var room in rooms)
		{
			doors.AddRange(room.GetComponentsInChildren<AnchorPoint>());
		}
		AnchorPoint result = null;
		foreach (var door in doors)
		{
			if (door.transform.parent == doorIn.transform.parent)
				continue;
			if (door.transform.parent.IsChildOf(sourceObject.transform))
				continue;
			if (door.transform.parent.IsChildOf(guideObject.transform))
				continue;
			if (!CanConnect(doorIn, door))
				continue;

			float curDist = (doorIn.transform.position - door.transform.position).sqrMagnitude;
			if (curDist < bestDist)
			{
				result = door;
				bestDist = curDist;
			}
		}
		return result;
	}

	private bool CanConnect(in AnchorPoint src, in AnchorPoint dst)
	{
		// Ignore invalid doors.
		if (src == null || dst == null)
		{
			return false;
		}
		// Ignore already connected doors.
		if (dst.Connected && !dst.ConnectedTo.transform.IsChildOf(sourceObject.transform))
		{
			return false;
		}
		// Only connect modules with correct door angles.
		if (Mathf.RoundToInt((src.transform.forward + dst.transform.forward).magnitude) != 0)
		{
			return false;
		}
		// Check if doors are too far apart
		float dist = (dst.transform.position - src.transform.position).magnitude;
		if (dist > SnapDistance)
		{
			return false;
		}
		// Only to check collision (not real movement)
		guideObject.transform.rotation = selectedObject.transform.rotation;
		guideObject.transform.position = selectedObject.transform.position - (src.transform.position - dst.transform.position);

		RoomCollider[] placedRoomColliders = level.gameObject.GetComponentsInChildren<RoomCollider>();

		foreach (RoomCollider placedRoomCollider in placedRoomColliders)
		{
			if (placedRoomCollider.transform.IsChildOf(sourceObject.transform))
			{
				continue;
			}
			foreach (RoomCollider guideCollider in guideObject.GetComponentsInChildren<RoomCollider>())
			{
				if (guideCollider.GetPosition() == placedRoomCollider.GetPosition())
				{
					guideObject.transform.SetPositionAndRotation(sourceObject.transform.position, sourceObject.transform.rotation);
					return false;
				}
			}
		}

		RoomTreeNode currentNode = sourceObject.GetComponent<RoomTreeNode>();
		RoomTreeNode parentNode = currentNode.GetParent();

		currentNode.SetParent(dst.GetComponentInParent<RoomTreeNode>());
		currentNode.DestroyChildren();

		if (sourceObject.GetComponent<RoomTreeNode>().CutBranch())
		{
			level.GetStartNode().ReconnectToTree();
		}
		else
		{
			currentNode.SetParent(parentNode);
			level.GetStartNode().ReconnectToTree();
			return false;
		}

		// No false
		return true;
	}

	private Vector3 MouseToWorldPosition()
	{
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.WorldToScreenPoint(selectedObject.transform.position).z;
		return Camera.main.ScreenToWorldPoint(mousePos);
	}
}


