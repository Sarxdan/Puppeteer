using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
* AUTHOR:
* Filip Renman
*
* DESCRIPTION:
* Script that makes the button it is attached to exit the game when pressed.
*
* CODE REVIEWED BY:
* Anton Jonsson 23/04/2019
*
* CONTRIBUTORS:
*
*
* CLEANED
*/

public class ExitButtonScript : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(delegate { ExitGame(); });
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}
