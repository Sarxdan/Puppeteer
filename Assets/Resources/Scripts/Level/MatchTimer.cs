using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Mirror;

public class MatchTimer : NetworkBehaviour
{
	public int TimeRemaining;
	public Text TimeRemainingTextBox;
	public int PostGameTime;
	public GameObject Canvas;

	[SerializeField]
	private int numberOfPuppetsAlive;
	[SerializeField]
	private int NumberOfPuppetsThatEscaped;
	private EndOfMatchCanvas script;
	private bool gameEnded;
	private NetworkManager manager;

    private int Minutes = 0;
    private int Seconds = 0;
    // Start is called before the first frame update
    void Start()
	{
		numberOfPuppetsAlive = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>().lobbySlots.Count - 1;
		NumberOfPuppetsThatEscaped = 0;
		script = Canvas.GetComponent<EndOfMatchCanvas>();
		gameEnded = false;
		manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
		StartCoroutine("Timer");
	}

	void FixedUpdate()
	{
		//if (!isServer)
		//	return;

		//if (NumberOfPuppetsThatEscaped >= numberOfPuppetsAlive && !gameEnded || TimeRemaining <= 0 && NumberOfPuppetsThatEscaped >= 1)
		//{
		//	//End the game. Puppets wins
		//	gameEnded = true;
		//	RpcPuppetsWins(numberOfPuppetsAlive, TimeRemaining);
		//	TimeRemaining = 0;
		//	StopCoroutine("Timer");
		//	StartCoroutine("EndTimer");
		//}
		//else if (TimeRemaining <= 0 && !gameEnded || numberOfPuppetsAlive <= 0 && !gameEnded)
		//{
		//	//End the game. Puppeteer wins
		//	gameEnded = true;
		//	RpcPuppeteerWins(numberOfPuppetsAlive, TimeRemaining);
		//	TimeRemaining = 0;
		//	StopCoroutine("Timer");
		//	StartCoroutine("EndTimer");
		//}

		if (gameEnded && PostGameTime < 0 && isServer)
		{
			StopCoroutine("EndTimer");
			manager.StopHost();
		}
	}

	public IEnumerator Timer()
	{
		for (int i = TimeRemaining; i > 0; i -= 60)
			if (i > 60)
				Minutes++;
			else
				Seconds = i;

		while (Minutes > 0 || Seconds > 0)
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

			string TimePrint = minutesString + ":" + secondsString;
			RpcUpdateTime(TimePrint);

			yield return new WaitForSeconds(1);

			TimeRemaining--;
			Seconds--;
			if (Seconds < 0)
			{
				Minutes--;
				Seconds = 59;
			}
		}
	}

    public IEnumerator EndTimer()
	{
		while (PostGameTime >= 0)
		{
			PostGameTime--;
			yield return new WaitForSeconds(1);
		}
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
	public void RpcUpdateTime(string timePrint)
	{
		TimeRemainingTextBox.text = timePrint;
	}

	[ClientRpc]
	public void RpcPuppeteerWins(int puppetsRemaining, int minutes, int seconds)
	{
		//Disable all the cameras in the scene
		foreach (var camera in GetComponents<Camera>())
		{
			camera.enabled = false;
		}

		//Set postgame info
		script.SetWinnerText("The Puppeteer wins!");
		script.SetTimeLeftInfoText(minutes.ToString("00") + ":" + seconds.ToString("00"));
		script.SetPuppetsAliveInfoText(puppetsRemaining.ToString());

		//Enable the "End of game camera"
		script.gameObject.SetActive(true);
	}

	[ClientRpc]
	public void RpcPuppetsWins(int puppetsRemaining, int minutes, int seconds)
	{
		//Disable all the cameras in the scene
		foreach (var camera in GetComponents<Camera>())
		{
			camera.enabled = false;
		}

		//Set postgame info
		script.SetWinnerText("The Puppets wins!");
		script.SetTimeLeftInfoText(minutes.ToString("00") + ":" + seconds.ToString("00"));
		script.SetPuppetsAliveInfoText(puppetsRemaining.ToString());

		//Enable the "End of game camera"
		script.gameObject.SetActive(true);
	}
}