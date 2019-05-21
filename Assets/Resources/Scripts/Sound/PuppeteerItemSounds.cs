using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the following objects prefabs for sound:
 * Puppeteer Traps
 * Puppeteer Enemies
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class PuppeteerItemSounds : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Pickup;
    [FMODUnity.EventRef] public string s_Place;
    public Vector3 pupCamPos;

    public void Start()
    {
        pupCamPos = GameObject.FindGameObjectWithTag("GameController").GetComponentInChildren<Camera>().transform.position;
    }

    public void Pickup()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Pickup, pupCamPos);
    }

    public void Place()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Place, pupCamPos);
    }
}
