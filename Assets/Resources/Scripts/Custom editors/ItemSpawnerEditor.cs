using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(ItemSpawner))]
public class ItemSpawnerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		ItemSpawner script = (ItemSpawner)target;
		if (GUILayout.Button("Find nodes"))
		{
			script.FindSnapPoints();
		}

	}
}
#endif