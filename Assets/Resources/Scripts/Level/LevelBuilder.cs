﻿using System.Collections;
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
* Ludvig Björk Förare (190430)
*
* CONTRIBUTORS: 
* Filip Renman, Kristoffer Lundgren
*
* 2019-05-03 Krig added code to spawn items in a set amout of random rooms
* 
* 
* CLEANED
*/

public class LevelBuilder : NetworkBehaviour
{
	// Rooms with more than one door
	public List<GameObject> MultiDoorRooms;
	// Rooms with only one door.
	public List<GameObject> DeadEnds;
	// Puppeteer items.
	public List<GameObject> PuppeteerItems; 
	// Start and End rooms
	public GameObject StartRoom, EndRoom;
	// Door to be placed between rooms.
	public GameObject Door;

	public int RoomsToSpawnBeforeDeadEndRooms = 10;
	// Used for deciding how many rooms should have items
	public float PercentageOfRoomsWithItems;

	private List<GameObject> rooms;

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
		}
		GameObject.Find("LoadingScreen").SetActive(false);
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
						if(ownCollider.GetPosition() == placedCollider.GetPosition())
						{
							canBePlaced = false;
							break;
						}
						else if (Vector3.Distance(ownCollider.GetPosition(),placedCollider.GetPosition()) < 2)
						{
							Debug.LogWarning("Overlapping rooms! Room: " + room.name + " Distance: " + Vector3.Distance(ownCollider.GetPosition(),placedCollider.GetPosition()));
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
		//Code added to spawn items in a set amout of random roooms
		RoomInteractable[] rooms = parent.gameObject.GetComponentsInChildren<RoomInteractable>();
		int numberOfRooms = rooms.Length;
		int numberOfRoomsToSpawnItemsIn = Mathf.FloorToInt((PercentageOfRoomsWithItems/100) * parent.gameObject.GetComponentsInChildren<RoomInteractable>().Length);
		List<int> roomIndices = GetRandom(0,numberOfRooms, numberOfRoomsToSpawnItemsIn);
		int index = 0;
        foreach (RoomInteractable room in rooms)
        {
			index++;
            NetworkServer.Spawn(room.gameObject);
            room.transform.SetParent(parent.transform);

			if(roomIndices.Contains(index))
			{
				var spawner = room.GetComponent<ItemSpawner>();
				// Checks so that the room has an item spawner script.
				if(spawner != null)
				{
					// Spawns items in the room
					spawner.SpawnItems();
				}
			}
        }
		// Spawn Button and final door.
		var finalRoomInfo = GameObject.Find("EndRoom(Clone)").GetComponent<FinalButtonPlacer>();

		var spawnableButton = Instantiate(finalRoomInfo.Button, finalRoomInfo.ButtonNode.transform);
		NetworkServer.Spawn(spawnableButton);
		var spawnableDoor = Instantiate(finalRoomInfo.Door, finalRoomInfo.DoorNode.transform);
		NetworkServer.Spawn(spawnableDoor);

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

	public List<GameObject> GetRoomsForItem()
	{
		List<GameObject> result = new List<GameObject>();
		foreach (var roomTreeNode in FindObjectsOfType<RoomTreeNode>())
		{
			result.Add(roomTreeNode.gameObject);
		}
		return result;
	}

    // Connect doors if able
    public void ConnectDoorsInRoomIfPossible(GameObject room)
	{
		foreach (var ownDoor in room.GetComponentsInChildren<AnchorPoint>())
		{
			ownDoor.DisconnectDoor();
		}
		foreach (var ownDoor in room.GetComponentsInChildren<AnchorPoint>())
		{
			foreach (var placedDoor in parent.GetComponentsInChildren<AnchorPoint>())
			{
				if (placedDoor == null || ownDoor == placedDoor)
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

	//Krig added. Returns a list of random numbers in a given range.
	public List<int> GetRandom(int min, int max, int num)
	{
		List<int> result = new List<int>();
		for (int i = 0; i < num; i++)
		{
			int randomNum = Random.Range(min, max);
			// If the random number already exists, generate a new one
			if (result.Contains(randomNum))
			{
				i--;
				continue;
			}
			else
				result.Add(randomNum);
		}

		if (result.Count == num)
		{
			return result;
		}
		else
		{
			Debug.LogError("ERROR: List not correct size.");
			return null;
		}
	}
}
