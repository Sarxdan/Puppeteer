using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Mirror;

/*
* AUTHOR:
* Filip Renman
*
* DESCRIPTION:
* Ends the game if the timer runs out or if the puppets/puppeteer wins.
*
* CODE REVIEWED BY:
* Anton Jonnson 14/05-2019
s* 
* CONTRIBUTORS:
* 
*/

public class MatchTimer : NetworkBehaviour
{
	public int MatchLength;
	public Text TimeRemainingTextBox;
	public string TimePrintOut; 
	public int PostGameTime;
	public GameObject Canvas;
    public GameObject endGameCamera;
	public RectTransform TimeTracker;
    public FMOD.Studio.EventInstance music;

    public int numberOfPuppetsAlive;
	private int NumberOfPuppetsThatEscaped;
	private EndOfMatchCanvas endOfMatchScript;
	private bool gameEnded;
	private NetworkManager manager;

    private int Minutes = 0;
    private int Seconds = 0;

// Start is called before the first frame update
    void Start()
    {
	    numberOfPuppetsAlive = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>().lobbySlots.Count - 1;
	    NumberOfPuppetsThatEscaped = 0;
	    endOfMatchScript = Canvas.GetComponent<EndOfMatchCanvas>();
        Canvas.gameObject.SetActive(false);
	    gameEnded = false;
	    manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
		TimeRemainingTextBox = GameObject.Find("TimeLeft").GetComponent<Text>();
        if (isServer)
		    StartCoroutine("Timer", 1);

		if (GameObject.FindObjectOfType<LevelBuilder>().isLocalPlayer)
		{
			TimeTracker.position = new Vector3(TimeTracker.position.x, TimeTracker.position.y + 50, TimeTracker.position.z);
		}
    }
	private void Update()
	{
		TimeRemainingTextBox.text = TimePrintOut;
	}

	public IEnumerator Timer()
    {
		yield return new WaitForSeconds(5);

		//Convert match time to minutes and seconds
		for (int i = MatchLength; i > 0; i -= 60)
		    if (i >= 60)
			    Minutes++;
		    else
			    Seconds = i;

	    //Loop while time is remaining
	    while (Minutes > 0 || Seconds >= 0)
	    {
		    string minutesString, secondsString;
		    if (Minutes > 10)
			    minutesString = Minutes.ToString();
		    else
			    minutesString = Minutes.ToString("00");
		    if (Seconds > 10)
			    secondsString = Seconds.ToString();
		    else
			    secondsString = Seconds.ToString("00");

		    TimePrintOut = minutesString + ":" + secondsString;
			RpcUpdateTime(TimePrintOut);

            //Gives the puppets some time to load in.
		    yield return new WaitForSeconds(1);

		    MatchLength--;
		    Seconds--;
			
		    if (Seconds < 0 && Minutes > 0)
		    {
			    Minutes--;
			    Seconds = 59;
		    }

            //Check if a team have won

            //If a puppet have escaped and all other are dead, the puppets win
            if (numberOfPuppetsAlive <= 0 && NumberOfPuppetsThatEscaped != 0 && !gameEnded)
            {
                //End the game. Puppets wins
                gameEnded = true;
                RpcPuppetsWins(NumberOfPuppetsThatEscaped, Minutes, Seconds);
                MatchLength = 0;
                StartCoroutine("EndTimer");
                StopCoroutine("Timer");
            }

            //If no puppets are alive, the puppeteer wins
            else if (numberOfPuppetsAlive <= 0 && !gameEnded)
            {
                //End the game. Puppeteer wins
                gameEnded = true;
                RpcPuppeteerWins(numberOfPuppetsAlive, Minutes, Seconds);
                MatchLength = 0;
                StartCoroutine("EndTimer");
                StopCoroutine("Timer");
            }

            //Problem line
            numberOfPuppetsAlive = FindObjectsOfType<PlayerController>().Length;
        }

        //If the time runs out and one puppet have escaped. The puppets win
        if (NumberOfPuppetsThatEscaped >= 1)
        {
            //End the game. Puppets wins
            gameEnded = true;
            RpcPuppetsWins(NumberOfPuppetsThatEscaped, Minutes, Seconds);
            MatchLength = 0;
            StartCoroutine("EndTimer");
            StopCoroutine("Timer");
        }

        //If the time runs out. The puppeteer wins
        else
        {
            //End the game. Puppeteer wins
            gameEnded = true;
            RpcPuppeteerWins(numberOfPuppetsAlive, Minutes, Seconds);
            MatchLength = 0;
            StartCoroutine("EndTimer");
            StopCoroutine("Timer");
        }
    }

    //Show the endscreen and then go back to main menu after a while
    public IEnumerator EndTimer()
    {
	    while (PostGameTime >= 0)
	    {
		    PostGameTime--;
		    yield return new WaitForSeconds(1);
	    }

        music.release();
        if (isServer)
            manager.StopHost();
    }

    public void PuppetEscaped()
    {
	    NumberOfPuppetsThatEscaped += 1;
    }

    public void PuppetDied()
    {
	    numberOfPuppetsAlive -= 1;
    }

    [ClientRpc]
    public void RpcUpdateTime(string value)
    {
	    TimePrintOut = value;
    }

    //Puppeteer win. Show endscreen for all clients
    [ClientRpc]
    public void RpcPuppeteerWins(int puppetsRemaining, int minutes, int seconds)
    {
	    //Disable all the cameras in the scene
	    foreach (var camera in GetComponents<Camera>())
	    {
		    camera.enabled = false;
	    }

        foreach (var canvas in GetComponents<Canvas>())
        {
            canvas.enabled = false;
        }

        endGameCamera.SetActive(true);

        //Set postgame info
        endOfMatchScript.SetWinnerText("The Puppeteer wins!");
	    endOfMatchScript.SetTimeLeftInfoText(minutes.ToString("00") + ":" + seconds.ToString("00"));
	    endOfMatchScript.SetPuppetsAliveInfoText(puppetsRemaining.ToString());

        //Enable the "End of game Canvas"
        endOfMatchScript.gameObject.SetActive(true);
    }

    //Puppets win. Show endscreen for all clients. 
    [ClientRpc]
    public void RpcPuppetsWins(int puppetsEscaped, int minutes, int seconds)
    {
	    //Disable all the cameras in the scene
	    foreach (var camera in GetComponents<Camera>())
	    {
		    camera.enabled = false;
	    }

        foreach (var canvas in GetComponents<Canvas>())
        {
            canvas.enabled = false;
        }

        endGameCamera.SetActive(true);

        //Set postgame info
        endOfMatchScript.SetWinnerText("The Puppets wins!");
	    endOfMatchScript.SetTimeLeftInfoText(minutes.ToString("00") + ":" + seconds.ToString("00"));
        endOfMatchScript.SetPuppetsText("Puppets Escaped");
	    endOfMatchScript.SetPuppetsAliveInfoText(puppetsEscaped.ToString());

	    //Enable the "End of game Canvas"
	    endOfMatchScript.gameObject.SetActive(true);
    }
}