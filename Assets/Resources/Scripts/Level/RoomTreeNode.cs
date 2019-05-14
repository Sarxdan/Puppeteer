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
    public bool InTree { get; set; } = false;

    // Update used to draw tree on level.
    void Update()
    {
		if (parent != null)
		{
			Debug.DrawLine(transform.position + new Vector3(0, 1, 0), parent.transform.position + new Vector3(0, 1, 0), Color.black);
		}
    }

	// Method to reduce the number of children connected to this node by as much as possible
	public void ReduceChildren()
	{
		InTree = false;

		foreach (RoomTreeNode child in children)
		{
			child.ConnectToOtherRoom();
			child.InTree = false;
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
				InTree = true;
				return true;
			}
		}
		return false;
	}

	// Special case of FindNewParent() method where all children must find a suitable parent for the tree to still be valid. Used to initiate the search.
	public bool CutBranch()
	{
		// Redoes search if one child finds a new parent because that new connected branch might allow the other childrens branches to find a new parent.
		bool redo = true;
		bool ret = true;
		while (redo)
		{
			redo = false;
			ret = true;
			foreach (RoomTreeNode child in children.ToArray())
			{
				if (child.InTree)
				{
					continue;
				}

				if (!child.FindNewParent())
				{
					ret = false;
				}
				else
				{
					redo = true;
				}
			}
		}
		return ret;
	}

	// Goes thorugh all connected doors and checks if they are connected to a branch that has not been cut off. If they are, set them as parent.
	public bool ConnectToOtherRoom()
	{
		foreach (AnchorPoint door in GetComponentsInChildren<AnchorPoint>())
		{
			if (door.Connected)
			{
				RoomTreeNode newNode = door.ConnectedTo.GetComponentInParent<RoomTreeNode>();
				if (newNode.InTree)
				{
					if (newNode.gameObject.name == "guideObject")
					{
						SetParent(FindObjectOfType<GrabTool>().currentNode); // Might not always find server script first (?)
					}
					else
					{
						SetParent(newNode);
					}
					InTree = true;
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
		InTree = false;
	}

	// Recursively sets the inTree bool to true for all children.
	public void ReconnectToTree()
	{
		InTree = true;
		foreach (RoomTreeNode child in children)
		{
			child.ReconnectToTree();
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
}