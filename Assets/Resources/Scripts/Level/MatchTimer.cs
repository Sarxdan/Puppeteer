using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MatchTimer : NetworkBehaviour
{
    public float TimeRemaining;
    public float PostGameTime;
    public GameObject Canvas;

    [SerializeField]
    private int numberOfPuppetsAlive;
    [SerializeField]
    private bool puppetEscaped;
    private EndOfMatchCanvas script;
    private bool gameEnded;
    private NetworkManager manager;

    // Start is called before the first frame update
    void Start()
    {
        numberOfPuppetsAlive = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>().lobbySlots.Count - 1;
        puppetEscaped = false;
        script = Canvas.GetComponent<EndOfMatchCanvas>();
        gameEnded = false;
        manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    void FixedUpdate()
    {
        if (!isServer)
            return;

        TimeRemaining -= Time.deltaTime;

        if (TimeRemaining <= 0 && !gameEnded || numberOfPuppetsAlive <= 0 && !gameEnded)
        {
            //End the game. Puppeteer wins
            gameEnded = true;
            RpcPuppeteerWins(numberOfPuppetsAlive, (int)TimeRemaining);
            TimeRemaining = 0;
        }
        else if (puppetEscaped && !gameEnded)
        {
            //End the game. Puppets wins
            gameEnded = true;
            RpcPuppetsWins(numberOfPuppetsAlive, (int)TimeRemaining);
            TimeRemaining = 0;
        }

        if (gameEnded && TimeRemaining < -PostGameTime && isServer)
        {
            manager.StopHost();
        }
    }

    public void PuppetEscaped()
    {
        puppetEscaped = true;
    }

    public void PuppetDied()
    {
        numberOfPuppetsAlive -= 1;
    }

    [ClientRpc]
    public void RpcPuppeteerWins(int puppetsRemaining, int timeLeft)
    {
        //Disable all the cameras in the scene
        foreach (var camera in GetComponents<Camera>())
        {
            camera.enabled = false;
        }

        //Set postgame info
        script.SetWinnerText("The Puppeteer wins!");
        script.SetTimeLeftInfoText((timeLeft / 60).ToString() + ":" + (timeLeft % 60).ToString());
        script.SetPuppetsAliveInfoText(puppetsRemaining.ToString());

        //Enable the "End of game camera"
        script.gameObject.SetActive(true);
    }

    [ClientRpc]
    public void RpcPuppetsWins(int puppetsRemaining, int timeLeft)
    {
        //Disable all the cameras in the scene
        foreach (var camera in GetComponents<Camera>())
        {
            camera.enabled = false;
        }

        //Set postgame info
        script.SetWinnerText("The Puppets wins!");
        script.SetTimeLeftInfoText((timeLeft / 60).ToString() + ":" + (timeLeft % 60).ToString());
        script.SetPuppetsAliveInfoText(puppetsRemaining.ToString());

        //Enable the "End of game camera"
        script.gameObject.SetActive(true);
    }
}
