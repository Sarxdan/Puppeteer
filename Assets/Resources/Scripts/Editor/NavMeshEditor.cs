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
#if UNITY_EDITOR
[CustomEditor(typeof(NavMesh))]
public class NavMeshEditor : Editor
{
    private NavMesh script;
    
    public override void OnInspectorGUI()
    {
        script = (NavMesh)target;
        
        DrawDefaultInspector();

        if(script.Faces != null)
            GUILayout.Label("Navmesh face count: " + script.Faces.Length);
        else
            GUILayout.Label("Navmesh face count: null");
        EditorGUI.BeginDisabledGroup(script.Faces != null && script.Faces.Length > 0);
        if(GUILayout.Button("Bake navmesh"))
        {
            script.BakeNavmesh();
            EditorUtility.SetDirty(target);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(script.Faces == null || script.Faces.Length == 0);
        if(GUILayout.Button("Clear navmesh"))
        {
            script.ClearNavmesh();
            EditorUtility.SetDirty(target);
        }
        EditorGUI.EndDisabledGroup();

    }
}
#endif