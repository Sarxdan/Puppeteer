using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Anton Jonsson
 * 
 * DESCRIPTION:
 * Script used to spawn and despawn decals
 * 
 * CODE REVIEWED BY:
 * 
 * CONTRIBUTORS:
 * Sandra Andersson
 */

public class DecalHandler : MonoBehaviour
{
	public int MaxDecalAmount = 50;
	public bool DecalDecay = true;
	[HideInInspector]
	public float DecayTime = 10;
	private Queue<GameObject> decalQueue = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
		if (DecalDecay)
		{
			StartCoroutine("DecayDecals");
		}
    }

	public void AddDecal(GameObject decal)
	{
		decalQueue.Enqueue(decal);

		if (decalQueue.Count > MaxDecalAmount)
		{
			Destroy(decalQueue.Dequeue());
		}
	}

	public IEnumerator DecayDecals()
	{
		while (true)
		{
			// Destroy first decal in queue
			if (decalQueue.Count > 0)
			{
				Destroy(decalQueue.Dequeue());
			}

			yield return new WaitForSeconds(DecayTime);
		}
	}
}
