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
	public int ColorAmount;
	public Color OpenColor = Color.green;
	public Color LockedColor = Color.red;

	public override void OnActivate()
	{
		foreach (var door in FindObjectsOfType<DoorComponent>())
		{
			foreach (var renderer in door.GetComponentsInChildren<Renderer>())
			{
				if (!door.Locked)
				{
					renderer.material.SetColor("Color_D2F3C594", OpenColor);
				}
				else
				{
					renderer.material.SetColor("Color_D2F3C594", LockedColor);
				}
				renderer.material.SetInt("Vector1_67A4DF5D", ColorAmount);
			}
		}
	}

	public override void OnComplete()
	{
		foreach (var door in FindObjectsOfType<DoorComponent>())
		{
			foreach (var renderer in door.GetComponentsInChildren<Renderer>())
			{
				renderer.material.SetInt("Vector1_67A4DF5D", 0);
			}
		}
	}
}
