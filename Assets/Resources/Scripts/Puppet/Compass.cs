﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * The compass tracks and represents select objects in the level using icon textures.
 * A built-in horizontal user interface is used to display the compass content.
 * 
 * CODE REVIEWED BY:
 * 
 */
public class Compass : MonoBehaviour
{
    // compass width to screen ratio
    public static float ScreenWidthRatio = 0.8f;
    // UI offset from top of the screen
    public static float TopOffset = 40.0f;
    // size of displayed icons
    public static float IconSize = 24.0f;

    // contains all tracked entities
    public List<Transform> targets;

    void OnGUI()
    {
        foreach (Transform target in targets)
        {
            // calculate angle using only x- and z-axes
            Vector3 inv = transform.InverseTransformPoint(target.position);
            float angle = Mathf.Clamp(Mathf.Atan2(inv.x, inv.z), -1.0f, 1.0f);

            // calculate marker position and draw marker on screen
            float markerPos = Screen.width * 0.5f - IconSize * 0.5f + angle * Screen.width * ScreenWidthRatio * 0.5f;
            GUI.DrawTexture(new Rect(markerPos, TopOffset, IconSize, IconSize), target.GetComponent<RawImage>().texture, ScaleMode.ScaleToFit, true);
        }
    }

    // registers a new tracked target in the compass
    public void AddTarget(in Transform target)
    {
        Debug.Assert(target.GetComponent<RawImage>() != null, "Compass targets requires an icon");

        if(!targets.Contains(target))
        {
            targets.Add(target.transform);
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
