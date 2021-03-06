﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
* AUTHOR:
* Kristoffer Lundgren
*
* DESCRIPTION:
* This Script shows the fades the splash screen and pulsated the press any button text
*
* CODE REVIEWED BY:
*
*
* CONTRIBUTORS:
*
*
* CLEANED
*/
public class SplashScreenScript : MonoBehaviour
{
    public GameObject PressAnyKey;
    public GameObject SplashScreen;
    public GameObject FadeOverlay;
    public GameObject Pulsate;
    public GameObject Background;


    private Image fadeImage;
    private Image pulsateImage;
    private float pulsate;

    private float fadeSpeed = 0.5f;
    private bool ListenForInput = false;

    private static bool disableSplashScreen;
    // Start is called before the first frame update
    void Start()
    {
        fadeImage = FadeOverlay.GetComponent<Image>();
        pulsateImage = Pulsate.GetComponent<Image>();

        if (disableSplashScreen)
            gameObject.SetActive(false);
        else
            StartCoroutine(SplashScreenTimer());
    }

    void FixedUpdate()
    {
        if(ListenForInput)
        {
            Pulsate.SetActive(true);
            if(Input.anyKey)
            {
                Pulsate.SetActive(false);
                PressAnyKey.SetActive(false);
                SplashScreen.SetActive(false);
                FadeOverlay.SetActive(false);
                Background.SetActive(false);
                ListenForInput = false;
                disableSplashScreen = true;
            }
            pulsate += Time.fixedDeltaTime + 0.04f;
            pulsateImage.color = new Color(0,0,0, ((Mathf.Sin(pulsate)/1.3f) + 0.02f));
        }
        if(fadeImage.color.a > 0)
        {
            fadeImage.color = new Color(0,0,0, fadeImage.color.a - (Time.deltaTime * fadeSpeed));
        }



    }
    // Wait for three seconds before you can continue
    private IEnumerator SplashScreenTimer()
    {

        yield return new WaitForSecondsRealtime(3);
        ListenForInput = true;
        PressAnyKey.SetActive(true);
        
    }

}
