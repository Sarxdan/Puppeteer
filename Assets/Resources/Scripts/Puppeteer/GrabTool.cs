using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund, Anton "Knugen" Jonsson
*
* DESCRIPTION:
* Tool used for grabbing and dropping rooms as puppeteer. Also does checks to determine if drop is legal.
*
* CODE REVIEWED BY:
* Filip Renman (24/4/2019)
*
* CONTRIBUTORS:
* Philip Stenmark
*/

public class GrabTool : NetworkBehaviour
{
	private LevelBuilder level;

	// The maximum distance for snapping modules
	public int SnapDistance = 10;
	// Maximum raycast ray length
	public float RaycastDistance = 500;
	// The lift height when grabbing an object
	public float LiftHeight = 3.0f;
	// The lift speed when grabbing an object
	public float LiftSpeed = 50.0f;

	// enables camera movement using mouse scroll
	public bool EnableMovement = true;

	private GameObject sourceObject;
	private GameObject selectedObject;
	private GameObject guideObject;

	private AnchorPoint bestSrcPoint;
	private AnchorPoint bestDstPoint;

	private RoomInteractable lastHit;
	private Vector3 grabOffset = new Vector3();

	private Vector3 localPlayerMousePos;

	// Original parent node used for updating tree when dropping without snapping to something.
	private RoomTreeNode firstParentNode;

    void Start()
    {
		//level = GameObject.Find("Level").GetComponent<LevelBuilder>();
		level = GetComponent<LevelBuilder>();
    }

    void Update()
    {
		if (isLocalPlayer)
		{
			ClientUpdate();
		}

		if (isServer)
		{
			if (selectedObject != null)
			{
				ServerUpdate();
			}
		}
    }

	private void ClientUpdate()
	{
		if (selectedObject == null)
		{
			// Raycast only on Puppeteer Interact layer.
			RaycastHit hit;
			int layerMask = 1 << LayerMask.NameToLayer("Puppeteer Interact");
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, RaycastDistance, layerMask, QueryTriggerInteraction.Collide))
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
			CmdUpdateMousePos(MouseToWorldPosition());

