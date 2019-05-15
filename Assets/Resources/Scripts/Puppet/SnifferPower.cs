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
 */

public class SnifferPower : PowerupBase
{
	// Int used to describe how much of the currently selected color should be blended into the doors colors
	public int ColorAmount = 1;
	public Color OpenColor = Color.green;
	public Color LockedColor = Color.red;

	public override void OnActivate()
	{
		// Go through each door and update the color of each shader in the door
		foreach (var door in FindObjectsOfType<DoorComponent>())
		{
			foreach (var renderer in door.GetComponentsInChildren<Renderer>())
			{
				// Choose color depending on if the door is locked or not
				if (!door.Locked)
				{
					renderer.material.SetColor("Color_D2F3C594", OpenColor);
				}
				else
				{
					renderer.material.SetColor("Color_D2F3C594", LockedColor);
				}
				// For all renderers in door, start blending the colors to the chosen amount
				renderer.material.SetInt("Vector1_67A4DF5D", ColorAmount);
			}
		}
	}

	public override void OnComplete()
	{
		// Set the blending amount to 0 for all renderers to stop adding color.
		foreach (var door in FindObjectsOfType<DoorComponent>())
		{
			foreach (var renderer in door.GetComponentsInChildren<Renderer>())
			{
				renderer.material.SetInt("Vector1_67A4DF5D", 0);
			}
		}
	}
}
