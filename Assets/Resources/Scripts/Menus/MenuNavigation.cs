using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

/*
* AUTHOR:
* Filip Renman, Benjamin "Boris" Vesterlund
*
* DESCRIPTION:
* Animations for the main menu
*
* CODE REVIEWED BY:
* 
*
* CONTRIBUTORS: 
* 
*/
//Cleaned

public class MenuNavigation : MonoBehaviour
{

	private int coroutinesRunning = 0;                      // "animation" cooldown. (so the game don't register button presses while panels are moving and breaking...)

    [Header("Menu Panels")]
    public GameObject OptionsPanel;							// the panel with options buttons on.
	public GameObject AudioOptionsPanel;					// the panel with options buttons on.
	public GameObject VideoOptionsPanel;					// the panel with options buttons on.
	public GameObject ControlsOptionsPanel;					// the panel with options button on.
    public GameObject JoinMenuPanel;
    public GameObject LobbyMenuPanel;
    public GameObject CreditsPanel;
	public GameState currentState = GameState.MainMenu;		// current state.


	[Header("Main Menu Buttons")]
	public Button JoinGameButton;
    public Button JoinGameIpButton;
	public Button HostGameButton;
	public Button CreditsButton;
	public Button OptionsButton;
	public Button ExitButton;

    [Header("Options Menu Buttons")]
    public Button VideoButton; public Text VideoText;
	public Button AudioButton; public Text AudioText;
	public Button ControlsButton; public Text ControlsText;

    [Header("Audio Menu Buttons")]
    public Slider MasterVolume; public Text MasterVolumeText;
	public Slider EffectsVolume; public Text EffectsVolumeText;
	public Slider MusicVolume; public Text MusicVolumeText;

    private bool buttonPressed = false;

	//----------------Enum----------------
	public enum GameState
	{
		MainMenu,
		JoinMenuStep1,
		LobbyMenu,
		HostMenu,
		Credits,
		Options,
		OptionsAudio,
		OptionsVideo,
		OptionsControls
	}


	// Start is called before the first frame update
	void Start()
	{
		JoinGameButton.onClick.AddListener(JoinGame);
        JoinGameIpButton.onClick.AddListener(HostGame);
		HostGameButton.onClick.AddListener(HostGame);
		CreditsButton.onClick.AddListener(Credits);
		OptionsButton.onClick.AddListener(Options);
			VideoButton.onClick.AddListener(VideoOptions);
			AudioButton.onClick.AddListener(AudioOptions);
			ControlsButton.onClick.AddListener(ControlsOptions);
		ExitButton.onClick.AddListener(Exit);
        
    }

	// Update is called once per frame
	void Update()
	{
        if (buttonPressed)
        {
            HostGame();
        }
	}

	void HostGame()
	{
        buttonPressed = true;
		    if (coroutinesRunning == 0 && currentState != GameState.LobbyMenu)
		    {
                buttonPressed = false;
			    ChangeState(GameState.LobbyMenu);
		    }
	}

	void JoinGame()
	{
        if (coroutinesRunning == 0 && currentState != GameState.JoinMenuStep1)
        {
            ChangeState(GameState.JoinMenuStep1);
        }
    }

	void Nickname()
	{
	}

    void Credits()
    {
        if (coroutinesRunning == 0 && currentState != GameState.Credits)
        {
            ChangeState(GameState.Credits);
        }
    }

	void Options()
	{
		if (coroutinesRunning == 0 && (currentState != GameState.Options && currentState != GameState.OptionsAudio && currentState != GameState.OptionsVideo && currentState != GameState.OptionsControls))
		{
			ChangeState(GameState.Options);
		}
	}

