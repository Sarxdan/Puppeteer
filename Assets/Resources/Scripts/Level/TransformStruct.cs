using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TransformStruct
{
	public Vector3 Position;
	public Quaternion Rotation;

	public TransformStruct(Vector3 pos, Quaternion rot)
	{
		Position = pos;
		Rotation = rot;
	}
}
