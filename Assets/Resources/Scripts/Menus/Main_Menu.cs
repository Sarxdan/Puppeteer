using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

/*
* AUTHOR:
* Filip Renman
*
* DESCRIPTION:
* Main menu specific script used to setup menu for networking
*
* CODE REVIEWED BY:
* Anton Jonsson 17/04-2019
*
* CONTRIBUTORS:
*
*
* CLEANED
*/

public class Main_Menu : MonoBehaviour
{
    public Button HostGameButton;
    public Button JoinGameButton;
	public Button SearchGameButton;
	public Button PostOnLanButton;
	public Button SetServerNameButton;
    private NetworkManager manager;
	private CustomNetworkDiscovery discovery;
    public InputField Ip_Field;
	public InputField Servername_Field;

    [Scene]  public string LobbyScene;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
		discovery = GameObject.Find("NetworkManager").GetComponent<CustomNetworkDiscovery>();
		HostGameButton.onClick.AddListener(HostGame);
        JoinGameButton.onClick.AddListener(JoinGame);
		SearchGameButton.onClick.AddListener(SearchGame);
		PostOnLanButton.onClick.AddListener(PostOnLan);
		SetServerNameButton.onClick.AddListener(SetServerName);
        Cursor.lockState = CursorLockMode.None;

        if (PlayerPrefs.HasKey("LatestIPToConnectTo"))
            Ip_Field.text = PlayerPrefs.GetString("LatestIPToConnectTo");
    }

    void HostGame()
    {
        manager.StartHost();
    }

	void SetServerName()
	{
		discovery.broadcastData = Servername_Field.text;
	}
	void PostOnLan()
	{
		discovery.Initialize();
	}

	void JoinGame()
    {
        if (Ip_Field.text != "")
        {
            manager.networkAddress = Ip_Field.text;
            PlayerPrefs.SetString("LatestIPToConnectTo", Ip_Field.text);
        }
        else
        {
			manager.networkAddress = "localhost";
            PlayerPrefs.SetString("LatestIPToConnectTo", "localhost");
        }
        JoinGameButton.interactable = false;
        manager.StartClient();
    }

	void SearchGame()
	{
		discovery.Initialize();
	}
}
