using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldPressButtonOnEnterPress : MonoBehaviour
{
    [Tooltip("The button that will be pressed when you press enter when the inputfield is selected")]
    public Button ButtonPressedOnEnter;

    private InputField field;

    void Start()
    {
        field = gameObject.GetComponent<InputField>();
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (field.isFocused && field.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            ButtonPressedOnEnter.onClick.Invoke();
        }
    }
}
