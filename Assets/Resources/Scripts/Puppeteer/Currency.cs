using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * AUTHOR:
 * Benjamin "Boris" Vesterlund
 * 
 * DESCRIPTION:
 * Code to increment currency. sits on puppeteer prefab.
 * 
 * CODE REVIEWED BY:
 * Ludvig Björk Förare
 * 
 * CONTRIBUTORS:
 * Sandra Andersson (Currency UI)
 */

public class Currency : MonoBehaviour
{
	public int CurrencyIncrease;
	public int CurrentCurrency;     // The current currency of the puppeteer
	public int TemporaryCurrency;   // Current currency - the chosen item to place

    public Text CurrencyText;
	private FinalRoomInteract Gamestate;
	private bool firstTime = true;

    // Start is called before the first frame update
    void Start()
    {
		Gamestate = GetComponent<FinalRoomInteract>();
		StartCoroutine("CountUpCurrency");
    }



	public IEnumerator CountUpCurrency()
	{
		while (true)
		{
			if (Gamestate.ButtonPressed && firstTime)
			{
				CurrentCurrency += 500;
				CurrencyIncrease += 5;
				foreach (var room in GetComponent<LevelBuilder>().GetRoomsForItem())
				{
					foreach (var point in room.GetComponent<ItemSpawner>().FindSnapPoints())
					{
						if (point is ItemSnapPoint)
						{
							continue;
						}
						point.Used = false;
					}
				}
			}
			CurrentCurrency += CurrencyIncrease;
			TemporaryCurrency += CurrencyIncrease;
            CurrencyText.text = CurrentCurrency.ToString();
			yield return new WaitForSeconds(1);
		}
	}
}
