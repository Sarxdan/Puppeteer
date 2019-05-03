using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Kristoffer Lundgren
 * 
 * DESCRIPTION:
 * Simple script for pausing and unpausing the game
 * 
 * 
 * CODE REVIEWED BY:
 * Benjamin "Boris" Vesterlund
 * CONTRIBUTORS:
 * 
*/
public class PauseMenu : MonoBehaviour
{
    // Stores the state of the pause menu
    private bool paused = false;
    // The UI for the pause menu
    public GameObject PauseMenuUI;
    // The owner of the pause menu ui
    public Transform Owner;
    // Playercontroller is use for disabling the movement and the mouse camera control when paused
    private PlayerController playerController;
    private PuppeteerCameraController puppeteerController;
    // Start is called before the first frame update
    void Start()
    {
		var temp = Owner.GetComponent<PlayerController>();
		if (temp != null)
		{
			playerController = temp;
		}
		else
		{
			puppeteerController = Owner.GetComponentInChildren<PuppeteerCameraController>();
		}
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the pause menu button is pressed
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(paused)
            {
                UnPause();
            }
            else
            {
                Pause();
            }
        }
    }
    // Turn on the UI and disable the controls
    void Pause()
    {
		PauseMenuUI.SetActive(true);
		paused = true;
		if (playerController != null)
			playerController.DisableInput = true;
		else
			puppeteerController.DisableInput = true;
    }
    // Turn off the UI and reenable the controls
    public void UnPause()
    {
        PauseMenuUI.SetActive(false);
        paused = false;
		if (playerController != null)
			playerController.DisableInput = false;
		else
			puppeteerController.DisableInput = false;
	}
}
