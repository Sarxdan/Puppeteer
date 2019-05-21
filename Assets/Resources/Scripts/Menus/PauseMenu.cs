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
    // All the UI containers
    public GameObject MainUI;
    public GameObject MainOptionsUI;
    public GameObject AudioSettingsUI;
    public GameObject VideoSettingsUI;
    // The owner of the pause menu ui
    public Transform Owner;
    // Playercontroller is use for disabling the movement and the mouse camera control when paused
    private PlayerController playerController;
    private PuppeteerCameraController puppeteerController;
    // Used for disabling the item grab tool when paused
    private ItemGrabTool itemGrabTool;
    // Used for disabling the grabTool when paused
    private GrabTool grabTool;
    // Start is called before the first frame update

    public GameObject itemHUD;
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
            itemGrabTool = Owner.GetComponentInChildren<ItemGrabTool>();
            grabTool = Owner.GetComponentInChildren<GrabTool>();
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
        MainUI.SetActive(true);
		paused = true;
		if (playerController != null)
			playerController.DisableInput = true;
		else
        {
			puppeteerController.DisableInput = true;
            grabTool.enabled = false;
            itemGrabTool.enabled = false;
            itemHUD.SetActive(false);
        }
    }
    // Turn off the UI and reenable the controls
    public void UnPause()
    {
        PauseMenuUI.SetActive(false);
        MainOptionsUI.SetActive(false);
        AudioSettingsUI.SetActive(false);
        VideoSettingsUI.SetActive(false);
        paused = false;
		if (playerController != null)
			playerController.DisableInput = false;
		else
        {
			puppeteerController.DisableInput = false;
            itemGrabTool.enabled = true;
            grabTool.enabled = true;
            itemHUD.SetActive(true);
        }
	}

    public void OnExit()
    {
        var temp = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>();
        if(temp == null)
        {
            Debug.LogError("No Network manager found: Does it have the right name?(NetworkManager)");
        }
        else
        {
            Owner.GetComponent<Music>().EndMatch();
            temp.StopHost();
        }


    }
}
