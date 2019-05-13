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
* Ludvig Björk Förare (190430)
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

	[Range(0, 1)]
	public float GlowDropoff = 0.14f;

	private GameObject sourceObject;
	private GameObject selectedObject;
	private GameObject guideObject;

	private AnchorPoint bestSrcPoint;
	private AnchorPoint bestDstPoint;

	private Vector3 grabOffset = new Vector3();

	// Mouse position of current Puppeteer. Used when server is not puppeteer.
	private Vector3 localPlayerMousePos;

    private RoomInteractable lastHit;

	// Original parent node used for updating tree when dropping without snapping to something.
	private RoomTreeNode firstParentNode;
	// Current selected node in tree. Used by RoomTreeNode to allow the selected object to be used in new tree.
	public RoomTreeNode currentNode;

    public readonly int MaxNumCollisions = 16;
    private Collider[] overlapColliders;
    private float updateInterval = 0.3f;

	void Start()
    {
		level = GetComponent<LevelBuilder>();

        if(isServer)
        {
            overlapColliders = new Collider[MaxNumCollisions];
            InvokeRepeating("ServerUpdate", 0.5f * updateInterval, updateInterval);
        }
    }

    void Update()
    {
        if(selectedObject == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, RaycastDistance, 1 << 8))
            {
                RoomInteractable interactable = hit.transform.GetComponent<RoomInteractable>();
                if (interactable != lastHit)
                {
                    lastHit = interactable;
                }
            }
            else
            {
                lastHit = null;
            }

            if (lastHit != null && Input.GetButtonDown("Fire") && lastHit.CanBePickedUp)
            {
                this.Pickup(lastHit.gameObject);
            }
        }
        else
        {
            // send current mouse position to server
            CmdUpdateMousePos(this.MouseToWorldPosition());

            Vector3 newPosition = MouseToWorldPosition() + grabOffset;
            selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, new Vector3(newPosition.x, LiftHeight, newPosition.z), LiftSpeed * Time.deltaTime);

            if (Input.GetButtonDown("Rotate"))
            {
                selectedObject.transform.Rotate(Vector3.up * 90.0f);
                CmdRotate(selectedObject.transform.rotation);
            }

            if (Input.GetButtonUp("Fire"))
            {
                Drop();
            }
        }
    }

	private void ServerUpdate()
	{
        if (selectedObject == null)
            return;

        var doorsInSelectedRoom = selectedObject.GetComponentsInChildren<AnchorPoint>();
        float bestDist = Mathf.Infinity;
        bestDstPoint = null;

        // Decide which door is best to snap to.
        foreach (var door in doorsInSelectedRoom)
        {
            if (door.transform.parent.IsChildOf(sourceObject.transform))
                continue;

            if (door.transform.parent.IsChildOf(guideObject.transform))
                continue;

            var nearestDoor = FindNearest(door, ref bestDist);
            if (nearestDoor != null)
            {
                bestSrcPoint = door;
                bestDstPoint = nearestDoor;
            }
        }

        // Move guideObject to best availible position. If there is none, move it to source.
        if (bestDstPoint != null)
        {
            if(this.CanConnect(bestSrcPoint, bestDstPoint))
            {
                guideObject.transform.position = selectedObject.transform.position - (bestSrcPoint.transform.position - bestDstPoint.transform.position);
                guideObject.transform.rotation = selectedObject.transform.rotation;

                RoomTreeNode currentNode = sourceObject.GetComponent<RoomTreeNode>();
                RoomTreeNode targetNode = bestDstPoint.GetComponentInParent<RoomTreeNode>();
                currentNode.DisconnectFromTree();
                currentNode.SetParent(targetNode);
                currentNode.CutBranch();
                level.GetStartNode().ReconnectToTree();
            }

            if(guideObject.transform.hasChanged)
            {
                RpcUpdateGuide(new TransformStruct(guideObject.transform.position, guideObject.transform.rotation));
            }
        }
    }

	// Method used for picking up an object.
	private void Pickup(GameObject pickupObject)
	{
		sourceObject = pickupObject;
		selectedObject = Instantiate(sourceObject);
		guideObject = Instantiate(sourceObject);
		guideObject.name = "guideObject";

		grabOffset = sourceObject.transform.position - MouseToWorldPosition();

		CmdUpdateMousePos(this.MouseToWorldPosition());
		CmdPickup(pickupObject);
	}

    // Rotate room on server
    [Command]
    public void CmdRotate(Quaternion rot)
    {
        selectedObject.transform.rotation = rot;
    }

    // Update mouse position on server
    [Command]
    public void CmdUpdateMousePos(Vector3 pos)
    {
        localPlayerMousePos = pos;
    }

    // Method for picking up objects on server and making them invisible if server is not Puppeteer.
    [Command]
	public void CmdPickup(GameObject pickupObject)
	{
		if (!isLocalPlayer)
		{
			sourceObject = pickupObject;
			selectedObject = Instantiate(sourceObject);
			guideObject = Instantiate(sourceObject);
			guideObject.name = "guideObject";
			// Disable colliders on server when server is not puppeteer.
			foreach (BoxCollider collider in guideObject.GetComponentsInChildren<BoxCollider>())
			{
				collider.enabled = false;
			}
		}
		
		grabOffset = sourceObject.transform.position - localPlayerMousePos;

		// Save the parent node of the picked up room to be able to reset if the position doesn't change.
		firstParentNode = sourceObject.GetComponent<RoomTreeNode>().GetParent();

		if (!isLocalPlayer)
		{
			foreach (MeshRenderer renderer in selectedObject.GetComponentsInChildren<MeshRenderer>())
			{
				renderer.enabled = false;
			}

			foreach (MeshRenderer renderer in guideObject.GetComponentsInChildren<MeshRenderer>())
			{
				renderer.enabled = false;
			}
		}
	}

	// Method to drop rooms to snapped position.
	private void Drop()
	{
		CmdDrop();
		if (!isServer)
		{
			Destroy(selectedObject);
			Destroy(guideObject);
			selectedObject = null;
			guideObject = null;
		}
	}

	// Method to update position of source object on server when drop happens.
	[Command]
	public void CmdDrop()
	{
		// Reset tree if position doesn't change.
		if (sourceObject.transform.position == guideObject.transform.position && sourceObject.transform.rotation == guideObject.transform.rotation)
		{
			sourceObject.GetComponent<RoomTreeNode>().SetParent(firstParentNode);
		}
		else
		{
            if(sourceObject != null)
            {
			    // Kill minions in room
			    sourceObject.GetComponent<RoomInteractable>().KillEnemiesInRoom();

			    // Move sourceobject to guideobject. Guideobject is already in the best availible position.
			    sourceObject.transform.SetPositionAndRotation(guideObject.transform.position, guideObject.transform.rotation);
			
			    // Connect all doors in the new position.
			    level.ConnectDoorsInRoomIfPossible(sourceObject);
            }
		}

		Destroy(selectedObject);
		Destroy(guideObject);
		selectedObject = null;
		guideObject = null;
	}

	// Method to send data from server to client about position of guideRoom.
	[ClientRpc]
	public void RpcUpdateGuide(TransformStruct target)
	{
		if (isLocalPlayer && guideObject != null)
		{
			guideObject.transform.rotation = target.Rotation;
			guideObject.transform.position = target.Position;
		}
	}

    public AnchorPoint FindNearest(in AnchorPoint target, ref float bestDist)
    {
        var list = new List<AnchorPoint>();
        var rooms = level.GetRooms();
        foreach(var room in rooms)
        {
            list.AddRange(room.GetComponentsInChildren<AnchorPoint>());
        }

        AnchorPoint result = null;
        foreach(var item in list)
        {
            if (target.transform.parent == item.transform.parent)
                continue;

            float curDist = Vector3.Distance(item.transform.position, target.transform.position);
            if (curDist > SnapDistance)
            {
                // ignore if too far apart
                continue;
            }
            else if(curDist < bestDist)
            {
                result = item;
                bestDist = curDist;
            }
        }
        return result;
    }

	private bool CanConnect(in AnchorPoint src, in AnchorPoint dst)
	{
        // Only connect modules with correct door angles.
        if (Mathf.RoundToInt((src.transform.forward + dst.transform.forward).magnitude) != 0)
			return false;

		// Check if source room contains player
		if (sourceObject.GetComponent<RoomInteractable>().RoomContainsPlayer())
			return false;
        
		// Only to check collision (not real movement)
		guideObject.transform.rotation = selectedObject.transform.rotation;
		guideObject.transform.position = selectedObject.transform.position - (src.transform.position - dst.transform.position);

        for(int i = 0; i < overlapColliders.Length; i++)
        {
            overlapColliders[i] = null;
        }

        int numCollisions = Physics.OverlapSphereNonAlloc(guideObject.transform.position, 8.0f, overlapColliders, 1 << 8);
        if(numCollisions >= MaxNumCollisions)
        {
            Debug.LogWarning("Too many collisions! Some collisions may be ignored.");
        }

        int actual = 0;

        for(int i = 0; i < overlapColliders.Length; i++)
        {
            Collider collider = overlapColliders[i];
            if(collider == null || collider.transform.IsChildOf(sourceObject.transform))
            {
                continue;
            }

            actual++;
        }

        /*
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
        */

        /*
		foreach (AnchorPoint guideDoor in guideObject.GetComponentsInChildren<AnchorPoint>())
		{
			guideDoor.Connected = false;
			guideDoor.ConnectedTo = null;

			foreach (AnchorPoint placedDoor in GameObject.Find("Level").GetComponentsInChildren<AnchorPoint>())
			{
				if (guideDoor == placedDoor)
				{
					continue;
				}
				if (guideDoor.GetPosition() == placedDoor.GetPosition())
				{
					guideDoor.NoSpawnConnectDoor(placedDoor);
					guideDoor.GetComponentInParent<RoomTreeNode>().InTree = true;
					break;
				}
			}
		}
        */

		// Check if it is possible to create a new valid tree when the room is moved. This should be done last.
		currentNode = sourceObject.GetComponent<RoomTreeNode>();
		RoomTreeNode parentNode = currentNode.GetParent();

		currentNode.DisconnectFromTree();
		
		if (!dst.GetComponentInParent<RoomTreeNode>().InTree)
		{
			DisconnectGuideDoors();
			return false;
		}

		currentNode.SetParent(dst.GetComponentInParent<RoomTreeNode>());

		if (sourceObject.GetComponent<RoomTreeNode>().CutBranch())
		{
			level.GetStartNode().ReconnectToTree();
			DisconnectGuideDoors();
		}
		else
		{
			currentNode.SetParent(parentNode);
			level.GetStartNode().ReconnectToTree();
			DisconnectGuideDoors();
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

	private void DisconnectGuideDoors()
	{
		foreach (AnchorPoint guideDoor in guideObject.GetComponentsInChildren<AnchorPoint>())
		{
			guideDoor.NoSpawnDisconnectDoor();
		}
	}
}