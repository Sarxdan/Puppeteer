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

    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine("CountUpCurrency");
    }

	public IEnumerator CountUpCurrency()
	{
		while (true)
		{
			CurrentCurrency += CurrencyIncrease;
			TemporaryCurrency += CurrencyIncrease;
            CurrencyText.text = CurrentCurrency.ToString();
			yield return new WaitForSeconds(1);
		}
	}
}