		void VideoOptions()
		{
			if (coroutinesRunning == 0 && currentState != GameState.OptionsVideo)
			{
				ChangeState(GameState.OptionsVideo);
			}
		}
		void AudioOptions()
		{
			if (coroutinesRunning == 0 && currentState != GameState.OptionsAudio)
			{
			    ChangeState(GameState.OptionsAudio);
			}
		}
		void ControlsOptions()
		{
			if (coroutinesRunning == 0 && currentState != GameState.OptionsControls)
			{
				ChangeState(GameState.OptionsControls);
			}
		}

	void Exit()
	{

	}


	//------------------GameStates--------------------------
	public void ChangeState(GameState newstate)
	{
        currentState = newstate;
		StartCoroutine(newstate.ToString() + "State");
	}

	IEnumerator MainMenuState()
	{
		//Enter

		while (currentState == GameState.Options){ yield return null; } // running.

		//Exit
	}
	IEnumerator OptionsState()
	{
		//Enter
		// Get the target position of panel
		Vector3 relativeLocation = new Vector3(Screen.width/3, 0.0f, 0.0f);
		Vector3 targetLocation = OptionsPanel.transform.position + relativeLocation;
		float timeDelta = 0.1f;

		// Start coroutine
		StartCoroutine(SmoothMove(OptionsPanel, targetLocation, timeDelta));


		while (currentState == GameState.Options || currentState == GameState.OptionsAudio || currentState == GameState.OptionsVideo || currentState == GameState.OptionsControls) { yield return null; } // running.

		// Exit
		// Get the target position
		Vector3 relativeLocation2 = new Vector3(-Screen.width / 3, 0.0f, 0.0f);
		Vector3 targetLocation2 = OptionsPanel.transform.position + relativeLocation2;

		// Start coroutine
		StartCoroutine(SmoothMove(OptionsPanel, targetLocation2, timeDelta));
	}
	IEnumerator OptionsAudioState()
	{
		// Enter
		// Get the target position of panel
		Vector3 relativeLocation = new Vector3((Screen.width / 2), 0, 0);
		Vector3 targetlocation = AudioOptionsPanel.transform.position + relativeLocation;
		float timeDelta = 0.1f;

		// Start coroutine
		StartCoroutine(SmoothMove(AudioOptionsPanel, targetlocation, timeDelta));

		while (currentState == GameState.OptionsAudio) { yield return null; } // running.

		// Exit
		// Get the target position
		Vector3 relativeLocation2 = new Vector3(-(Screen.width / 2), 0, 0);
		Vector3 targetLocation2 = AudioOptionsPanel.transform.position + relativeLocation2;

		// Start coroutine
		StartCoroutine(SmoothMove(AudioOptionsPanel, targetLocation2, timeDelta));

	}
	IEnumerator OptionsVideoState()
	{
		// Enter
		// Get the target position of panel
		Vector3 relativeLocation = new Vector3((Screen.width / 2), 0, 0);
		Vector3 targetlocation = VideoOptionsPanel.transform.position + relativeLocation;
		float timeDelta = 0.1f;

		// Start coroutine
		StartCoroutine(SmoothMove(VideoOptionsPanel, targetlocation, timeDelta));


		while (currentState == GameState.OptionsVideo) { yield return null; } // running.

		// Exit
		// Get the target position
		Vector3 relativeLocation2 = new Vector3(-(Screen.width / 2), 0, 0);
		Vector3 targetLocation2 = VideoOptionsPanel.transform.position + relativeLocation2;

		// Start coroutine
		StartCoroutine(SmoothMove(VideoOptionsPanel, targetLocation2, timeDelta));
	}
	IEnumerator OptionsControlsState()
	{
		Vector3 relativeLocation = new Vector3((Screen.width / 2),0,0);
		Vector3 targetLocation = ControlsOptionsPanel.transform.position + relativeLocation;

		float timeDelta = 0.1f;

		StartCoroutine(SmoothMove(ControlsOptionsPanel, targetLocation, timeDelta));

		while(currentState == GameState.OptionsControls) {yield return null;} // running

		// Exit
		// Get the target position
		Vector3 relativeLocation2 = new Vector3(-(Screen.width / 2), 0, 0);
		Vector3 targetLocation2 = ControlsOptionsPanel.transform.position + relativeLocation2;

		StartCoroutine(SmoothMove(ControlsOptionsPanel, targetLocation2, timeDelta));
	}
    IEnumerator JoinMenuStep1State()
    {
        // Enter
        // Get the target position of panel
        Vector3 relativeLocation = new Vector3((Screen.width / 2) + (Screen.width / 4), 0, 0);
        Vector3 targetlocation = JoinMenuPanel.transform.position + relativeLocation;
        float timeDelta = 0.1f;

        // Start coroutine
        StartCoroutine(SmoothMove(JoinMenuPanel, targetlocation, timeDelta));


        while (currentState == GameState.JoinMenuStep1) { yield return null; } // running.

        // Exit
        // Get the target position
        Vector3 relativeLocation2 = new Vector3(-(Screen.width / 2) - (Screen.width / 4), 0, 0);
        Vector3 targetLocation2 = JoinMenuPanel.transform.position + relativeLocation2;

        // Start coroutine
        StartCoroutine(SmoothMove(JoinMenuPanel, targetLocation2, timeDelta));

    }
    IEnumerator LobbyMenuState()
    {
        // Enter
        // Get the target position of panel
        Vector3 relativeLocation = new Vector3((Screen.width / 2) + (Screen.width / 4), 0, 0);
        Vector3 targetlocation = LobbyMenuPanel.transform.position + relativeLocation;
        float timeDelta = 0.1f;

        // Start coroutine
        StartCoroutine(SmoothMove(LobbyMenuPanel, targetlocation, timeDelta));


        while (currentState == GameState.LobbyMenu) { yield return null; } // running.

        // Exit
        // Get the target position
        Vector3 relativeLocation2 = new Vector3(-(Screen.width / 2) - (Screen.width / 4), 0, 0);
        Vector3 targetLocation2 = LobbyMenuPanel.transform.position + relativeLocation2;

        // Start coroutine
        StartCoroutine(SmoothMove(LobbyMenuPanel, targetLocation2, timeDelta));
    }
    IEnumerator CreditsState()
    {
        // Enter
        // Get the target position of panel
        Vector3 relativeLocation = new Vector3((Screen.width / 2) + (Screen.width / 4), 0, 0);
        Vector3 targetlocation = CreditsPanel.transform.position + relativeLocation;
        float timeDelta = 0.1f;

        // Start coroutine
        StartCoroutine(SmoothMove(CreditsPanel, targetlocation, timeDelta));


        while (currentState == GameState.Credits) { yield return null; } // running.

        // Exit
        // Get the target position
        Vector3 relativeLocation2 = new Vector3(-(Screen.width / 2) - (Screen.width / 4), 0, 0);
        Vector3 targetLocation2 = CreditsPanel.transform.position + relativeLocation2;

        // Start coroutine
        StartCoroutine(SmoothMove(CreditsPanel, targetLocation2, timeDelta));
    }

    //--------------------Movment--------------------
    IEnumerator SmoothMove(GameObject item, Vector3 target, float delta)
	{
        coroutinesRunning += 1;
		// will need to perform some of this process and yeild until next frames
		float closeEnough = 0.2f;
		float distance = (item.transform.position - target).magnitude;

		// GC will trigger unless we define this ahead of time
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		// continue until we're there
		while(distance>=closeEnough)
		{
			// Confirm thaht it's moving

			// Move a bit then wait until next frame
			item.transform.position = Vector3.Lerp(item.transform.position, target, delta);
			yield return wait;

			// Check if we should repeat
			distance = (item.transform.position - target).magnitude;
		}
		// Complete the motion to prevent sliding
		item.transform.position = target;

        // Comfirm End
        coroutinesRunning -= 1;

    }
}
