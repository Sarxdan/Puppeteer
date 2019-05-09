using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund, Anton "Knugen" Jonsson
*
* DESCRIPTION:
* Script to determine what rooms should do when interacted with.
*
* CODE REVIEWED BY:
* Filip Renman (24/4/2019)
*
* CONTRIBUTORS:
*/

public class RoomInteractable : Interactable
{
	public bool CanBePickedUp = true;

	public override void OnInteractBegin(GameObject interactor)
	{
		
	}

	public override void OnInteractEnd(GameObject interactor)
	{

	}

	// Returns true if there is a player in the room
	public bool RoomContainsPlayer()
	{
		foreach (BoxCollider collider in GetComponents<BoxCollider>())
		{
			foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
			{
				// Check if any player is within any collider on the room.
				if (collider.bounds.Contains(player.transform.position))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Kill all enemies in room. Used before room is moved.
	public void KillEnemiesInRoom()
	{
		foreach (BoxCollider collider in GetComponents<BoxCollider>())
		{
			foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
			{
				// Check if any player is within any collider on the room.
				if (collider.bounds.Contains(enemy.transform.position))
				{
					enemy.GetComponent<HealthComponent>().Damage(696969);
				}
			}
		}
	}

	public void MoveMinionsWithin(Vector3 deltaPos){
		if(EnemySpawner.AllMinions.Count == 0) return;
		foreach(BoxCollider collider in GetComponents<BoxCollider>()){
			foreach(StateMachine minion in EnemySpawner.AllMinions){
				if(collider.bounds.Contains(minion.transform.position)){
					minion.PathFinder.Stop();
					Debug.DrawRay(minion.transform.position, deltaPos, Color.red, 7);
					minion.transform.Translate(deltaPos);
				}
			}
		}
	}
}
