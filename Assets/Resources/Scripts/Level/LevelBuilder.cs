using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund, Anton "Knugen" Jonsson
*
* DESCRIPTION:
* Script that randomizes the level and places the rooms in a order that is complient with the rules set in the technical specifications.
*
* CODE REVIEWED BY:
* Sandra "Sanders" Andersson (16/4)
* Filip Renman (24/4)
*
* CONTRIBUTORS: 
* Filip Renman, Kristoffer Lundgren
*/

public class LevelBuilder : NetworkBehaviour
{
	// Rooms with more than one door
	public List<GameObject> MultiDoorRooms;
	// Rooms with only one door.
	public List<GameObject> DeadEnds;
	// Start and End rooms
	public GameObject StartRoom, EndRoom;
	// Door to be placed between rooms.
	public GameObject Door;

	public int RoomsToSpawnBeforeDeadEndRooms = 10;

	// List for storing RoomCollider scripts, used for checking if rooms are on the same position.
	private List<RoomCollider> roomColliderPositions = new List<RoomCollider>();
	// Queue with all doors not connected to other doors.
	private Queue<AnchorPoint> openDoorQueue = new Queue<AnchorPoint>();
	// Randomized list of rooms to be placed in level. Currently works as a queue.
	private List<GameObject> roomsToBePlaced = new List<GameObject>();

	private RoomTreeNode startNode;

	private GameObject parent;

	// Randomize order of rooms and place them in level. Also some networking checks to only do this on server.
    void Start()
    {
		if (isServer)
		{
			parent = GameObject.Find("Level");
			RandomizeRooms();
			SpawnRooms();
			SpawnRoomsOnNetwork();
			RpcSetParents();
		}
	}

	// Randomizes the order of the rooms and puts them into roomsToBePlaced list.
	private void RandomizeRooms()
	{
		// Randomize a set amount of multidoor rooms first to avoid death by only dead ends.
		if (RoomsToSpawnBeforeDeadEndRooms > MultiDoorRooms.Count)
		{
			RoomsToSpawnBeforeDeadEndRooms = MultiDoorRooms.Count;
		}
		for (int i = 0; i < RoomsToSpawnBeforeDeadEndRooms; i++)
		{
			int index = Random.Range(0, MultiDoorRooms.Count);
			GameObject instance = Instantiate(MultiDoorRooms[index], parent.transform);
            instance.transform.position = new Vector3(0, -100, 0);
			SpawnDoors(instance);
			roomsToBePlaced.Add(instance);

			MultiDoorRooms.RemoveAt(index);
		}

		// Randomize the rest of the rooms together
		while (MultiDoorRooms.Count + DeadEnds.Count > 0)
		{
			// Create index over whole range of rooms left.
			int index = Random.Range(0, MultiDoorRooms.Count + DeadEnds.Count);

			if (index < MultiDoorRooms.Count)
			{
				var instance = Instantiate(MultiDoorRooms[index], parent.transform);
                // Move room out of the way.
                instance.transform.position = new Vector3(0, -100, 0);
				SpawnDoors(instance);
				roomsToBePlaced.Add(instance);

				MultiDoorRooms.RemoveAt(index);
			}
			else
			{
				index -= MultiDoorRooms.Count;
				var instance = Instantiate(DeadEnds[index], parent.transform);
                // Move room out of the way.
                instance.transform.position = new Vector3(0, -100, 0);
				SpawnDoors(instance);
				roomsToBePlaced.Add(instance);

				DeadEnds.RemoveAt(index);
			}
		}

		// Add EndRoom to list last so it is placed last.
		var endRoom = Instantiate(EndRoom, parent.transform);
		endRoom.transform.position = new Vector3(0, -100, 0);
		SpawnDoors(endRoom);
		roomsToBePlaced.Add(endRoom);
	}