			if (Input.GetButtonUp("Fire"))
			{
				Drop();
			}
			else
			{
				if (Input.GetButtonDown("Rotate"))
				{
					// Rotate room around its own up-axis

					selectedObject.transform.RotateAround(selectedObject.transform.position, selectedObject.transform.up, 90);
					CmdRotate(selectedObject.transform.rotation);
				}
				if (!isServer)
				{
					ClientUpdatePositions();
				}

			}
		}
	}

	[Command]
	public void CmdRotate(Quaternion rot)
	{
		selectedObject.transform.rotation = rot;
	}

	[Command]
	public void CmdUpdateMousePos(Vector3 pos)
	{
		localPlayerMousePos = pos;
	}

	private void ServerUpdate()
	{
		ServerUpdatePositions();
	}

	// Method used for picking up an object.
	private void Pickup(GameObject pickupObject)
	{
		if (!isServer)
		{
			sourceObject = pickupObject;
			sourceObject.name = "CurrentSourceObject";

			selectedObject = Instantiate(sourceObject);
			selectedObject.name = "SelectedObject";

			guideObject = Instantiate(sourceObject);
			guideObject.name = "GuideObject";

			grabOffset = sourceObject.transform.position - MouseToWorldPosition();
		}

		CmdUpdateMousePos(MouseToWorldPosition());
		CmdPickup(pickupObject);
	}

	[Command]
	public void CmdPickup(GameObject pickupObject)
	{
		sourceObject = pickupObject;
		sourceObject.name = "CurrentSourceObject";

		selectedObject = Instantiate(sourceObject);
		selectedObject.name = "SelectedObject";

		guideObject = Instantiate(sourceObject);
		guideObject.name = "GuideObject";

		grabOffset = sourceObject.transform.position - localPlayerMousePos;

		// Save the parent node of the picked up room to be able to reset if the position doesn't change.
		firstParentNode = sourceObject.GetComponent<RoomTreeNode>().GetParent();
	}

	private void Drop()
	{
		CmdDrop();
		if (!isServer)
		{
			Destroy(selectedObject);
			selectedObject = null;
			Destroy(guideObject);
			guideObject = null;
		}
	}

	[Command]
	public void CmdDrop()
	{
		// Reset tree if position doesn't change.
		if (sourceObject.transform.position == guideObject.transform.position && sourceObject.transform.rotation == guideObject.transform.rotation)
		{
			sourceObject.GetComponent<RoomTreeNode>().SetParent(firstParentNode);
		}

		// Move sourceobject to guideobject. Guideobject is already in the best availible position.
		sourceObject.transform.position = guideObject.transform.position;
		sourceObject.transform.rotation = guideObject.transform.rotation;

		Destroy(selectedObject);
		selectedObject = null;
		Destroy(guideObject);
		guideObject = null;

		// Connect all doors in the new position.
		level.ConnectDoorsInRoomIfPossible(sourceObject);
	}

	private void ClientUpdatePositions()
	{
		Vector3 newPosition = MouseToWorldPosition() + grabOffset;
		selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, new Vector3(newPosition.x, LiftHeight, newPosition.z), LiftSpeed * Time.deltaTime);
	}

	// Moves all objects to their positions.
	private void ServerUpdatePositions()
	{
		// Move source object to mouse.
		Vector3 newPosition = localPlayerMousePos + grabOffset;
		selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, new Vector3(newPosition.x, LiftHeight, newPosition.z), LiftSpeed * Time.deltaTime);

		var doorsInSelectedRoom = selectedObject.GetComponentsInChildren<AnchorPoint>();
		float bestDist = Mathf.Infinity;

		bestDstPoint = null;

		// Decide which door is best to snap to.
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

		// Move guideObject to best availible position. If there is none, move it to source.
		if (bestDstPoint != null)
		{
			RpcUpdateGuide(new TransformStruct(selectedObject.transform.position - (bestSrcPoint.transform.position - bestDstPoint.transform.position), selectedObject.transform.rotation));
			guideObject.transform.position = selectedObject.transform.position - (bestSrcPoint.transform.position - bestDstPoint.transform.position);
			guideObject.transform.rotation = selectedObject.transform.rotation;

			RoomTreeNode currentNode = sourceObject.GetComponent<RoomTreeNode>();
			RoomTreeNode targetNode = bestDstPoint.GetComponentInParent<RoomTreeNode>();
			currentNode.DisconnectFromTree();
			currentNode.SetParent(targetNode);
			currentNode.CutBranch();
			level.GetStartNode().ReconnectToTree();
		}
		else
		{
			RpcUpdateGuide(new TransformStruct(sourceObject.transform.position, sourceObject.transform.rotation));
			guideObject.transform.position = sourceObject.transform.position;
			guideObject.transform.rotation = sourceObject.transform.rotation;
		}
	}

	[ClientRpc]
	public void RpcUpdateGuide(TransformStruct target)
	{
		if (isLocalPlayer)
		{
			guideObject.transform.rotation = target.Rotation;
			guideObject.transform.position = target.Position;
		}
	}

	// Checks all other doors in the level and picks the best one.
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
			// Skip the door if not all requirements are met.
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
		// Check if source room contains player
		if (sourceObject.GetComponent<RoomInteractable>().RoomContainsPlayer())
		{
			return false;
		}

		// Only to check collision (not real movement)
		guideObject.transform.rotation = selectedObject.transform.rotation;
		guideObject.transform.position = selectedObject.transform.position - (src.transform.position - dst.transform.position);

		RoomCollider[] placedRoomColliders = level.GetLevel().GetComponentsInChildren<RoomCollider>();

		// Check if room overlaps when moved.
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

		// Check if it is possible to create a new valid tree when the room is moved. This should be done last.
		RoomTreeNode currentNode = sourceObject.GetComponent<RoomTreeNode>();
		RoomTreeNode parentNode = currentNode.GetParent();

		currentNode.DisconnectFromTree();
		
		if (!dst.GetComponentInParent<RoomTreeNode>().InTree())
		{
			return false;
		}

		currentNode.SetParent(dst.GetComponentInParent<RoomTreeNode>());

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

		// All criterias are met
		return true;
	}

	private Vector3 MouseToWorldPosition()
	{
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.WorldToScreenPoint(selectedObject.transform.position).z;
		return Camera.main.ScreenToWorldPoint(mousePos);
	}
}


