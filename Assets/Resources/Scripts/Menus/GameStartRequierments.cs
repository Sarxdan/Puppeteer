using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
* AUTHOR:
* Filip Renman, Kristoffer Lundgren
*
* DESCRIPTION:
* Enables the start game button when all players in the lobby have selected a character and the puppeteer is selected
*
* CODE REVIEWED BY:
* Anton Jonsson 17/04-2019
*
* CONTRIBUTORS:
*/

public class GameStartRequierments : MonoBehaviour
{
    public CharacterSelect CharacterSelectScript;
    public SelectableCharacter Puppeteer;
    private CustomNetworkManager NetworkManager;
    private Button StartGameButton;

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>();
        StartGameButton = gameObject.GetComponent<Button>();
        StartGameButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (CharacterSelectScript.NumberOfSelectedCharacters == NetworkManager.lobbySlots.Count && Puppeteer.Selected)
            StartGameButton.interactable = true;
        else
            StartGameButton.interactable = false;
    }
}
