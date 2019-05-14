using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

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
	private int SnapDistance = 10;
	// Maximum raycast ray length
	private float RaycastDistance = 500;
	// The lift height when grabbing an object
	private float LiftHeight = 3.0f;
	// The lift speed when grabbing an object
	private float LiftSpeed = 50.0f;
	//The distance off the groud that the preview trap is
	private float PreviewLiftHeight = 2.0f;

	public GameObject[] HudItems;

	public Button ButtonBearTrap, ButtonSpikeTrap, ButtonChandelier, ButtonFakeItem, ButtonMinionSpawner, ButtonSpawnTank;
	public GameObject BearTrap, SpikeTrap, Chandelier, FakeItem, MinionSpawner, SpawnTank;

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
		SpawnPuppeteerSpawnables();
    }

	public void BearTrapClick()
	{
		Pickup(BearTrap);
	}
	public void SpikeTrapClick()
	{
		Pickup(SpikeTrap);
	}
	public void ChandelierTrapClick()
	{
		Pickup(Chandelier);
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
			return;
        }
        else
		{
			// Send current mouseposition to server
			CmdUpdateMousePos(MouseToWorldPosition());

			if (Input.GetButtonUp("Fire"))
			{
				Drop();
			}

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
		sourceObject = Instantiate(pickupTrap);
		sourceObject.transform.position = new Vector3(0, -100, 0);
		// Instansiate the floating object
		selectedObject = Instantiate(sourceObject);

		selectedObject.transform.rotation = new Quaternion();
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.WorldToScreenPoint(new Vector3(0, LiftHeight, 0)).z;
		selectedObject.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
		selectedObject.name = "SelectedObject";
		selectedObject.GetComponent<BoxCollider>().enabled = false;
		// Handles the change in temporary currency. Can be used to show currency left after placement.
		cost = selectedObject.GetComponent<SnapFunctionality>().Cost;
		currency.TemporaryCurrency = currency.CurrentCurrency - cost;

		// Instansiate the guide object on the ground
		guideObject = Instantiate(selectedObject);
		guideObject.name = "GuideObject";
		guideObject.GetComponent<BoxCollider>().enabled = false;

		grabOffset = selectedObject.transform.position - MouseToWorldPosition();

        CmdUpdateMousePos(MouseToWorldPosition() + grabOffset);
		
        CmdPickup(pickupTrap);
    }

    [Command]
    public void CmdPickup(GameObject pickupTrap)
    {
		if (!isLocalPlayer)
		{
			sourceObject = Instantiate(pickupTrap);
			sourceObject.transform.position = new Vector3(0, -100, 0);
			selectedObject = Instantiate(sourceObject);
			selectedObject.name = "SelectedObject";
			selectedObject.transform.position = localPlayerMousePos;
			selectedObject.transform.localEulerAngles = new Vector3(0, 0, 0);
			selectedObject.GetComponent<BoxCollider>().enabled = false;

			// handels the change in temporary currency. can be used to show currency left after placement.
			cost = selectedObject.GetComponent<SnapFunctionality>().Cost;
			currency.TemporaryCurrency = currency.CurrentCurrency - cost;

			guideObject = Instantiate(sourceObject);
			guideObject.name = "GuideObject";
			guideObject.GetComponent<BoxCollider>().enabled = false;
		}

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

    public void Drop()
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

	// Method to update position of source object on server when drop happens.
	[Command]
    public void CmdDrop()
    {
		// Reset everything if trap is droped without a target place.
		if ((sourceObject.transform.position.x == guideObject.transform.position.x && sourceObject.transform.position.z == guideObject.transform.position.z && sourceObject.transform.rotation == guideObject.transform.rotation) || bestDstPoint == null)
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
			guideObject.GetComponent<BoxCollider>().enabled = true;
			// Set the layer on the item and all of its children in order to make it visible and interactable
			NetworkServer.Spawn(guideObject);
			RpcUpdateLayer(guideObject);
			SnapPointBase point = bestDstPoint.GetComponent<SnapPointBase>();
			point.Used = true;
			guideObject = null;
		}
    }

	[ClientRpc]
	public void RpcUpdateLayer(GameObject target)
	{
		SetLayerOnAll(target, 0);
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
		Vector3 newPosition = localPlayerMousePos;
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, new Vector3(newPosition.x, LiftHeight, newPosition.z), LiftSpeed * Time.deltaTime);

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
            RpcUpdateGuide(new TransformStruct(selectedObject.transform.position - (selectedObject.transform.position - bestDstPoint.transform.position) + previewLiftVector, selectedObject.transform.rotation));
			guideObject.transform.position = selectedObject.transform.position - (selectedObject.transform.position - bestDstPoint.transform.position) + previewLiftVector;
			guideObject.transform.rotation = selectedObject.transform.rotation;
        }
        else
        {
        	RpcUpdateGuide(new TransformStruct(new Vector3(0, -1000, 0), sourceObject.transform.rotation));
			guideObject.transform.position = new Vector3(0, -1000, 0);
			guideObject.transform.rotation = sourceObject.transform.rotation;
        }
    }
	// Method to send data from server to client about position of guideObject.
	[ClientRpc]
    public void RpcUpdateGuide(TransformStruct target)
    {
        if (isLocalPlayer && guideObject != null)
        {
			guideObject.transform.position = target.Position;
            guideObject.transform.rotation = target.Rotation;
        }
    }
	// Checks all other snap points in the level and picks the best one.
	private SnapPointBase FindNearestFreePoint(in SnapFunctionality heldTrap, ref float bestDist)
    {
        List<SnapPointBase> snapPoints = new List<SnapPointBase>();
        var rooms = level.GetRooms();
		foreach (var room in rooms)
		{
			if (room.GetComponent<ItemSpawner>() /*&& !room.GetComponent<RoomInteractable>().RoomContainsPlayer()*/)
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

	private void SpawnPuppeteerSpawnables()
	{

		GameObject localPlayer = gameObject;
		// Find LocalPlayer Puppeteer
		foreach (GrabTool grabToolScript in FindObjectsOfType<GrabTool>())
		{
			if (grabToolScript.isLocalPlayer)
			{
				localPlayer = grabToolScript.gameObject;
				break;
			}
		}

		Transform cameraTransform = localPlayer.GetComponentInChildren<Camera>().transform;
		Vector3[] pos = new Vector3[4];
		pos[0] = new Vector3(Screen.width - 100, Screen.height / 2 + 225, 20);
		pos[1] = new Vector3(Screen.width - 100, Screen.height / 2 + 75, 20);
		pos[2] = new Vector3(Screen.width - 100, Screen.height / 2 - 75, 20);
		pos[3] = new Vector3(Screen.width - 100, Screen.height / 2 - 225, 20);
		int i = 0;

		HudItems = new GameObject[level.PuppeteerItems.Count];

		foreach (var item in level.PuppeteerItems)
		{
			var spawnable = Instantiate(item, cameraTransform);
			spawnable.transform.position = Camera.main.ScreenToWorldPoint(pos[i]);
			spawnable.transform.eulerAngles = new Vector3(0, 0, 0);
			HudItems[i] = spawnable;
			i++;
		}

		//RpcSetParents();
	}

	[ClientRpc]
	public void RpcSetParents()
	{
		if (isLocalPlayer)
		{
			Transform cameraTransform = GetComponentInChildren<Camera>().transform;
			foreach (SnapFunctionality snappable in FindObjectsOfType<SnapFunctionality>())
			{
				snappable.transform.SetParent(cameraTransform);
			}
		}
	}
}
