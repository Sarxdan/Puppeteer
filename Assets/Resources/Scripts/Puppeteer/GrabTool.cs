using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTool : MonoBehaviour
{
	private GameObject level;

	private GameObject sourceObject;
	private GameObject selectedObject;
	private GameObject guideObject;

    void Start()
    {
		level = GameObject.Find("Level");
    }

    void Update()
    {
		if (selectedObject == null)
		{
			RaycastHit hit;
			// TODO: Add raycast limit?
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
			{
				GameObject hitObject = hit.transform.gameObject;
				if (hitObject.tag == "Connectable")
				{
					// Debug.Log("hejhoj");
				}
			}
			// send ray. Save object it hits.
			// If óbject is interactable. Call onraycastenter
			// If mousebutton is down. Call pickup.
		}
    }
}
