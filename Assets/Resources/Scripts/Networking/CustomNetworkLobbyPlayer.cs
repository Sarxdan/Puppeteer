﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

/*
* AUTHOR:
* Filip Renman, Kristoffer Lundgren
*
* DESCRIPTION:
* This class is created when a player joins/creates a lobby for a multiplayer game.
* The class stores variables that can be used later to manipulate the game. For example choose which character the player plays as.
*
* CODE REVIEWED BY:
* Anton Jonsson 17/04-2019
*
* CONTRIBUTORS:
*/

public class CustomNetworkLobbyPlayer : NetworkLobbyPlayer
{
    [SyncVar]
    public bool PlayerIsReady;

    [SyncVar]
    public int SelectedCharacterIndex = -1;

    [Header("UI")]
    public Button ReadyButton;
    public Text ReadyText;

    // Update is called once per frame
    void Update()
    {
        if (PlayerIsReady)
        {
            ReadyText.text = "Ready";
            ReadyText.color = new Color(0, 255, 0);
        }
        else
        {
            ReadyText.text = "Not Ready";
            ReadyText.color = new Color(255, 0, 0);
        }
    }

    //Runs when client connects to the lobby
    public override void OnStartClient()
    {
        base.OnStartClient();

        NetworkLobbyManager lobby = NetworkManager.singleton as NetworkLobbyManager;

        if (lobby != null && SceneManager.GetActiveScene().name == lobby.LobbyScene)
        {
            gameObject.transform.SetParent(GameObject.Find("Players").transform);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        GameObject.Find("ReadyButton").GetComponent<Button>().onClick.AddListener(delegate { ToggleReadyState(); });
        GameObject.Find("StartCharacterSelectButton").SetActive(true);
    }

    //Runs when the client enters the lobby
    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        //gameObject.transform.localPosition = new Vector3(-(Screen.width / 3), ((Screen.height / 3) ) - (Index * (Screen.height / 6)), 0);
        //gameObject.transform.position = new Vector3(-(Screen.width / 3), ((Screen.height / 3) ) - (Index * (Screen.height / 6)), 0);
        gameObject.transform.localPosition = new Vector3(0, -(Index * (Screen.height / 6)), 0);

        if (isLocalPlayer)
        {
            //GameObject.Find("ReadyButton").gameObject.SetActive(false);
            Debug.Log("Satan");
            GameObject.Find("ReadyButton").GetComponent<Button>().onClick.AddListener(delegate { ToggleReadyState(); });
            //ReadyButton.gameObject.SetActive(false);
        }

    }
    #region Commands

    [Command]
    public void CmdToggleReadyState()
    {
        PlayerIsReady = !PlayerIsReady;
        CustomNetworkManager lobby = CustomNetworkManager.singleton as CustomNetworkManager;
        lobby?.PlayerReadyStatusChanged();
    }


    [Command]
    public void CmdChangeSelectedCharacter(int index)
    {
        SelectedCharacterIndex = index;
        RpcSelectCharacter(index);
    }
    #endregion


    #region ClientRpc
    [ClientRpc]
    public void RpcHideCanvas()
    {
        gameObject.transform.SetParent(null);
        GameObject.Find("Canvas").SetActive(false);
    }

    [ClientRpc]
    public void RpcSelectCharacter(int characterIndex)
    {
        Debug.Log(characterIndex);
        //TODO Change Index.ToString() to be the players name
        GameObject.Find("CharacterSelecter").GetComponent<CharacterSelect>().CharacterSelected(characterIndex, Index.ToString(), Index);
    }
    #endregion

    public void HideCanvas()
    {
        RpcHideCanvas();
    }

    public void ChangeSelectedCharacter(int index)
    {
        CmdChangeSelectedCharacter(index);
    }

    public void ToggleReadyState()
    {
        if (isLocalPlayer)
        {
        CmdToggleReadyState();
        }
    }
}