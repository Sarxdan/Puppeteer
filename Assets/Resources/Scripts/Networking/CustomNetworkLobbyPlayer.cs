using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
* 
*
* CONTRIBUTORS:
*/


public class CustomNetworkLobbyPlayer : NetworkLobbyPlayer
{
    [SyncVar]
    public bool PlayerIsReady;

    //[SyncVar(hook = nameof(SelectCharacter))]
    [SyncVar]
    public int SelectedCharacterIndex = -1;

    [Header("UI")]
    public Button ReadyButton;
    public Text ReadyText;

    void Start()
    {
        base.Start();
        DontDestroyOnLoad(this.gameObject);

        gameObject.transform.position = new Vector3((Index * 200) - 400, 200, 0);

        if (!isLocalPlayer)
        {
            ReadyButton.gameObject.SetActive(false);
        }

        ReadyButton.onClick.AddListener(delegate { CmdChangeReadyState2(); });
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerIsReady)
            ReadyText.text = "Ready";
        else
            ReadyText.text = "Not Ready";
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        NetworkLobbyManager lobby = NetworkManager.singleton as NetworkLobbyManager;

        if (lobby != null && SceneManager.GetActiveScene().name == lobby.LobbyScene)
        {
            gameObject.transform.SetParent(GameObject.Find("Players").transform);
        }
    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();
    }
    #region Commands
    [Command]
    public void CmdChangeReadyState2()
    {
        PlayerIsReady = !PlayerIsReady;
        CustomNetworkManager lobby = CustomNetworkManager.singleton as CustomNetworkManager;
        lobby?.PlayerReadyStatusChanged();
    }

    public void ChangeSelectedCharacter(int index)
    {
        CmdChangeSelectedCharacter(index);
    }

    [Command]
    public void CmdChangeSelectedCharacter(int index)
    {
        SelectedCharacterIndex = index;
        RpcSelectCharacter(index);
    }
    #endregion

    [ClientRpc]
    public void RpcSelectCharacter(int index)
    {
        Debug.Log("Big oof");
        GameObject.Find("CharacterSelecter").GetComponent<CharacterSelect>().CharacterSelected(index, Index.ToString());
    }

    public void HideCanvas()
    {
        RpcHideCanvas();
    }

    [ClientRpc]
    public void RpcHideCanvas()
    {
        gameObject.transform.SetParent(null);
        GameObject.Find("Canvas").SetActive(false);
    }


}