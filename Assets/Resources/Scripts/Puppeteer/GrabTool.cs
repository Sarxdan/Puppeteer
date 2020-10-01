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
//CLEANED

public class GrabTool : NetworkBehaviour
{
    private PuppeteerRoomSounds sounds;
	private LevelBuilder level;

    // The lift height when grabbing an object
    public float LiftHeight = 3.0f;
    // The lift speed when grabbing an object
    public float LiftSpeed = 50.0f;

    private GameObject sourceObject;
    private GameObject selectedObject;
    private GameObject guideObject;

    private AnchorPoint bestSrcPoint;
    private AnchorPoint bestDstPoint;

    private Vector3 grabOffset;

    // Mouse position of current Puppeteer. Used when server is not puppeteer.
    private Vector3 localPlayerMousePos;

    // tracks the last hit object
    private RoomInteractable lastHit;

    // Original parent node used for updating tree when dropping without snapping to something.
    private RoomTreeNode firstParentNode;
    // Current selected node in tree. Used by RoomTreeNode to allow the selected object to be used in new tree.
    public RoomTreeNode currentNode;

    public readonly float UpdateFrequency = 0.12f;
    private RoomCollider[] guideColliders;

    void Start()
    {
        sounds = GetComponent<PuppeteerRoomSounds>();
		level = GetComponent<LevelBuilder>();

        if (isServer)
        {
            InvokeRepeating("ServerUpdate", 0, UpdateFrequency);
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (selectedObject == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1 << 8))
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

            if (Input.GetButtonUp("Fire"))
            {
                Drop();
            }

            if (Input.GetButtonDown("Rotate"))
            {
                sounds.Rotate();
                selectedObject.transform.Rotate(Vector3.up * 90.0f);
                CmdRotate(selectedObject.transform.rotation);
            }
        }
    }

    private void ServerUpdate()
    {
        if (selectedObject == null)
            return;

        // update mouse position
        Vector3 newPosition = localPlayerMousePos + grabOffset;
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, new Vector3(newPosition.x, LiftHeight, newPosition.z), LiftSpeed * Time.deltaTime);

        var doorsInSelectedRoom = selectedObject.GetComponentsInChildren<AnchorPoint>();
        float bestDist = Mathf.Infinity;
        bestDstPoint = null;

        // Decide which door is best to snap to.
        foreach (var door in doorsInSelectedRoom)
        {
            var nearestDoor = FindNearest(door, ref bestDist);
            if (nearestDoor != null)
            {
                bestSrcPoint = door;
                bestDstPoint = nearestDoor;
            }
        }

        // Move guideObject to best availible position. If there is none, move it to source.
        if (bestDstPoint != null && selectedObject.transform.hasChanged)
        {
            if (this.CanConnect(bestSrcPoint, bestDstPoint))
            {
                // send over network
                RpcUpdateGuide(new TransformStruct(selectedObject.transform.position - (bestSrcPoint.transform.position - bestDstPoint.transform.position), selectedObject.transform.rotation.normalized));

                RoomTreeNode currentNode = sourceObject.GetComponent<RoomTreeNode>();
                RoomTreeNode targetNode = bestDstPoint.GetComponentInParent<RoomTreeNode>();
                currentNode.DisconnectFromTree();
                currentNode.SetParent(targetNode);
                currentNode.CutBranch();
                level.GetStartNode().ReconnectToTree();
            }
            else
            {
                // update over network
                RpcUpdateGuide(new TransformStruct(sourceObject.transform.position, sourceObject.transform.rotation.normalized));
                guideObject.transform.SetPositionAndRotation(sourceObject.transform.position, sourceObject.transform.rotation);
            }
        }
    }

	// Method used for picking up an object.
	private void Pickup(GameObject pickupObject)
	{
        sounds.Pickup();

		sourceObject = pickupObject;
		selectedObject = Instantiate(sourceObject);
		guideObject = Instantiate(sourceObject);
        guideObject.name = "guideObject";
        guideColliders = guideObject.GetComponentsInChildren<RoomCollider>();

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
            guideColliders = guideObject.GetComponentsInChildren<RoomCollider>();

            // disable render
            foreach (var renderer in selectedObject.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = false;
            }
        }

        grabOffset = sourceObject.transform.position - localPlayerMousePos;

		// Save the parent node of the picked up room to be able to reset if the position doesn't change.
		firstParentNode = sourceObject.GetComponent<RoomTreeNode>().GetParent();
	}

	// Method to drop rooms to snapped position.
	private void Drop()
	{
        sounds.Place();

		CmdDrop();
		if (!isServer)
		{
			Destroy(guideObject);
			Destroy(selectedObject);
			selectedObject = null;
			guideObject = null;
		}
	}

	// Method to update position of source object on server when drop happens.
	[Command]
	public void CmdDrop()
	{
        if (sourceObject != null)
        {
            // Reset tree if position doesn't change.
            if (sourceObject.transform.position == guideObject.transform.position && sourceObject.transform.rotation == guideObject.transform.rotation)
            {
                sourceObject.GetComponent<RoomTreeNode>().SetParent(firstParentNode);
            }
            else
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
        foreach (var room in rooms)
        {
            list.AddRange(room.GetComponentsInChildren<AnchorPoint>());
        }

        AnchorPoint result = null;
        foreach (var item in list)
        {
            float curDist = Vector3.Distance(item.transform.position, target.transform.position);
            if (curDist < bestDist && !item.Connected)
            {
                result = item;
                bestDist = curDist;
            }
        }
        return result;
    }

    private bool CanConnect(in AnchorPoint src, in AnchorPoint dst)
    {
        // ignore invalid
        if(src == null || dst == null)
        {
            return false;
        }
         
        // only connect modules with correct door angles.
        if (Mathf.RoundToInt((src.transform.forward + dst.transform.forward).magnitude) != 0)
        {
            return false;
        }

        // check if source room contains player
        if (sourceObject != null && sourceObject.GetComponent<RoomInteractable>().RoomContainsPlayer())
        {
            return false;
        }

        guideObject.transform.position = selectedObject.transform.position - (bestSrcPoint.transform.position - bestDstPoint.transform.position);
        guideObject.transform.rotation = selectedObject.transform.rotation;
      
        RoomCollider[] placedRoomColliders = level.GetLevel().GetComponentsInChildren<RoomCollider>();

        foreach (var placedRoomCollider in placedRoomColliders)
        {
            if (placedRoomCollider.transform.IsChildOf(sourceObject.transform))
            {
                continue;
            }
            foreach (var guideCollider in guideColliders)
            {
                if (guideCollider.GetPosition() == placedRoomCollider.GetPosition())
                {
                    return false;
                }
            }
        }

        currentNode = sourceObject.GetComponent<RoomTreeNode>();
        RoomTreeNode parentNode = currentNode.GetParent();

        currentNode.DisconnectFromTree();

        if (!bestDstPoint.GetComponentInParent<RoomTreeNode>().InTree)
        {
            return false;
        }

        currentNode.SetParent(bestDstPoint.GetComponentInParent<RoomTreeNode>());

        if (!sourceObject.GetComponent<RoomTreeNode>().CutBranch())
        {
            currentNode.SetParent(parentNode);
            level.GetStartNode().ReconnectToTree();
            return false;
        }

        level.GetStartNode().ReconnectToTree();

        // made it!
        return true;
    }

    private Vector3 MouseToWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(selectedObject.transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}