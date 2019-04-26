using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund
*
* DESCRIPTION:
* System to display tutorials if it's the first time it starts up.
*
* CODE REVIEWED BY:
* Sandra "Sanders" Andersson (25/4)
*
* CONTRIBUTORS: 
* 
*/

public class Tutorial : MonoBehaviour
{
	// Bool to used to to contain the playerpref "firsttime" (to store choices made).
	public bool FirstTime = true;
	// Index of displayed text and image.
	public uint Index = 0;
	// All buttons on all of the tutorial screen.
	public Button YesButton, NoButton, NextButton, PreviousButton, ExitTutButton, ResetButton;
	// The tutorial textbox.
	public Text TextBox;
	// The tutorial image.
	public Image Image;

	// List of assets to be displayed. 
	public TextAsset[] TutorialTexts;
	public Sprite[] TutorialImages;

    // adds listeners and checks if the tutorialscreen should be displayed.
    void Start()
    {
		// Used to reset the playerprefs (TEMP)
		//ResetButton.onClick.AddListener(ResetPlayerPrefs);

		if (PlayerPrefs.HasKey("firsttime"))
		{
			FirstTime = GetBool("firsttime");
		}

		if (!FirstTime)
		{
			GameObject.Find("Tutorial1").SetActive(false); 
		}

		YesButton.onClick.AddListener(Yes);
		NoButton.onClick.AddListener(No);
		NextButton.onClick.AddListener(Next);
		PreviousButton.onClick.AddListener(Previous);
    }
	// If Yes button is pressed load up the first text and image.
	public void Yes()
	{
		TextBox.text = TutorialTexts[Index].text;
		Image.sprite = TutorialImages[Index];
	}
	// If No button is pressed set the playerpref to false. (making the screen not show up next time)
	public void No()
	{
		SetBool(false);
	}
	// If Next button is pressed increment Index if not max and display new text and image.
	public void Next()
	{
		if (Index < TutorialTexts.Length - 1)
			Index++;
		else
			Index = 0;

		TextBox.text = TutorialTexts[Index].text;
		Image.sprite = TutorialImages[Index];
	}
	// If Previous button is pressed decrement Index if not 0 and display new text and image.
	public void Previous()
	{
		if (Index > 0)
			Index--;
		else
			Index = (uint)TutorialTexts.Length - 1;

		TextBox.text = TutorialTexts[Index].text;
		Image.sprite = TutorialImages[Index];
	}
	// If Reset button pressed set playerpref ("firsttime") to true.
	private void ResetPlayerPrefs()
	{
		SetBool(true);
	}


	// get and set of player prefs. (turning int to bool)
	public static void SetBool(bool state)
	{
		PlayerPrefs.SetInt("firsttime", state ? 1 : 0);
	}
	public static bool GetBool(string key)
	{
		return PlayerPrefs.GetInt(key) == 1;
	}
}
