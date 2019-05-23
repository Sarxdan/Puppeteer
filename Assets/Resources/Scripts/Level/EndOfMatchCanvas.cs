using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *CLEANED
 */

public class EndOfMatchCanvas : MonoBehaviour
{
    public Text WinnerText;
    public Text PuppetsInfoText;
    public Text PuppetsAliveInfoText;
    public Text TimeLeftText;
    public Text TimeLeftInfoText;

    public void SetWinnerText(string text)
    {
        WinnerText.text = text;
    }

    public void SetPuppetsText(string text)
    {
        PuppetsInfoText.text = text;
    }

    public void SetPuppetsAliveInfoText(string text)
    {
        PuppetsAliveInfoText.text = text;
    }

    public void SetTimeLeftText(string text)
    {
        TimeLeftText.text = text;
    }

    public void SetTimeLeftInfoText(string text)
    {
        TimeLeftInfoText.text = text;
    }
}
