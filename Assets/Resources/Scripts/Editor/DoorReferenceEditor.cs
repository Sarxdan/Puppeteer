using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * DESCRIPTION:
 * An editor mod that adds an "update doors" button to DoorReferences components
 *
 * CODE REVIEWED BY:
 * 
 */

#if UNITY_EDITOR
[CustomEditor(typeof(DoorReferences))]
public class DoorReferenceEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DoorReferences script = (DoorReferences)target;
        if(GUILayout.Button("Update doors"))
        {
            script.GetDoors();
            EditorUtility.SetDirty(target);
        }

    }
}
#endif