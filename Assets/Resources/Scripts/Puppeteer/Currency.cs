using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Update is called once per frame
    void Update()
    {
        
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
