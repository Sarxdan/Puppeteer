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
* OtherName McOtherNameson
*
* CONTRIBUTORS:
*/

public class LevelBuilder : NetworkBehaviour
{
	// Rooms with the more then one door
	public List<GameObject> MultiDoorRooms;
	// Rooms with only one door.
	public List<GameObject> DeadEnds;
	// Start and End rooms
	public GameObject StartRoom, EndRoom;		

	// List for storing RoomCollider scripts, used for checking if rooms are on the same position.
	private List<RoomCollider> roomColliderPositions = new List<RoomCollider>();
	// Queue with all doors not connected to other doors.
	private Queue<AnchorPoint> openDoorQueue = new Queue<AnchorPoint>();
	// Randomized list of rooms to be placed in level. Currently works as a queue.
	private List<GameObject> roomsToBePlaced = new List<GameObject>();

	// Randomize order of rooms and place them in level. Also some networking checks to only do this on server.
    void Start()
    {
		if (false) // TODO: change to "!isServer" when networking is done.
		{
			return;
		}

		RandomizeRooms();
		SpawnRooms();
    }

	// Randomizes the order of the rooms and puts them into roomsToBePlaced list.
	private void RandomizeRooms()
	{
		// Randomize a set amount of multidoor rooms first to avoid death by only dead ends.
		for (int i = 0; i < 10; i++)
		{
			int index = Random.Range(0, MultiDoorRooms.Count);
			var instance = Instantiate(MultiDoorRooms[index], transform);
			instance.transform.position = new Vector3(0, -100, 0);
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
				var instance = Instantiate(MultiDoorRooms[index], transform);
				// Move room out of the way.
				instance.transform.position = new Vector3(0, -100, 0);
				roomsToBePlaced.Add(instance);

				MultiDoorRooms.RemoveAt(index);
			}
			else
			{
				index -= MultiDoorRooms.Count;
				var instance = Instantiate(DeadEnds[index], transform);
				// Move room out of the way.
				instance.transform.position = new Vector3(0, -100, 0);
				roomsToBePlaced.Add(instance);

				DeadEnds.RemoveAt(index);
			}
		}

		// Add EndRoom to list last so it is placed last.
		var endRoom = Instantiate(EndRoom, transform);
		endRoom.transform.position = new Vector3(0, -100, 0);
		roomsToBePlaced.Add(endRoom);
	}

	// Spawns the rooms in the level.
	private void SpawnRooms()
	{
		// Place startroom before adding other rooms.
		var startRoom = Instantiate(StartRoom, transform);

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

			// TODO: Randomize order of doors
			// Goes through all doors in the first room in the list and try to attatch it to the first door in the doorqueue.
			foreach (var door in room.GetComponentsInChildren<AnchorPoint>())
			{
				// Find the angle requierd for the doors to line up, then rotate the hole room.
				float angle = Mathf.Round(-Vector3.Angle(opendoor.transform.forward, door.transform.forward) + 180);
				room.transform.Rotate(Vector3.up, angle);

				// Find the distance between the doors and move the room so the rooms match up.
				Vector3 diff = (opendoor.GetPosition() - door.GetPosition());
				room.transform.position += diff;

				// Go through all the RoomColliders in the room to be placed and check so no overlap with anyother RoomColliders exicts.
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
					continue;
				}

				// If no overlap is detected, set both doors to connected.
				door.Connected = true;
				opendoor.Connected = true;

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
							otherdoor.Connected = true;
							placedDoor.Connected = true;
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
}
