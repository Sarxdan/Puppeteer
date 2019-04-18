using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTreeNode : MonoBehaviour
{
	private RoomTreeNode parent;
	private bool inTree = true;

	private List<RoomTreeNode> children = new List<RoomTreeNode>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (parent !=  null)
		{
			Debug.DrawLine(transform.position + new Vector3(0, 1, 0), parent.transform.position + new Vector3(0, 1, 0), Color.black);
		}
    }

	public bool FindNewParent()
	{
		if (ConnectToOtherRoom())
		{
			return true;
		}

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

	public bool CutBranch()
	{
		foreach (RoomTreeNode child in children)
		{
			if (!child.FindNewParent())
			{
				return false;
			}
		}
		return true;
	}

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

	public void DisconnectFromTree()
	{
		foreach (RoomTreeNode child in children)
		{
			child.DisconnectFromTree();
		}
		inTree = false;
	}

	public void ReconnectToTree()
	{
		inTree = true;
		foreach (RoomTreeNode child in children)
		{
			child.ReconnectToTree();
		}
	}

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
