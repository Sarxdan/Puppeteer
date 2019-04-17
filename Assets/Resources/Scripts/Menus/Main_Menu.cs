using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

/*
* AUTHOR:
* Filip Renman, Kristoffer Lundgren
*
* DESCRIPTION:
* Main menu specific script used to setup menu for networking
*
* CODE REVIEWED BY:
* Anton Jonsson 17/04-2019
*
* CONTRIBUTORS:
*/

public class Main_Menu : MonoBehaviour
{
    public Button HostGameButton;
    public Button JoinGameButton;
    public NetworkManager manager;
    public InputField Ip_Field;

    [Scene]  public string LobbyScene;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        HostGameButton.onClick.AddListener(HostGame);
        JoinGameButton.onClick.AddListener(JoinGame);

    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkClient.isConnected && !ClientScene.ready)
        {
            ClientScene.Ready(NetworkClient.connection);

            if (ClientScene.localPlayer == null)
            {
                ClientScene.AddPlayer();
            }
        }
    }

    void HostGame()
    {
        manager.StartHost();
    }

    void JoinGame()
    {
        manager.networkAddress = Ip_Field.text;
        manager.StartClient();
    }

    void Test()
    {
        SceneManager.LoadScene(LobbyScene.ToString());
    }
}
