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
* Filip Renman
*
* CLEANED
*/
public class VideoScript : MonoBehaviour
{
    //The first frame of the background video
    [Tooltip("The picture that will be displayed untill the video is loaded")]
    public Texture FirstFrameImage;
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
		Image.enabled = true;
        VideoPlayer.Prepare();
        Image.texture = FirstFrameImage;
        while(!VideoPlayer.isPrepared)
        {
                yield return new WaitForSeconds(0.5f);
                break;
        }

        Image.texture = VideoPlayer.texture;
        VideoPlayer.Play();
    }
}
