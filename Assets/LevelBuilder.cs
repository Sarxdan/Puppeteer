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
		int index = 0;/*Random.Range(0, MultiDoorRooms.Count);*/
		while (MultiDoorRooms.Count > 0)
		{
			var instance = Instantiate(MultiDoorRooms[index], transform);
			instance.transform.position = new Vector3(0, 100, 0);
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
		var doors = startRoom.GetComponentsInChildren<AnchorPoint>();
		for (int i = 0; i < doors.Length; i++)
		{
			OpenDoorQueue.Enqueue(doors[i]);
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

					Vector3 diff = (opendoor.transform.position - door.transform.position);
					room.transform.position += diff;

					bool canBePlaced = true;
					foreach (BoxCollider collider in room.GetComponentsInChildren<BoxCollider>())
					{
						Collider[] o = Physics.OverlapBox(collider.transform.position, collider.size / 2);
						if (Physics.CheckBox(collider.transform.position, collider.size/2))
						{
							canBePlaced = false;
							break;
						}
					}

					if (!canBePlaced)
					{
						room.transform.position = new Vector3(0, 100, 0);
						continue;
					}

					foreach (var otherdoor in room.GetComponentsInChildren<AnchorPoint>())
					{
						if (otherdoor == door)
						{
							continue;
						}
						OpenDoorQueue.Enqueue(otherdoor);
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
