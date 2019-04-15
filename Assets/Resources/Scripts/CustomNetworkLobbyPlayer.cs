using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

public class CustomNetworkLobbyPlayer : NetworkLobbyPlayer
{
    [SyncVar]
    public bool ReadyToBegin2;

    [Header("UI")]
    public Button ReadyButton;
    public Text ReadyText;

    void Start()
    {
        base.Start();
        DontDestroyOnLoad(this.gameObject);

        gameObject.transform.position = new Vector3((Index * 200) - 200, 200, 0);

        if (!isLocalPlayer)
        {
            ReadyButton.gameObject.SetActive(false);
        }

        ReadyButton.onClick.AddListener(delegate { CmdChangeReadyState2(); });
    }

    // Update is called once per frame
    void Update()
    {
        if (ReadyToBegin2)
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

    public void SugMinKukMirror()
    {
        if (ReadyToBegin2)
            CmdChangeReadyState2();
        else
            CmdChangeReadyState2();
    }

    [Command]
    public void CmdChangeReadyState2()
    {
        if (!isServer)
            return;

        ReadyToBegin2 = !ReadyToBegin2;
    }
}