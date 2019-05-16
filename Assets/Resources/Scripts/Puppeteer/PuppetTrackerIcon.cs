using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PuppetTrackerIcon : MonoBehaviour
{
    public static readonly float ScreenWidthRatio = 0.7f;
    // UI offset from top of the screen
    public static readonly float TopOffset = 30.0f;
    // size of displayed icons
    public static readonly float IconSize = 50.0f;

    public static readonly float IconHeight = 1;
    public List<Transform> Targets;
    private Camera puppeteerCamera;
    private PuppeteerCameraController puppeteerCameraController;
    private float halfIconSize;

    // Start is called before the first frame update
    void Start()
    {
        halfIconSize = IconSize / 2;
        puppeteerCamera = GameObject.FindGameObjectWithTag("GameController").GetComponentInChildren<Camera>();
        puppeteerCameraController = puppeteerCamera.GetComponent<PuppeteerCameraController>();
        if (!GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            this.enabled = false;
            return;
        }
    }

    // Update is called once per frame
    void OnGUI()
    {
        for (int i = 0; i < Targets.Count; i++)
        {
            var target = Targets[i];
            Vector3 coords = puppeteerCamera.WorldToScreenPoint(target.transform.position + new Vector3(0, IconHeight, 0));


            if (target != null)
            {
                GUI.color = new Color(1, 1, 1, Mathf.Clamp(Mathf.Pow(2*(puppeteerCamera.transform.position.y/puppeteerCameraController.FarCameraZoomLimit), 2), 0, 1));
                GUI.DrawTexture(new Rect(coords.x - halfIconSize, Screen.height - coords.y - halfIconSize - IconHeight, IconSize, IconSize), target.GetComponent<RawImage>().texture, ScaleMode.ScaleToFit, true);
                GUI.color = new Color(1, 1, 1, 1);
            }
            else
            {
                // remove invalid target
                Targets.RemoveAt(i);
            }
        }
    }

    // registers a new tracked target in the compass
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
