using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreenScript : MonoBehaviour
{
    public GameObject PressAnyKey;
    public GameObject SplashScreen;
    public GameObject FadeOverlay;
    public GameObject Pulsate;


    private Image fadeImage;
    private Image pulsateImage;
    private float fade = 1;
    private float pulsate;

    private float fadeSpeed = 0.5f;
    private float alpha = 1.0f;
    private bool ListenForInput = false;
    // Start is called before the first frame update
    void Start()
    {
        fadeImage = FadeOverlay.GetComponent<Image>();
        pulsateImage = Pulsate.GetComponent<Image>();
        StartCoroutine(SplashScreenTimer());
    }

    void Update()
    {
        if(ListenForInput)
        {
            if(Input.anyKeyDown)
            {
                PressAnyKey.SetActive(false);
                SplashScreen.SetActive(false);
                FadeOverlay.SetActive(false);
                ListenForInput = false;
            }
            Pulsate.SetActive(true);
            pulsate += Time.deltaTime + 0.05f;
            pulsateImage.color = new Color(0,0,0, ((Mathf.Sin(pulsate)/1.3f) + 0.02f));
        }
        if(fadeImage.color.a > 0)
        {
            fadeImage.color = new Color(0,0,0, fadeImage.color.a - (Time.deltaTime * fadeSpeed));
        }



    }

    // Update is called once per frame

    private IEnumerator SplashScreenTimer()
    {

        yield return new WaitForSecondsRealtime(3);
        ListenForInput = true;
        PressAnyKey.SetActive(true);
        
    }

}
