using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
	public bool FirstTime = true;
	public uint Index = 0;
	public Button YesButton, NoButton, NextButton, PreviousButton, ExitTutButton;
	public Text TextBox;
	public Image Image;

	public TextAsset[] TutorialTexts;
	public Sprite[] TutorialImages;

    // Start is called before the first frame update
    void Start()
    {
		YesButton.onClick.AddListener(Yes);
		NoButton.onClick.AddListener(No);
		NextButton.onClick.AddListener(Next);
		PreviousButton.onClick.AddListener(Previous);
		ExitTutButton.onClick.AddListener(Exit);

		if (PlayerPrefs.HasKey("firsttime"))
		{
			FirstTime = GetBool("firsttime");
		}

		if (!FirstTime)
		{
			GameObject.Find("Tutorial1 - Test").SetActive(false); 
		}

		//SetBool("firsttime", true);



    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Yes()
	{
		Debug.Log(TutorialTexts[Index].text);
		TextBox.text = TutorialTexts[Index].text;
		Image.sprite = TutorialImages[Index];
	}

	public void No()
	{
		SetBool("firsttime", false);
	}

	public void Next()
	{
		if (Index < TutorialTexts.Length - 1)
			Index++;
		else
			Index = 0;

		Debug.Log(TutorialTexts[Index].text);
		TextBox.text = TutorialTexts[Index].text;
		Image.sprite = TutorialImages[Index];
	}

	public void Previous()
	{
		if (Index > 0)
			Index--;
		else
			Index = (uint)TutorialTexts.Length - 1;

		TextBox.text = TutorialTexts[Index].text;
		Image.sprite = TutorialImages[Index];
	}
	public void Exit()
	{
	}



	public static void SetBool(string key, bool state)
	{
		PlayerPrefs.SetInt(key, state ? 1 : 0);
	}

	public static bool GetBool(string key)
	{
		return PlayerPrefs.GetInt(key) == 1;
	}
}
