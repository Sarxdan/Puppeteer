using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Anton Jonsson
 * 
 * DESCRIPTION:
 * Changes the color of doors to display whether they are locked or not
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * CLEANED
 */

public class SnifferPower : PowerupBase
{
	// Int used to describe how much of the currently selected color should be blended into the doors colors
	public int FlashSpeed = 1;
	public Color LockedColor = Color.red;
	public Color OpenColor = Color.green;

	// Time between checks to see if the doors should change color during the powerups active time
	public float refreshTime = 0.5f;
	private bool activated = false;

	// Arrays to store doors affected by powerup and whether they were last locked or not in order to notice if they change mid powerup
	private DoorComponent[] doorArray;
	private bool[] lockedBoolArray;


	public override void OnActivate()
	{
		if (doorArray == null)
		{
			doorArray = FindObjectsOfType<DoorComponent>();
			lockedBoolArray = new bool[doorArray.Length];
		}

		// Go through each door and update the color of each shader in the door
		for (int i = 0; i < doorArray.Length; i++)
		{
			foreach (var renderer in doorArray[i].GetComponentsInChildren<Renderer>())
			{
				lockedBoolArray[i] = doorArray[i].Locked;

				// Choose color depending on if the door is locked or not
				if (doorArray[i].Locked)
				{
					renderer.material.SetColor("_Paw_Color", LockedColor);
				}
				else
				{
					renderer.material.SetColor("_Paw_Color", OpenColor);
				}
				// For all renderers in door, start blending the colors to the chosen amount
				renderer.material.SetInt("_Flash_Speed", FlashSpeed);
				renderer.material.SetInt("_Paw_On", 1);
			}
		}
		activated = true;
		StartCoroutine("UpdateDoorColors");
	}

	public override void OnComplete()
	{
		activated = false;

		// Set the blending amount to 0 for all renderers to stop adding color
		foreach (var door in FindObjectsOfType<DoorComponent>())
		{
			foreach (var renderer in door.GetComponentsInChildren<Renderer>())
			{
				renderer.material.SetInt("_Paw_On", 0);
			}
		}
	}
	
	// Check the doors to see if they have changed their locked state at regular intervals
	public IEnumerator UpdateDoorColors()
	{
		// Wait when this is first called since the doors were just updated
		yield return new WaitForSeconds(refreshTime);

		while (activated)
		{
			for (int i = 0; i < doorArray.Length; i++)
			{
				if (doorArray[i].Locked != lockedBoolArray[i])
				{
					// This door has changed its locked value so we update color
					foreach (var renderer in doorArray[i].GetComponentsInChildren<Renderer>())
					{
						if (doorArray[i].Locked)
						{
							renderer.material.SetColor("Color_D2F3C594", LockedColor);
						}
						else
						{
							renderer.material.SetColor("Color_D2F3C594", OpenColor);
						}
					}
					lockedBoolArray[i] = doorArray[i].Locked;
				}
			}
			yield return new WaitForSeconds(refreshTime);
		}
	}
}
