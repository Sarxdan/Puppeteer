using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * AUTHOR:
 * Filip Renman
 * 
 * 
 * DESCRIPTION:
 * Displays a icon for every puppet that have been added to the target list for the puppeteer
 * 
 * CODE REVIEWED BY:
 * Sandra Andersson 20/05-2019 
 * 
 * CONTRIBUTORS:
 * 
*/

public class PuppetTracker : MonoBehaviour
{
    // The UI Icons that will be used to mark the players
    public List<RawImage> Icons;
    // The puppets that are going to get an icon
    public List<Transform> Targets;
    // The puppeteers camera
    public Camera puppeteerCamera;
    // The puppeteers camera controller. Used to check if the player is controlling the puppeteer.
    private PuppeteerCameraController puppeteerCameraController;


    // Start is called before the first frame update
    void Start()
    {
        Invoke("Setup", 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (Targets.Count == 0)
            return;

        for (int i = 0; i < Targets.Count; i++)
        {
            var target = Targets[i];

            //Special case for gekko if he is invisible
            if (target.GetComponent<InvisibilityPower>().IsActive)
            {
                //Convert the targets position to 2d position based on the camera
                Vector3 coords = puppeteerCamera.WorldToScreenPoint(target.transform.position);
                //Set the icon's position to the new position
                Icons[i].transform.position = coords;
                //Set the alpha on the icon
                Icons[i].color = new Color(1, 1, 1, 0);
            }
            else if (target != null)
            {
                //Convert the targets position to 2d position based on the camera
                Vector3 coords = puppeteerCamera.WorldToScreenPoint(target.transform.position);
                //Set the icon's position to the new position
                Icons[i].transform.position = coords;
                //Set the alpha on the icon
                Icons[i].color = new Color(1, 1, 1, Mathf.Clamp(Mathf.Pow(2 * (puppeteerCamera.transform.position.y / puppeteerCameraController.FarCameraZoomLimit) - 0.4f, 3), 0, 1));
            }
            else
            {
                // remove invalid target
                Targets.RemoveAt(i);
                Icons[i].color = new Color(1, 1, 1, 0);
            }
        }
    }

    //Find how many people are in the game and 
    public void Setup()
    {
        puppeteerCameraController = puppeteerCamera.GetComponent<PuppeteerCameraController>();

        int i = 0;
        foreach (var puppet in GameObject.FindGameObjectsWithTag("Player"))
        {
            Targets.Add(puppet.transform);
            Icons[i].texture = puppet.GetComponent<RawImage>().texture;
            Icons[i].color = new Color(1, 1, 1, 0);
            i++;
        }
    }
}
