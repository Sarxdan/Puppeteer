using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;

/*
* AUTHOR:
* Filip Renman, Kristoffer Lundgren
*
* DESCRIPTION:
* Handles the pregame lobby, scene switching and the spawning of prefabs in the main game.
*
* CODE REVIEWED BY:
* Anton Jonsson 17/04-2019
*
* CONTRIBUTORS:
*/


public class CustomNetworkManager : NetworkLobbyManager
{
    [Header("Custom elements")]
    public Button StartButton;
    public Button StartGameButton;
    public GameObject PlayersContainer;
    public GameObject[] PlayableCharacters;

    private int AmountOfPlayersLastUpdate = -1;


    void Start()
    {
        StartButton = GameObject.Find("StartCharacterSelectButton").GetComponent<Button>();
        StartButton.onClick.AddListener(HideCanvas);
        StartButton.onClick.AddListener(CloseLobby);
        StartButton.gameObject.SetActive(false);

        GameObject.Find("DisconnectButton").GetComponent<Button>().onClick.AddListener(StopHost);

        PlayersContainer = GameObject.Find("Players");
    }

    //Update is called once per frame
    public void Update()
    {
        if (SceneManager.GetActiveScene().name == "InGame")
            return;

        if (StartButton == null)
        {
            StartButton = GameObject.Find("StartCharacterSelectButton").GetComponent<Button>();
            StartButton.onClick.AddListener(HideCanvas);
            StartButton.gameObject.SetActive(false);

            GameObject.Find("DisconnectButton").GetComponent<Button>().onClick.AddListener(StopHost);

            PlayersContainer = GameObject.Find("Players");
        }

        if (!allPlayersReady)
            StartButton.interactable = false;
        else
            StartButton.interactable = true;

        if (AmountOfPlayersLastUpdate != lobbySlots.Count)
            PlayerReadyStatusChanged();

        AmountOfPlayersLastUpdate = lobbySlots.Count;
    }

    //Runs when a game lobby is created by the host
    public override void OnLobbyStartHost()
    {
        StartButton.gameObject.SetActive(true);
        StartButton.enabled = true;
        StartButton.interactable = false;
        base.OnLobbyStartHost();
    }

    //Runs when a client enters the lobby
    public override void OnLobbyClientEnter()
    {
        if (PlayersContainer.transform.childCount > 1)
            StartButton.gameObject.SetActive(false);

        base.OnLobbyClientEnter();
    }

    //Runs when the host stops hosting the lobby
    public override void OnLobbyStopHost()
    {
        base.OnLobbyStopHost();
    }

    //Changes the scene for everyone in the lobby and connected to the host
    public override void ServerChangeScene(string sceneName)
    {
        base.ServerChangeScene(sceneName);
    }

    //Hide the main menu canvas to reveal the character select screen 
    public void HideCanvas()
    {
        foreach (CustomNetworkLobbyPlayer item in lobbySlots)
        {
            if (item.isServer)
            {
                item.HideCanvas();
            }
        }

    }

    //Runs when a player changes ready status (Ready/Not Ready)
    internal void PlayerReadyStatusChanged()
    {
        int currentPlayers = 0;
        int readyPlayers = 0;

        foreach (CustomNetworkLobbyPlayer item in lobbySlots)
        {
            if (item != null)
            {
                currentPlayers++;
                if (item.PlayerIsReady)
                    readyPlayers++;
            }
        }

        if (currentPlayers == readyPlayers)
            allPlayersReady = true;
        else
            allPlayersReady = false;

        StartButton.interactable = allPlayersReady;
    }

    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn)
    {
        int prefabIndex = -1;

        foreach (CustomNetworkLobbyPlayer player in FindObjectsOfType<CustomNetworkLobbyPlayer>())
        {
            int connectionID = 0;
            NetworkIdentity ni = player.GetComponent<NetworkIdentity>();

            if (ni.connectionToClient != null)
            {
                connectionID = ni.connectionToClient.connectionId;
            }
            else if (ni.connectionToServer != null)
            {
                connectionID = ni.connectionToServer.connectionId;
            }

            if (connectionID == conn.connectionId)
            {
                prefabIndex = player.SelectedCharacterIndex;
            }
        }
            GameObject playerPrefab = (GameObject)Instantiate(PlayableCharacters[prefabIndex], PlayableCharacters[prefabIndex].transform.position, Quaternion.identity);
            return playerPrefab;
    }

    public void CloseLobby()
    {
        maxConnections = lobbySlots.Count;
    }
}
