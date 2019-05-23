using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
/*
* AUTHOR:
* Kristoffer Lundgren
*
* DESCRIPTION:
* This script is needed to play the video in the main menu
*
* CODE REVIEWED BY:
*
*
* CONTRIBUTORS:
*
*
* CLEANED
*/
public class VideoScript : MonoBehaviour
{
    // Image to play the video on
    public RawImage Image;
    // The Video player
    public VideoPlayer VideoPlayer;
    void Start()
    {
        StartCoroutine(PlayVideo());
    }

    
    IEnumerator PlayVideo()
    {
        VideoPlayer.Prepare();
        while(!VideoPlayer.isPrepared)
        {
                yield return new WaitForSeconds(0.5f);
                break;
        }

        Image.texture = VideoPlayer.texture;
        VideoPlayer.Play();
    }
}
