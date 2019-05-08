using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
* AUTHOR:
* Filip Renman
*
* DESCRIPTION:
* Makes it so you can press enter while an inputfield is focuesd to press a button
*
* CODE REVIEWED BY:
* Anton Jonsson (05/08-2019)
*
* CONTRIBUTORS:
* 
*/


public class InputFieldPressButtonOnEnterPress : MonoBehaviour
{
    [Tooltip("The button that will be pressed when you press enter when the inputfield is selected")]
    public Button ButtonPressedOnEnter;

    private InputField field;

    //Prevents the client from joining a host 4 times.
    private bool beenPressed;

    void Start()
    {
        field = gameObject.GetComponent<InputField>();
        beenPressed = false;
    }

    //Might be called several times per frame (update).
    void OnGUI()
    {
        if (field.isFocused && field.text != "" && Input.GetKeyDown("return") && !beenPressed)
        {
            beenPressed = true;
            ButtonPressedOnEnter.onClick.Invoke();
        }
    }
}
