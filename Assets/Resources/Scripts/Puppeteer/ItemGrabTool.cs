using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund, Kristoffer "Krig" Lundgren
* RE-AUTHERED 05-16
* Benjamin "Boris" Vesterlund
*
* DESCRIPTION:
* Tool used for grabbing and dropping traps and spawners as puppeteer. Also does checks to determine if drop is legal.
*
* CODE REVIEWED BY:
* Filip rehman 2019-05-07 
* Kristoffer "Krig" Lundgren 2019-05-20
*
* CONTRIBUTORS:
* Philip Stenmark, Anton "Knugen" Jonsson
*/

public class ItemGrabTool : NetworkBehaviour
{
    private LevelBuilder level;

	// The maximum distance for snapping modules
	private int SnapDistance = 60;
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
	public GameObject BearTrap, SpikeTrapFloor, SpikeTrapRoof, Chandelier, FakeItem, MinionSpawner, Tank;

	// The object which the player clicks on
	private GameObject sourceObject;
	// The object which floats around following the mouse
	private GameObject selectedObject;
	// The object which snaps to points in the map
	private GameObject guideObject;
	// The transform on the server.
	private TransformStruct placingTransform;
	// The parent object.
	private GameObject parent;
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
		SetPrices();
    }

	// Sets pricetag to correct value att start of scene.
	void SetPrices()
	{
		ButtonBearTrap.GetComponentInChildren<Text>().text = BearTrap.GetComponent<SnapFunctionality>().Cost.ToString();
		ButtonSpikeTrap.GetComponentInChildren<Text>().text = SpikeTrapFloor.GetComponent<SnapFunctionality>().Cost.ToString();
		ButtonChandelier.GetComponentInChildren<Text>().text = Chandelier.GetComponent<SnapFunctionality>().Cost.ToString();
		ButtonFakeItem.GetComponentInChildren<Text>().text = FakeItem.GetComponent<SnapFunctionality>().Cost.ToString();
		ButtonMinionSpawner.GetComponentInChildren<Text>().text = MinionSpawner.GetComponent<SnapFunctionality>().Cost.ToString();
		ButtonSpawnTank.GetComponentInChildren<Text>().text = Tank.GetComponent<SnapFunctionality>().Cost.ToString();
	}

	// Button presses on puppeteer UI.
	public void BearTrapClick()
	{
		Pickup(BearTrap);
	}
	public void SpikeTrapClick()
	{
		Pickup(SpikeTrapFloor);
	}
	public void ChandelierTrapClick()
	{
		Pickup(Chandelier);
	}
	public void FakeItemClick()
	{
		Pickup(FakeItem);
	}
	public void MinionSpawnerClick()
	{
		Pickup(MinionSpawner);
	}
	public void TankClick()
	{
		Pickup(Tank);
	}


    // Update is called once per frame
    void Update()
	{
		ClientUpdate();
    }

	// handels all input from the local player.
    private void ClientUpdate()
    {
        if (selectedObject == null)
			return;
        else
		{
			if (Input.GetButtonUp("Fire"))
			{
				Drop();
			}

			if (Input.GetButtonDown("Rotate"))
			{
				if (selectedObject.GetComponent<SnapFunctionality>().FloorSpike)
				{
					Destroy(selectedObject);
					selectedObject = null;
					Destroy(guideObject);
					guideObject = null;
					Destroy(sourceObject);
					sourceObject = null;

					Pickup(SpikeTrapRoof);
				}
				else if (selectedObject.GetComponent<SnapFunctionality>().RoofSpike)
				{
					Destroy(selectedObject);
					selectedObject = null;
					Destroy(guideObject);
					guideObject = null;
					Destroy(sourceObject);
					sourceObject = null;

					Pickup(SpikeTrapFloor);
				}
				else
					selectedObject.transform.RotateAround(selectedObject.transform.position, selectedObject.transform.up, 90);
			}

			ClientUpdatePositions();
        }
    }

	// Method used for picking up an object.
	private void Pickup(GameObject pickupTrap)
    {
		// Sets the SourceObject.
		sourceObject = Instantiate(pickupTrap);
		sourceObject.name = "sourceObject";
		sourceObject.transform.position = new Vector3(0, -100, 0);

        sourceObject.GetComponentInChildren<PuppeteerItemSounds>().Pickup();

        // Instansiate the floating object
        selectedObject = Instantiate(sourceObject);

		Vector3 mousePos = Input.mousePosition;
		mousePos.z = Camera.main.WorldToScreenPoint(new Vector3(0, LiftHeight, 0)).z;

		selectedObject.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
		selectedObject.name = "SelectedObject";
        selectedObject.GetComponent<Collider>().enabled = false;

		// Handles the change in temporary currency. Can be used to show currency left after placement.
		cost = selectedObject.GetComponent<SnapFunctionality>().Cost;
		currency.TemporaryCurrency = currency.CurrentCurrency - cost;

		// Instansiate the guide object on the ground
		guideObject = Instantiate(sourceObject);
		guideObject.name = "GuideObject";
        guideObject.GetComponent<Collider>().enabled = false;

        grabOffset = selectedObject.transform.position - MouseToWorldPosition();

		// tells the server what gameobject is picked up.
		CmdPickUp(pickupTrap.name);
    }

	// Server binding the guideobject to the correct object.
	[Command]
	public void CmdPickUp(String pickupTrapName)
	{
		if (isLocalPlayer)
			return;
		// Handels if guideobject is not null, make it null and only destroy it if not placed.
		if (guideObject != null)
		{
			if (!guideObject.GetComponent<SnapFunctionality>().Placed)
			{
				Destroy(guideObject);
			}
			guideObject = null;
		}
		// All cases of pickup, handled by string, because gameobject needs to be spawned before being able to be past as argument.
		if (pickupTrapName == "Bear Trap")
		{
			guideObject = Instantiate(BearTrap);
			guideObject.transform.position = new Vector3(0,-100,0);
		}
		else if (pickupTrapName == "Chandelier")
		{
			guideObject = Instantiate(Chandelier);
			guideObject.transform.position = new Vector3(0,-100,0);
		}
		else if (pickupTrapName == "Spike Floor")
		{
			guideObject = Instantiate(SpikeTrapFloor);
			guideObject.transform.position = new Vector3(0,-100,0);
		}
		else if (pickupTrapName == "Spike Roof")
		{
			guideObject = Instantiate(SpikeTrapRoof);
			guideObject.transform.position = new Vector3(0,-100,0);
		}
		else if (pickupTrapName == "Fake item spawner")
		{
			guideObject = Instantiate(FakeItem);
			guideObject.transform.position = new Vector3(0,-100,0);
		}
		else if (pickupTrapName == "MinionSpawner")
		{
			guideObject = Instantiate(MinionSpawner);
			guideObject.transform.position = new Vector3(0,-100,0);
		}
		else if (pickupTrapName == "TankSpawner")
        {
            guideObject = Instantiate(Tank);
			guideObject.transform.position = new Vector3(0,-100,0);
        }
        
		// Relay to all clients what object is picked up.
		RpcPickUp(pickupTrapName);
	}

	// Same as CmdPickUp but binds the guideobject on all clients.
	[ClientRpc]
	public void RpcPickUp(string pickupTrapName)
	{
		if (isServer || FindObjectOfType<ItemGrabTool>().enabled)
			return;

		// Handels if guideobject is not null, make it null and only destroy it if not placed.
		if (guideObject != null)
		{
			if (!guideObject.GetComponent<SnapFunctionality>().Placed)
			{
				Destroy(guideObject);
			}
			guideObject = null;
		}
		// All cases of pickup, handled by string, because gameobject needs to be spawned before being able to be past as argument.
		if (pickupTrapName == "Bear Trap")
		{
			guideObject = Instantiate(BearTrap);
		}
		else if (pickupTrapName == "Chandelier")
		{
			guideObject = Instantiate(Chandelier);
		}
		else if (pickupTrapName == "Spike Floor")
		{
			guideObject = Instantiate(SpikeTrapFloor);
		}
		else if (pickupTrapName == "Spike Roof")
		{
			guideObject = Instantiate(SpikeTrapRoof);
		}
		else if (pickupTrapName == "Fake item spawner")
		{
			guideObject = Instantiate(FakeItem);
		}
		else if (pickupTrapName == "MinionSpawner")
		{
			guideObject = Instantiate(MinionSpawner);
		}
		else if (pickupTrapName == "TankSpawner")
		{
			guideObject = Instantiate(Tank);
		}
	}

	// Checks all rooms for open nodes, finding the best available node and moves the guideObject to that location.
	// if no position is available or close enough
	private void  ClientUpdatePositions()
    {
		Vector3 newPosition = MouseToWorldPosition() + grabOffset;
		selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, new Vector3(newPosition.x, LiftHeight, newPosition.z), LiftSpeed * Time.deltaTime);

		float bestDist = Mathf.Infinity;
		bestDstPoint = null;

		var nearestPoint = FindNearestFreePoint(selectedObject.GetComponent<SnapFunctionality>(), ref bestDist);
		if (nearestPoint != null)
		{
			bestDstPoint = nearestPoint;
			Debug.DrawLine(bestDstPoint.transform.position, selectedObject.transform.position, Color.yellow);

			guideObject.transform.position = selectedObject.transform.position - (selectedObject.transform.position - bestDstPoint.transform.position) + previewLiftVector;
			guideObject.transform.rotation = selectedObject.transform.rotation;
		}
		else
		{
			guideObject.transform.position = sourceObject.transform.position;
			guideObject.transform.rotation = sourceObject.transform.rotation;
		}
	}

	// If Object is dropped. (Placed)
	public void Drop()
    {
		// If the guideobject is still on the sourceobject (no new node or position is found at the time of release) destroy all objects and reset. 
		if ((sourceObject.transform.position == guideObject.transform.position && sourceObject.transform.rotation == guideObject.transform.rotation) || bestDstPoint == null)
		{
			Destroy(selectedObject);
			selectedObject = null;
			Destroy(guideObject);
			guideObject = null;
			Destroy(sourceObject);
			sourceObject = null;
		}
		// Else draw money. set the room where the node is found into trap parent and place the gameobject correctly.
		else
		{
			currency.CurrentCurrency = currency.CurrentCurrency - cost;

            guideObject.GetComponentInChildren<PuppeteerItemSounds>().Place();

            guideObject.name = "Placed Trap";
			// sets the parent on server.
			CmdSetParent(bestDstPoint.GetComponentInParent<NetworkIdentity>().gameObject);
			guideObject.transform.position -= previewLiftVector;
			guideObject.GetComponent<SnapFunctionality>().Placed = true;
			guideObject.GetComponent<Collider>().enabled = true;

			SnapPointBase point = bestDstPoint.GetComponent<SnapPointBase>();
			point.Used = true;
			// send transform of the trap to the server.
			CmdSendTransform(new TransformStruct(guideObject.transform.position,guideObject.transform.rotation));
			// tell the server to place and spawn the trap.
			CmdDrop();

			// destroy all object used in calculations, exept guideobject if this happens to be server.
			Destroy(selectedObject);
			selectedObject = null;
			Destroy(sourceObject);
			sourceObject = null;
			if (!isServer)
			{
				Destroy(guideObject);
				guideObject = null;
			}
		}
    }

	// Tells the server who the perent room is.
	[Command]
	public void CmdSetParent(GameObject sentParent)
	{
		parent = sentParent;
		// relay the parent to all clients.
		RpcSetParent(sentParent);
	}

	// Server spawns the gameobject. placing it in the correct position.
	[Command]
    public void CmdDrop()
    {
		if (isServer)
		{
			NetworkServer.Spawn(guideObject);
			guideObject.transform.position = placingTransform.Position;
			guideObject.transform.rotation = placingTransform.Rotation;
			guideObject.transform.SetParent(parent.transform);
			// tells all clients to parent the gameobjects.
			RpcSetParent(parent);
			guideObject.GetComponent<SnapFunctionality>().Placed = true;
			guideObject.GetComponent<Collider>().enabled = true;
			// updates layers so the trap is visable to clients.
			RpcUpdateLayer(guideObject);
		}
    }


	// Tells server where the gameobject is going.
	[Command]
	public void CmdSendTransform(TransformStruct transformStruct)
	{
		if (isServer)
		{
			placingTransform = transformStruct;
		}
	}

	// Calls all clients and tells them to change layer of gameobject.
	[ClientRpc]
	public void RpcUpdateLayer(GameObject target)
	{
		SetLayerOnAll(target, 0);
	}

	// Calls all but the puppeteer to tell dem to parent the gameobjects.
	[ClientRpc]
	public void RpcSetParent(GameObject sentParent)
	{
		if (FindObjectOfType<ItemGrabTool>().enabled)
		{
			return;
		}
		guideObject.transform.SetParent(sentParent.transform);
	}

	// Checks all other snap points in the level and picks the best one.
	private SnapPointBase FindNearestFreePoint(in SnapFunctionality heldTrap, ref float bestDist)
    {
        List<SnapPointBase> snapPoints = new List<SnapPointBase>();
        var rooms = level.GetRoomsForItem();
		foreach (var room in rooms)
		{
			if (room.GetComponent<ItemSpawner>() && !room.GetComponent<RoomInteractable>().RoomContainsPlayer())
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
