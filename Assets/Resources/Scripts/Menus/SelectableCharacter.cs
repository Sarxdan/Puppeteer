using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableCharacter : MonoBehaviour
{
    public int PlayerIndex;

    public bool Selected = false;

    [SerializeField]
    private Light spotlight;

    [SerializeField]
    private Text text;

    //Temporary stuff remove when making the finished menu.
    private bool clicked;

    void Start()
    {
        spotlight = GetComponentInChildren<Light>();
        text = GetComponentInChildren<Text>();
    }

    void OnMouseOver()
    {
        if(!Selected)
        {
            spotlight.enabled = true;
            if (Input.GetAxis("Fire") > 0)
            {
                foreach (CustomNetworkLobbyPlayer item in GameObject.FindObjectsOfType<CustomNetworkLobbyPlayer>())
                {
                    if (item.isLocalPlayer)
                    {
                        item.ChangeSelectedCharacter(PlayerIndex);
                    }
                }
            }
        }
        
    }

    void OnMouseExit()
    {
        if(!Selected)
        {
            LightEnabled(false);
            spotlight.enabled = false;
        }

    }

    public void LightEnabled(bool isEnabled)
    {
        spotlight.enabled = isEnabled;
    }
}
