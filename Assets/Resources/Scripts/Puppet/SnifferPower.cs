using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnifferPower : PowerupBase
{
	public override void OnActivate()
	{
		foreach (var door in FindObjectsOfType<DoorComponent>())
		{
			foreach (var renderer in door.GetComponentsInChildren<Renderer>())
			{
				if (!door.Locked)
				{
					renderer.material.SetColor("Color_D2F3C594", Color.green);
				}
				else
				{
					renderer.material.SetColor("Color_D2F3C594", Color.red);
				}
				renderer.material.SetInt("Vector1_67A4DF5D", 1);
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
