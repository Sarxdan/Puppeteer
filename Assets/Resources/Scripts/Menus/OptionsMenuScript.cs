using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class OptionsMenuScript : MonoBehaviour
{
    Resolution[] resolutions;
    public Dropdown dropdownMenu;
    public Toggle FullscreenToggleButton;


    void Start()
    {
        FullscreenToggleButton.isOn = Screen.fullScreen;
        FullscreenToggleButton.onValueChanged.AddListener(delegate { setFullscreen(); });


        dropdownMenu.GetComponent<Dropdown>().ClearOptions();
        resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (!dropdownMenu.GetComponent<Dropdown>().options.Contains(new Dropdown.OptionData(ResToString(resolutions[i]))))
            {
                dropdownMenu.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(ResToString(resolutions[i])));
                dropdownMenu.GetComponent<Dropdown>().value = i;
            }
        }
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
