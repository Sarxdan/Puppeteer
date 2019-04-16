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
	public List<GameObject> MultiDoorRooms;		// Rooms with the more then one door
	public List<GameObject> DeadEnds;           // Rooms with only one door.
	private List<RoomCollider> roomColliderPositions = new List<RoomCollider>();
	public GameObject StartRoom, EndRoom;

	public Queue<AnchorPoint> OpenDoorQueue = new Queue<AnchorPoint>();
	public List<GameObject> roomsToBePlaced = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
		if (false) // TODO: change to "!isServer" when done.
		{
			return;
		}

		RandomizeRooms();
		SpawnRooms();



    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void RandomizeRooms()
	{
		
		while (MultiDoorRooms.Count > 0)
		{
			int index = Random.Range(0, MultiDoorRooms.Count);
			var instance = Instantiate(MultiDoorRooms[index], transform);
			instance.transform.position = new Vector3(0, -100, 0);
			roomsToBePlaced.Add(instance);
			
			MultiDoorRooms.RemoveAt(index);

		}
		while(DeadEnds.Count > 0)
		{
			//int index = Random.Range(0, DeadEnds.Count);
			//roomsToBePlaced.Add(DeadEnds[index]);
			//DeadEnds.RemoveAt(index);
		}
	}

	private void SpawnRooms()
	{
		var startRoom = Instantiate(StartRoom, transform);

		foreach (RoomCollider colliderPosition in startRoom.GetComponentsInChildren<RoomCollider>())
		{
			roomColliderPositions.Add(colliderPosition);
		}

		foreach (var door in startRoom.GetComponentsInChildren<AnchorPoint>())
		{
			OpenDoorQueue.Enqueue(door);
		}

		while(roomsToBePlaced.Count > 0)
		{
			var opendoor = OpenDoorQueue.Dequeue();
			foreach (var room in roomsToBePlaced)
			{

				foreach (var door in room.GetComponentsInChildren<AnchorPoint>())
				{
					float angle = Mathf.Round(-Vector3.Angle(opendoor.transform.forward, door.transform.forward) + 180);
					room.transform.Rotate(Vector3.up, angle);

					Vector3 diff = (opendoor.GetPosition() - door.GetPosition());
					room.transform.position += diff;

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

					if (!canBePlaced)
					{
						room.transform.position = new Vector3(0, -100, 0);
						continue;
					}

					door.Connected = true;
					opendoor.Connected = true;

					foreach (RoomCollider colliderPosition in room.GetComponentsInChildren<RoomCollider>())
					{
						roomColliderPositions.Add(colliderPosition);
					}

					foreach (var otherdoor in room.GetComponentsInChildren<AnchorPoint>())
					{
						if (otherdoor == door)
						{
							continue;
						}
						foreach (var placedDoor in OpenDoorQueue)
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
							OpenDoorQueue.Enqueue(otherdoor);
						}
					}
					roomsToBePlaced.Remove(room);
					break;
				}
				break;
			}
			//break;
		}
	}

}
