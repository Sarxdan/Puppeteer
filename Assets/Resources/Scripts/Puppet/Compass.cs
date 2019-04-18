using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * 
 * 
 * CODE REVIEWED BY:
 * 
 */ 
public class Compass : MonoBehaviour
{
    public List<Transform> targets;

    void Update()
    {
        
    }

    void OnGUI()
    {
        foreach (Transform target in targets)
        {
            Vector3 inv = transform.InverseTransformPoint(target.position);
            float angle = Mathf.Clamp(Mathf.Atan2(inv.x, inv.z), -1.0f, 1.0f);

            GUI.Box(new Rect(Screen.width / 2 - 32 / 2 + angle * 200.0f, 40.0f, 32.0f, 32.0f), GUIContent.none);
        }
    }

    // registers a new tracked target in the compass
    public void AddTarget(in Transform target)
    {
        if(!targets.Contains(target))
        {
            targets.Add(target);
        }
    }

    // unregisters a tracked target
    public void RemoveTarget(in Transform target)
    {
        if(targets.Contains(target))
        {
            targets.Remove(target);
        }
    }
}
