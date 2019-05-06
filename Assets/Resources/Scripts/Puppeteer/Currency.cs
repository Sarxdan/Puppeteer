using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
 * 
 */

public class Currency : MonoBehaviour
{
	public int CurrencyIncrease;
	public int CurrentCurrency;
	public int TemporaryCurrency;

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
			yield return new WaitForSeconds(1);
		}
	}
}
