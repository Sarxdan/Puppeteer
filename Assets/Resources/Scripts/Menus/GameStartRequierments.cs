using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
