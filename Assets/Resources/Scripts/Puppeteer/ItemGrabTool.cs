using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund, Kristoffer "Krig" Lundgren
*
* DESCRIPTION:
* Tool used for grabbing and dropping traps and spawners as puppeteer. Also does checks to determine if drop is legal.
*
* CODE REVIEWED BY:
* Filip rehman 2019-05-07 
*
* CONTRIBUTORS:
* Philip Stenmark, Anton "Knugen" Jonsson
*/

public class ItemGrabTool : NetworkBehaviour
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
	//The distance of the groud that the preview trap is
	private float PreviewLiftHeight = 2.0f;

	// enables camera movement using mouse scroll
	public bool EnableMovement = true;
	// The object which the player clicks on
	private GameObject sourceObject;
	// The object which floats around following the mouse
	private GameObject selectedObject;
	// The object which snaps to points in the map
	private GameObject guideObject;
	// The vector that is added to the preview objects vector to move it up before it is placed
	private Vector3 previewLiftVector;
	private SnapPointBase bestDstPoint;

	private SnapFunctionality lastHit;
	private Vector3 grabOffset = new Vector3();

	private Currency currency;
	private int cost;

	// Mouse position of current Puppeteer. Used when server is not puppeteer.
	private Vector3 localPlayerMousePos;

    // Start is called before the first frame update
    void Start()
    {
        level = GetComponent<LevelBuilder>();
		currency = GetComponent<Currency>();
		previewLiftVector = new Vector3(0,PreviewLiftHeight,0);

    }

    // Update is called once per frame
    void Update()
	{
		// Update local players rooms and send data to server. Purely visual.
		if (isLocalPlayer)
            ClientUpdate();

		// Main update and checks always run on server
		if (isServer)
            if (selectedObject != null)
                ServerUpdate();
    }

    private void ClientUpdate()
    {
        if (selectedObject == null)
        {
			// Raycast only on Puppeteer Item Interact layer.
			RaycastHit hit;
            int layermask = 1 << LayerMask.NameToLayer("PuppeteerItemInteract");
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit, RaycastDistance, layermask, QueryTriggerInteraction.Collide))
            {
                GameObject hitObject = hit.transform.gameObject;

				// Start and stop glow
				var trapComponent = hitObject.GetComponent<SnapFunctionality>();
                if (trapComponent != null)
                {
                    if (trapComponent != lastHit)
                    {
                        if (lastHit != null)
                        {
                            var glow = lastHit.GetComponent<Glowable>();
                            if (glow)
                            {
                                glow.Toggle(false);
                            }
                        }
                        lastHit = trapComponent;
                        if(!lastHit.Placed)
                        {
                            var glow = lastHit.GetComponent<Glowable>();
                            if (glow)
                            {
                                glow.Toggle(true);
                            }
                        }
                    }
					// If pickup button is pressed, call pickup method.
					if (Input.GetButtonDown("Fire"))
					{
						if (!trapComponent.Placed)
						{
							Pickup(hitObject);
						}
					}
                }
                else
                {
					// If raycast doesn't hit a valid object, stop previus glow.
					if (lastHit != null)
                    {
                        var glow = lastHit.GetComponent<Glowable>();
                        if (glow)
                        {
                            glow.Toggle(false);
                        }
                        lastHit = null;
                    }
                }
            }
            else
			{
				// If raycast doesn't hit any objects, stop previus glow.
                if (lastHit != null)
                {
                    var glow = lastHit.GetComponent<Glowable>();
                    if (glow)
                    {
                        glow.Toggle(false);
                    }
                    lastHit = null;
                }
			}
        }
        else
		{
			// Send current mouseposition to server
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

	// Rotate trap on server
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

    private void ServerUpdate()
    {
        ServerUpdatePositions();
    }
	// Method used for picking up an object.
	private void Pickup(GameObject pickupTrap)
    {
		// Instansiate the floating object
		sourceObject = pickupTrap;
		selectedObject = Instantiate(sourceObject);
		selectedObject.name = "SelectedObject";

		// Handles the change in temporary currency. Can be used to show currency left after placement.
		cost = selectedObject.GetComponent<SnapFunctionality>().Cost;
		currency.TemporaryCurrency = currency.CurrentCurrency - cost;
		// Instansiate the guide object on the ground
		guideObject = Instantiate(sourceObject);
		guideObject.name = "GuideObject";

		grabOffset = sourceObject.transform.position - MouseToWorldPosition();

        CmdUpdateMousePos(MouseToWorldPosition());
        CmdPickup(pickupTrap);
    }

    [Command]
    public void CmdPickup(GameObject pickupTrap)
    {
		if (!isLocalPlayer)
		{
			sourceObject = pickupTrap;
			selectedObject = Instantiate(sourceObject);
			selectedObject.name = "SelectedObject";

			// handels the change in temporary currency. can be used to show currency left after placement.
			cost = selectedObject.GetComponent<SnapFunctionality>().Cost;
			currency.TemporaryCurrency = currency.CurrentCurrency - cost;

			guideObject = Instantiate(sourceObject);
			guideObject.name = "GuideObject";

			// Disable colliders on server when server is not puppeteer.
			foreach (BoxCollider collider in guideObject.GetComponentsInChildren<BoxCollider>())
			{
				collider.enabled = false;
			}
		}

		grabOffset = sourceObject.transform.position - localPlayerMousePos;

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

    private void Drop()
    {
        CmdDrop();
		if (!isServer)
		{
			Destroy(selectedObject);
			selectedObject = null;

			// removes currency from the puppeteer when placed.
			currency.CurrentCurrency = currency.CurrentCurrency - cost;

			guideObject.name = "Placed Trap";
			guideObject.transform.SetParent(bestDstPoint.transform.parent);
			guideObject.transform.position -=previewLiftVector;
            guideObject.GetComponent<SnapFunctionality>().Placed = true;
			NetworkServer.Spawn(guideObject);
			SetLayerOnAll(guideObject, 0);
			SnapPointBase point = bestDstPoint.GetComponent<SnapPointBase>();
			point.Used = true;
			guideObject = null;
		}
    }

	// Method to update position of source object on server when drop happens.
	[Command]
    public void CmdDrop()
    {
		// Reset everything if trap is droped without a target place.
		if (sourceObject.transform.position.x == guideObject.transform.position.x && sourceObject.transform.position.z == guideObject.transform.position.z && sourceObject.transform.rotation == guideObject.transform.rotation)
		{
			Destroy(selectedObject);
			selectedObject = null;
			Destroy(guideObject);
			guideObject = null;
		}
		else
		{
			Destroy(selectedObject);
			selectedObject = null;

			// removes currency from the puppeteer when placed.
			currency.CurrentCurrency = currency.CurrentCurrency - cost;

			guideObject.name = "Placed Trap";
			guideObject.transform.SetParent(bestDstPoint.transform.parent);
			guideObject.transform.position -=previewLiftVector;
			guideObject.GetComponent<SnapFunctionality>().Placed = true;
			NetworkServer.Spawn(guideObject);
			// Set the layer on the item and all of its children in order to make it visible and interactable
			SetLayerOnAll(guideObject, 0);
			SnapPointBase point = bestDstPoint.GetComponent<SnapPointBase>();
			point.Used = true;
			//bestDstPoint.GetComponent<TrapSnapPoint>().Used = true;
			guideObject = null;
		}
    }

	// Move local selected object for client.
	private void  ClientUpdatePositions()
    {
        Vector3  newPosition = MouseToWorldPosition() + grabOffset;
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, new Vector3(newPosition.x, LiftHeight,newPosition.z), LiftSpeed * Time.deltaTime);
    }

	// Moves all objects to their positions.
	private void ServerUpdatePositions()
    {
		// Move source object to mouse.
		Vector3 newPosition = localPlayerMousePos + grabOffset;
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, new Vector3(newPosition.x, LiftHeight, newPosition.z), LiftSpeed *  Time.deltaTime);

        float bestDist = Mathf.Infinity;
        bestDstPoint = null;
		// Decide which snap point is best to snap to.
		var nearestPoint = FindNearestFreePoint(selectedObject.GetComponent<SnapFunctionality>(), ref bestDist);
        if (nearestPoint != null)
        {
            bestDstPoint = nearestPoint;

            Debug.DrawLine(bestDstPoint.transform.position, selectedObject.transform.position, Color.yellow);
        }
		// Move guideObject to best available position. If there is none, move it to source.
		if (bestDstPoint != null)
        {
            RpcUpdateGuide(new TransformStruct(selectedObject.transform.position - (selectedObject.transform.position - bestDstPoint.transform.position), selectedObject.transform.rotation));
			guideObject.transform.position = selectedObject.transform.position - (selectedObject.transform.position - bestDstPoint.transform.position) + previewLiftVector;
			guideObject.transform.rotation = selectedObject.transform.rotation;
        }
        else
        {
        	RpcUpdateGuide(new TransformStruct(sourceObject.transform.position, sourceObject.transform.rotation));
			guideObject.transform.position = sourceObject.transform.position + previewLiftVector;
			guideObject.transform.rotation = sourceObject.transform.rotation;
        }
    }
	// Method to send data from server to client about position of guideObject.
	[ClientRpc]
    public void RpcUpdateGuide(TransformStruct target)
    {
        if (isLocalPlayer)
        {
			guideObject.transform.position = target.Position + previewLiftVector;
            gameObject.transform.rotation = target.Rotation;
        }
    }
	// Checks all other snap points in the level and picks the best one.
	private SnapPointBase FindNearestFreePoint(in SnapFunctionality heldTrap, ref float bestDist)
    {
        List<SnapPointBase> snapPoints = new List<SnapPointBase>();
        var rooms = level.GetRooms();
		foreach (var room in rooms)
		{
			if (room.GetComponent<ItemSpawner>())
			{
				snapPoints.AddRange(room.GetComponent<ItemSpawner>().FindSnapPoints());
			}
		}
        SnapPointBase result = null;
        foreach (var snapPoint in snapPoints)
        {
            if (!CanBePlaced(heldTrap, snapPoint))
                continue;
            float curDist = (heldTrap.transform.position - snapPoint.transform.position).sqrMagnitude;
            if(curDist < SnapDistance && curDist < bestDist)
            {
                bestDist = curDist;
                result = snapPoint;
            }
        }
        return result;

    } 
	// Checks if the currently held item can be placed on a specific point
    bool CanBePlaced(SnapFunctionality heldTrap, SnapPointBase snapPoint)
    {
		if (cost > currency.CurrentCurrency)
			return false;

		else if (heldTrap.FakeItem)
		{
			var snap = snapPoint.GetComponent<ItemSnapPoint>();
			if (snap == null || snap.Used)
				return false;
		}
		else
		{
			var snap = snapPoint.GetComponent<TrapSnapPoint>();
			if (snap == null)
				return false;
			if (snap.Used)
				return false;
			if (snap.Floor && !heldTrap.Floor)
				return false;
			if (snap.Roof && !heldTrap.Roof)
				return false;
			if (snap.Wall && !heldTrap.Wall)
				return false;
		}
		return true;
    }

    private Vector3 MouseToWorldPosition()
	{
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.WorldToScreenPoint(selectedObject.transform.position).z;
		return Camera.main.ScreenToWorldPoint(mousePos);
	}
	// Sets the layer of a game object and all of its children
	private void SetLayerOnAll(GameObject obj, int layer)
	{
		foreach (Transform trans in obj.GetComponentsInChildren<Transform>(true))
		{
			trans.gameObject.layer = layer;
		}
	}
}
