using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
* AUTHOR:
* Filip Renman
*
* DESCRIPTION:
* Saves the value of a toggle button to toggle the mouse camera movement as a puppeteer
*
* CODE REVIEWED BY:
* Anton Jonsson (06/05/2019)
*
* CONTRIBUTORS:
*
*
*/

public class MouseCameraMovementToggle : MonoBehaviour
{
    private Toggle toggleButton;


    void Start()
    {
        toggleButton = gameObject.GetComponent<Toggle>();

        if (!PlayerPrefs.HasKey("MouseCameraMovementPuppeteer"))
            PlayerPrefs.SetInt("MouseCameraMovementPuppeteer", 1);

        if (PlayerPrefs.GetInt("MouseCameraMovementPuppeteer") == 1)
           toggleButton.isOn = true;
        else
            toggleButton.isOn = false;

        toggleButton.onValueChanged.AddListener(delegate { ToggleMouseCameraMovement(); });

    }

    public void ToggleMouseCameraMovement()
    {
        if (PlayerPrefs.GetInt("MouseCameraMovementPuppeteer") == 1)
        {
            toggleButton.isOn = false;
            PlayerPrefs.SetInt("MouseCameraMovementPuppeteer", 0);
        }
        else
        {
            toggleButton.isOn = true;
            PlayerPrefs.SetInt("MouseCameraMovementPuppeteer", 1);
        }
    }
}
