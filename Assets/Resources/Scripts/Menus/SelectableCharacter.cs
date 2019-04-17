using UnityEngine;

/*
* AUTHOR:
* Filip Renman, Kristoffer Lundgren
*
* DESCRIPTION:
* This class makes a character able to be selected in the character select menu.
*
* CODE REVIEWED BY:
* Anton Jonsson 17/04-2019
*
* CONTRIBUTORS:
*/

public class SelectableCharacter : MonoBehaviour
{
    public int CharacterIndex;
    public int PlayerIndex;

    //Used to disable animations if a character is selected such as the spotlight turning on and off on hover.
    public bool Selected = false;

    [SerializeField]
    private Light spotlight;

    [SerializeField]
    private TextMesh text;

    void Start()
    {
        //-1 means that a character is not selected by a player
        PlayerIndex = -1;
        spotlight = GetComponentInChildren<Light>();
        text = GetComponentInChildren<TextMesh>();
    }

    //Enables the spotlight on hover. If the "Fire" input is pressed during hover, select the character.
    void OnMouseOver()
    {
        if(!Selected)
        {
            LightEnabled(true);
            if (Input.GetAxis("Fire") > 0)
            {
                foreach (CustomNetworkLobbyPlayer item in GameObject.FindObjectsOfType<CustomNetworkLobbyPlayer>())
                {
                    if (item.isLocalPlayer)
                    {
                        item.ChangeSelectedCharacter(CharacterIndex);
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
        }

    }

    public void LightEnabled(bool isEnabled)
    {
        spotlight.enabled = isEnabled;
    }

    public void ChangeNameTag(string name)
    {
        text.text = name;
    }
}
