using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund, Anton "Knugen" Jonsson
*
* DESCRIPTION:
* Script used for each node in a tree. The script allows for recursive method calls throughout the tree.
*
* CODE REVIEWED BY:
* Filip Renman (24/4/2019)
* 
*
* CONTRIBUTORS: 
*/

public class RoomTreeNode : MonoBehaviour
{

	private RoomTreeNode parent;
	private List<RoomTreeNode> children = new List<RoomTreeNode>();

	private bool inTree = false;

	// Update used to draw tree on level.
    void Update()
    {
		if (parent != null)
		{
			Debug.DrawLine(transform.position + new Vector3(0, 1, 0), parent.transform.position + new Vector3(0, 1, 0), Color.black);
		}
    }

	// Method that recursively goes through tree (depth first) to find a new suitable parent node for the part of the tree that has been cut off.
	public bool FindNewParent()
	{
		// If this is a suitable new parent node, return true.
		if (ConnectToOtherRoom())
		{
			return true;
		}

		// Go through each child and check if they are a suitable new parent. If they are, set them as your new parent and return true so you become the new parent of yor previous parent.
		foreach (RoomTreeNode child in children)
		{
			if (child.FindNewParent())
			{
				SetParent(child); 
				return true;
			}
		}
		return false;
	}

	// Special case of FindNewParent() method where all children must find a suitable parent for the tree to still be valid. Used to initiate the search.
	public bool CutBranch()
	{
		//
		//foreach (RoomTreeNode child in children.ToArray())
		//{
		//	child.FindNewParent();
		//}

		bool ret = true;
		foreach (RoomTreeNode child in children.ToArray())
		{
			if (!child.FindNewParent())
			{
				ret = false;
			}
		}
		return ret;
	}

	public void GlowBranch(Color color)
	{
		Glowable glow = GetComponent<Glowable>();
		if (glow != null)
		{
			glow.GlowColor = color;
			GetComponent<RoomInteractable>().OnRaycastEnter();
		}

		foreach (RoomTreeNode child in children.ToArray())
		{
			child.GlowBranch(new Color(color.r - 0.2f, color.g, color.b));
		}
	}

	// Goes thorugh all connected doors and checks if they are connected to a branch that has not been cut off. If they are, set them as parent.
	public bool ConnectToOtherRoom()
	{
		foreach (AnchorPoint door in GetComponentsInChildren<AnchorPoint>())
		{
			if (door.Connected)
			{
				RoomTreeNode newNode = door.ConnectedTo.GetComponentInParent<RoomTreeNode>();
				if (newNode.inTree)
				{
					SetParent(newNode);
					return true;
				}
			}
		}
		return false;
	}

	// Recursively sets the inTree bool to false for all children.
	public void DisconnectFromTree()
	{
		foreach (RoomTreeNode child in children)
		{
			child.DisconnectFromTree();
		}
		inTree = false;
	}

	// Recursively sets the inTree bool to true for all children.
	public void ReconnectToTree()
	{
		inTree = true;
		foreach (RoomTreeNode child in children)
		{
			child.ReconnectToTree();
		}
	}

	// Recursively resets glow values for all children
	public void ResetGlow()
	{
		foreach (RoomTreeNode child in children)
		{
			child.ResetGlow();
			Glowable glow = child.GetComponent<Glowable>();
			if (glow != null)
			{
				glow.GlowColor = Color.white;
				child.GetComponent<RoomInteractable>().OnRaycastExit();
			}
		}
	}

	// Switches parent.
	public void SetParent(RoomTreeNode parent)
	{
		if (this.parent != null)
		{
			this.parent.RemoveChild(this);
		}
		this.parent = parent;
		parent.AddChild(this);
	}

	public void AddChild(RoomTreeNode child)
	{
		children.Add(child);
	}

	public void RemoveChild(RoomTreeNode child)
	{
		children.Remove(child);
	}

	public RoomTreeNode GetParent()
	{
		return parent;
	}

	public bool InTree()
	{
		return inTree;
	}
}
