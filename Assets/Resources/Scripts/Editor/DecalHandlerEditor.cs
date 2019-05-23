using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 *CLEANED
 */

#if UNITY_EDITOR
[CustomEditor(typeof(DecalHandler))]
public class DecalHandlerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DecalHandler script = (DecalHandler)target;

		DrawDefaultInspector(); 

		if (script.DecalDecay)
		{
			script.DecayTime = EditorGUILayout.FloatField("Decay Time", script.DecayTime);
		}
	}
}
#endif