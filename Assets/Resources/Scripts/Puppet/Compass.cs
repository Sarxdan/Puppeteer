using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * The compass tracks and represents select objects in the level using icon textures.
 * A built-in horizontal user interface is used to display the compass content.
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 * CLEANED
 */

public class Compass : MonoBehaviour
{
    //Compass width to screen ratio
    public static readonly float ScreenWidthRatio = 0.7f;
    //UI offset from top of the screen
    public static readonly float TopOffset = 30.0f;
    //Size of displayed icons
    public static readonly float IconSize = 32.0f;

    //Contains all tracked entities
    public List<Transform> Targets;

    private Texture2D barTex;

    void Start()
    {
        barTex = Resources.Load<Texture2D>("Textures/HUD/Compass/Compass_Line");

        if(GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            this.enabled = false;
        }
    }

    void OnGUI()
    {
        for(int i = 0; i < Targets.Count; i++)
        {
            var target = Targets[i];

            if(target != null)
            {
                //Calculate angle using only x- and z-axes
                Vector3 inv = transform.InverseTransformPoint(target.position);
                float angle = Mathf.Clamp(Mathf.Atan2(inv.x, inv.z), -1.0f, 1.0f);

                float markerPos = Screen.width * 0.5f - IconSize * 0.5f + angle * Screen.width * ScreenWidthRatio * 0.5f;
                GUI.DrawTexture(new Rect(markerPos, TopOffset, IconSize, IconSize), target.GetComponent<RawImage>().texture, ScaleMode.ScaleToFit, true);
            }
            else
            {
                //Remove invalid target
                Targets.RemoveAt(i);
            }
        }
        GUI.DrawTexture(new Rect(Screen.width * 0.5f - Screen.width * ScreenWidthRatio * 0.5f, TopOffset + IconSize, Screen.width * ScreenWidthRatio, 2.0f), barTex, ScaleMode.StretchToFill, true);
    }

    //Registers a new tracked target in the compass
    public void AddTarget(in Transform target)
    {
        Debug.Assert(target.GetComponent<RawImage>() != null, "Compass targets requires an icon");

        if(!Targets.Contains(target))
        {
            Targets.Add(target);
        }
    }

    //Unregisters a tracked target
    public void RemoveTarget(in Transform target)
    {
        if(Targets.Contains(target))
        {
            Targets.Remove(target);
        }
    }
}
