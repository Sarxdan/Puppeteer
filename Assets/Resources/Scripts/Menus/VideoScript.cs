using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoScript : MonoBehaviour
{
    public RawImage Image;
    public VideoPlayer VideoPlayer;
    public GameObject SplashScreenContainer;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayVideo());
    }

    
    IEnumerator PlayVideo()
    {
        VideoPlayer.Prepare();
        while(!VideoPlayer.isPrepared)
        {
                yield return new WaitForSeconds(1);
                break;
        }

        Image.texture = VideoPlayer.texture;
        VideoPlayer.Play();
        SplashScreenContainer.GetComponent<SplashScreenScript>().StartListening();
    }
}
