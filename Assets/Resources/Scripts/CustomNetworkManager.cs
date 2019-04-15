using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CustomNetworkManager : NetworkLobbyManager
{

    public Button StartButton;
    public GameObject playersContainer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        if (!allPlayersReady)
            StartButton.interactable = false;
        else
            StartButton.interactable = true;
    }

    public override void OnLobbyStartHost()
    {
        StartButton.gameObject.SetActive(true);
        StartButton.interactable = false;
        base.OnLobbyStartHost();
    }

    public override void OnLobbyClientEnter()
    {
        if (playersContainer.transform.childCount == 1)
            StartButton.gameObject.SetActive(false);

        base.OnLobbyClientEnter();
    }

    public override void OnLobbyStopHost()
    {
        base.OnLobbyStopHost();
    }

    public override void ServerChangeScene(string sceneName)
    {
        base.ServerChangeScene(sceneName);
    }
}
