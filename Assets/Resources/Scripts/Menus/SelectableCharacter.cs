using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableCharacter : MonoBehaviour
{
    public int PlayerIndex;

    [SerializeField]
    private Light spotlight;

    [SerializeField]
    private Text text;

    void Start()
    {
        spotlight = GetComponentInChildren<Light>();
        text = GetComponentInChildren<Text>();
    }

    void OnMouseOver()
    {
        spotlight.enabled = true;
        if (Input.GetAxis("Fire") > 0)
        {
            gameObject.SetActive(false);
        }
    }

    void OnMouseExit()
    {
        spotlight.enabled = false;
    }
}