	// Spawns the rooms in the level.
	private void SpawnRooms()
	{
		// Place startroom before adding other rooms.
		var startRoom = Instantiate(StartRoom, parent.transform);
		SpawnDoors(startRoom);
		startNode = startRoom.GetComponent<RoomTreeNode>();

		// Add all RoomCollider scripts in startroom into roomColliderPositions list.
		foreach (RoomCollider colliderPosition in startRoom.GetComponentsInChildren<RoomCollider>())
		{
			roomColliderPositions.Add(colliderPosition);
		}

		// Add all avalible doors from startroom into openDoorQueue.
		foreach (var door in startRoom.GetComponentsInChildren<AnchorPoint>())
		{
			openDoorQueue.Enqueue(door);
		}

		// Place all other rooms
		while(roomsToBePlaced.Count > 0)
		{
			var opendoor = openDoorQueue.Dequeue();
			var room = roomsToBePlaced[0];

			// Add doors to temporary list so they can be randomized.
			List<AnchorPoint> doorList = new List<AnchorPoint>();
			doorList.AddRange(room.GetComponentsInChildren<AnchorPoint>());

			// Goes through all doors in the first room in the list and try to attatch it to the first door in the doorqueue.
			while (doorList.Count > 0)
			{
				int index = Random.Range(0, doorList.Count);
				AnchorPoint	door = doorList[index];

				// Find the angle requierd for the doors to line up, then rotate the whole room.
				float angle = Mathf.Round(-Vector3.Angle(opendoor.transform.forward, door.transform.forward) + 180);
				room.transform.Rotate(Vector3.up, angle);

				// Find the distance between the doors and move the room so the rooms match up.
				Vector3 diff = (opendoor.GetPosition() - door.GetPosition());
				room.transform.position += diff;

				// Go through all the RoomColliders in the room to be placed and check so no overlap with any other RoomColliders exists.
				bool canBePlaced = true;
				foreach (RoomCollider ownCollider in room.GetComponentsInChildren<RoomCollider>())
				{
					foreach (RoomCollider placedCollider in roomColliderPositions)
					{
						if (ownCollider.GetPosition() == placedCollider.GetPosition())
						{
							canBePlaced = false;
							break;
						}
					}
				}
				// If any overlap is detected, move the room out of the way and start again.
				if (!canBePlaced)
				{
					room.transform.position = new Vector3(0, -100, 0);
					doorList.RemoveAt(index);
					continue;
				}

				// If no overlap is detected, set both doors to connected.
				opendoor.ConnectDoor(door);

				door.GetComponentInParent<RoomTreeNode>().SetParent(opendoor.GetComponentInParent<RoomTreeNode>());
				door.GetComponentInParent<RoomTreeNode>().ReconnectToTree();

				// Add all the RoomColliders in the new room to the roomColliderPositions List.
				foreach (RoomCollider colliderPosition in room.GetComponentsInChildren<RoomCollider>())
				{
					roomColliderPositions.Add(colliderPosition);
				}

				// Go through all the doors in the new room and add all available doors to the openDoorQueue.
				// Also check if two doors happen to connect, then set both as connected.
				foreach (var otherdoor in room.GetComponentsInChildren<AnchorPoint>())
				{
					if (otherdoor == door)
					{
						continue;
					}
					foreach (var placedDoor in openDoorQueue)
					{
						if (otherdoor.GetPosition() == placedDoor.GetPosition())
						{
							otherdoor.ConnectDoor(placedDoor);
							break;
						}
					}
					if (!otherdoor.Connected)
					{
						openDoorQueue.Enqueue(otherdoor);
					}
				}
				// Remove the room from roomsToBePlaced and break the loop, no need to check any more of the doors.
				roomsToBePlaced.Remove(room);
				break;
			}
			// If the room does not fit the opendoor, put the opendoor back in the end of the queue.
			if (!opendoor.Connected)
			{
				openDoorQueue.Enqueue(opendoor);
			}
		}
	}

    //Tells the network to spawn the rooms on every client
    private void SpawnRoomsOnNetwork()
    {
        foreach (RoomInteractable room in parent.gameObject.GetComponentsInChildren<RoomInteractable>())
        {
            NetworkServer.Spawn(room.gameObject);
            room.transform.SetParent(parent.transform);
        }

		foreach (DoorComponent door in FindObjectsOfType<DoorComponent>())
		{
			NetworkServer.Spawn(door.gameObject);
		}
    }

	// Returns a List of rooms.
	public List<GameObject> GetRooms()
	{
		List<GameObject> result = new List<GameObject>();
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			result.Add(parent.transform.GetChild(i).gameObject);
		}
		return result;
	}

	// Connect doors if able
	public void ConnectDoorsInRoomIfPossible(GameObject room)
	{
		foreach (var ownDoor in room.GetComponentsInChildren<AnchorPoint>())
		{
			ownDoor.DisconnectDoor();
			foreach (var placedDoor in parent.GetComponentsInChildren<AnchorPoint>())
			{
				if (ownDoor == placedDoor)
					continue;

				if (ownDoor.GetPosition() == placedDoor.GetPosition())
				{
					ownDoor.ConnectDoor(placedDoor);
					break;
				}
			}
		}
	}
	
	public RoomTreeNode GetStartNode()
	{
		if (startNode == null)
		{
			startNode = GameObject.Find("startRoom").GetComponent<RoomTreeNode>();
		}

		return startNode;
	}

	// Method to instantiate doors at all anchorpoints of a room.
	private void SpawnDoors(GameObject room)
	{
		foreach (AnchorPoint doorAnchor in room.GetComponentsInChildren<AnchorPoint>())
		{
			GameObject door = Instantiate(Door, doorAnchor.transform);
			DoorComponent doorScript = door.GetComponent<DoorComponent>();
			door.transform.localEulerAngles = new Vector3(0, 0, 0);
			door.transform.position = doorAnchor.transform.position + doorAnchor.transform.rotation * doorScript.adjustmentVector;
			doorScript.defaultAngle = 0;
			doorScript.Locked = true;

		}
	}

	public GameObject GetLevel()
	{
		return parent;
	}
	
	[ClientRpc]
	public void RpcSetParents()
	{
		parent = GameObject.Find("Level");

		// Move rooms into level GameObject
		foreach (RoomInteractable room in FindObjectsOfType<RoomInteractable>())
		{
			room.transform.SetParent(parent.transform);
		}

		// Move physical doors into room door GameObjects
		foreach (DoorComponent door in FindObjectsOfType<DoorComponent>())
		{
			foreach (AnchorPoint anchor in FindObjectsOfType<AnchorPoint>())
			{
				if ((anchor.transform.position - door.transform.position).magnitude <= 0.8f && Quaternion.Angle(anchor.transform.rotation, door.transform.rotation) <= 5)
				{
					door.transform.SetParent(anchor.transform);
					break;
				}
			}
		}

		// Connect Anchorpoints that are 
		foreach (AnchorPoint anchor in FindObjectsOfType<AnchorPoint>())
		{
			foreach (AnchorPoint otherAnchor in FindObjectsOfType<AnchorPoint>())
			{
				if (anchor != otherAnchor && anchor.GetPosition() == otherAnchor.GetPosition())
				{
					anchor.ConnectDoorClient(otherAnchor);
				}
			}
		}
	}

	public void BuildTree()
	{
		if (isLocalPlayer)
		{
			// Build tree on client
			GetStartNode().ReconnectToTree();
			GetStartNode().BuildTree();
		}
	}
}
