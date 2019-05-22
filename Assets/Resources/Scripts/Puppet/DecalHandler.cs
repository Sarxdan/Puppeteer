using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Anton Jonsson
 * 
 * DESCRIPTION:
 * Script used to spawn and despawn decals using a queue
 * 
 * CODE REVIEWED BY:
 * Ludvig Björk Förare (190515)
 * 
 * CONTRIBUTORS:
 * Sandra Andersson
 * 
 * CLEANED
 */

public class DecalHandler : MonoBehaviour
{
	public int MaxDecalAmount = 50;
	public bool DecalDecay = true;
	[HideInInspector]
	public float DecayTime = 10;
	private Queue<GameObject> decalQueue = new Queue<GameObject>();
	private bool decayEnabled = false;

	// Adds decal to queue and starts decay if not already started
	public void AddDecal(GameObject decal)
	{
		// Add decal last in queue
		decalQueue.Enqueue(decal);

		// If queue is too full, remove first item in queue
		if (decalQueue.Count > MaxDecalAmount)
		{
			Destroy(decalQueue.Dequeue());
		}
        // If this is the first decal in queue, start decay if enabled.
        else if (decalQueue.Count == 1)  
		{
			if (DecalDecay && !decayEnabled)
			{
				StartCoroutine("DecayDecals");
			}
		}
	}

	public IEnumerator DecayDecals()
	{
		while (true)
		{
			if (decayEnabled)
			{
				// Destroy first decal in queue
				if (decalQueue.Count > 0)
				{
					Destroy(decalQueue.Dequeue());
				}
                // If all decals have been destroyed, stop decay.
                if (decalQueue.Count == 0) 
				{
					decayEnabled = false;
					break;
				}
			}
			else
			{
				decayEnabled = true;
			}
			yield return new WaitForSeconds(DecayTime);
		}
	}
}
