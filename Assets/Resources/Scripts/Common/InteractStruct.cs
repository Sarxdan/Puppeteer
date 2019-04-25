using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InteractStruct
{
	public GameObject Source;
	public GameObject Target;

	public InteractStruct(GameObject source, GameObject target)
	{
		Source = source;
		Target = target;
	}
}
