using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/*
 * AUTHOR:
 * Filip Renman
 * 
 * 
 * DESCRIPTION:
 * Displays a icon for every puppet that have been added to the target list for the puppeteer
 * 
 * CODE REVIEWED BY:
 * Anton Jonsson (16/05-2019)
 * 
 * CONTRIBUTORS:
 * 
*/

public class PuppetTrackerIcon : MonoBehaviour
{
    // size of displayed icons
    public static readonly float IconSize = 50.0f;
    // How much further up the icon is going to be from the puppet
    public static readonly float IconHeight = 3;
    // The puppets that are going to get an icon
    public List<Transform> Targets;
    // The puppeteers camera
    private Camera puppeteerCamera;
    // The puppeteers camera controller. Used to check if the player is controlling the puppeteer.
    private PuppeteerCameraController puppeteerCameraController;
    // Half the icon size.
    private float halfIconSize;

    // Start is called before the first frame update
    void Start()
    {
        halfIconSize = IconSize / 2;
        puppeteerCamera = GameObject.FindGameObjectWithTag("GameController").GetComponentInChildren<Camera>();
        puppeteerCameraController = puppeteerCamera.GetComponent<PuppeteerCameraController>();

        //If we are not the puppeteer then we disable this script
        if (!GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            enabled = false;
            return;
        }
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (gameObject.tag == "player")
            return;

        //Check every target
        for (int i = 0; i < Targets.Count; i++)
        {
            var target = Targets[i];
            if (target != null)
            {
                //Convert the targets position to 2d position based on the camera
                Vector3 coords = puppeteerCamera.WorldToScreenPoint(target.transform.position + new Vector3(0, IconHeight, 0));
                //Check how far the camera is from the target and fade it the closer the camera is to the target.
                GUI.color = new Color(1, 1, 1, Mathf.Clamp(Mathf.Pow(2*(puppeteerCamera.transform.position.y/puppeteerCameraController.FarCameraZoomLimit)-0.4f, 3), 0, 1));
                //Draw the icon.
                GUI.DrawTexture(new Rect(coords.x - halfIconSize, Screen.height - coords.y - halfIconSize - IconHeight, IconSize, IconSize), target.GetComponent<RawImage>().texture, ScaleMode.ScaleToFit, true);
                //Reset the alpha
                GUI.color = new Color(1, 1, 1, 1);
            }
            else
            {
                // remove invalid target
                Targets.RemoveAt(i);
            }
        }
    }

    // registers a new tracked target for the puppeteer
    public void AddTarget(in Transform target)
    {
        Debug.Assert(target.GetComponent<RawImage>() != null, "Compass targets requires an icon");

        if (!Targets.Contains(target))
        {
            Targets.Add(target);
        }
    }

    // unregisters a tracked target
    public void RemoveTarget(in Transform target)
    {
        if (Targets.Contains(target))
        {
            Targets.Remove(target);
        }
    }
}
