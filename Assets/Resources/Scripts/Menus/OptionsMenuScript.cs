using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/*
* AUTHOR:
* Filip Renman, Kristoffer Lundgren
*
* DESCRIPTION:
* The script that makes you able to change resolution and fullscreen.
* 
* 
* 
* CODE REVIEWED BY:
* Benjamin "Boris" Vesterlund 23/4/2019
*
* CONTRIBUTORS:
*
*
* CLEANED
*/

public class OptionsMenuScript : MonoBehaviour
{
    Resolution[] resolutions;
    public Dropdown dropdownMenu;
    public Toggle FullscreenToggleButton;

    private int currentResolutionIndex;

    void Start()
    {
        FullscreenToggleButton.isOn = Screen.fullScreen;
        FullscreenToggleButton.onValueChanged.AddListener(delegate { setFullscreen(); });

        int currentwidth = Screen.width;
        int currentHeight = Screen.height;

        dropdownMenu.GetComponent<Dropdown>().ClearOptions();

        //Get all the resolutions able for the screen 
        resolutions = Screen.resolutions;
        int index = 0;

        //Add options to the dropdown.
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (!dropdownMenu.GetComponent<Dropdown>().options.Contains(new Dropdown.OptionData(ResToString(resolutions[i]))))
            {
                dropdownMenu.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(ResToString(resolutions[i])));
                dropdownMenu.GetComponent<Dropdown>().value = i;

                //Set the current selected option to the current window size.
                if(resolutions[i].height == currentHeight && resolutions[i].width == currentwidth && resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
                    currentResolutionIndex = index;
                index++;

            }
        }

        dropdownMenu.GetComponent<Dropdown>().value = currentResolutionIndex;
        dropdownMenu.onValueChanged.AddListener(delegate { changeResolution(); });
    }

    string ResToString(Resolution res)
    {
        return res.width + " x " + res.height + " @" + res.refreshRate + "Hz" ;
    }

    void changeResolution()
    {
        Screen.SetResolution(resolutions[dropdownMenu.value].width, resolutions[dropdownMenu.value].height, false);
        Screen.fullScreen = FullscreenToggleButton.isOn;
    }

    void setFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
