using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * DESCRIPTION:
 * An editor mod that adds buttons for baking/clearing the data from a navmesh component
 *
 * CODE REVIEWED BY:
 * 
 */

[CustomEditor(typeof(NavMesh))]
public class NavMeshEditor : Editor
{
    private NavMesh script;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        script = (NavMesh)target;
        EditorGUI.BeginDisabledGroup(script.faces != null && script.faces.Length > 0);
        if(GUILayout.Button("Bake navmesh"))
        {
            script.BakeNavmesh();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(script.faces == null || script.faces.Length == 0);
        if(GUILayout.Button("Clear navmesh"))
        {
            script.ClearNavmesh();
        }
        EditorGUI.EndDisabledGroup();

    }
}