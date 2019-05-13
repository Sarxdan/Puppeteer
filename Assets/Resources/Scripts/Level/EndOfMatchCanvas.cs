using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndOfMatchCanvas : MonoBehaviour
{
    public Text WinnerText;
    public Text PuppetsAliveInfoText;
    public Text TimeLeftInfoText;


    public void SetWinnerText(string text)
    {
        WinnerText.text = text;
    }

    public void SetPuppetsAliveInfoText(string text)
    {
        PuppetsAliveInfoText.text = text;
    }

    public void SetTimeLeftInfoText(string text)
    {
        TimeLeftInfoText.text = text;
    }
}
