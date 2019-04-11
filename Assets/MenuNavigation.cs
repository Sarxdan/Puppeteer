using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class MenuNavigation : MonoBehaviour
{
	public bool cooldown = true;							// "animation" cooldown. (so the game don't register button presses while panels are moving and breaking...)
	public GameObject OptionsPanel;							// the panel with options buttons on.
	public GameState currentState = GameState.MainMenu;		// current state.


	// Main Menu Buttons
	public Button JoinGameButton; 
	public Button HostGameButton;
	public Button NicknameButton;
	public Button OptionsButton;
	public Button ExitButton;

	// Options Menu Buttons
	public Button VideoButton; public Text VideoText;
	public Button AudioButton; public Text AudioText;
	public Button ControlsButton; public Text ControlsText;

	//----------------Enum----------------
	public enum GameState
	{
		MainMenu,
		JoinMenuStep1,
		JoinMenuStep2,
		HostMenu,
		Nickname,
		Options
	}


	// Start is called before the first frame update
	void Start()
	{
		JoinGameButton.onClick.AddListener(JoinGame);
		HostGameButton.onClick.AddListener(HostGame);
		NicknameButton.onClick.AddListener(Nickname);
		OptionsButton.onClick.AddListener(Options);
		ExitButton.onClick.AddListener(Exit);
	}

	// Update is called once per frame
	void Update()
	{

	}

	void HostGame()
	{
		if (cooldown && currentState != GameState.MainMenu)
		{
			ChangeState(GameState.MainMenu);
		}
	}

	void JoinGame()
	{

	}

	void Nickname()
	{
	}

	void Options()
	{
		if (cooldown && currentState != GameState.Options)
		{
			ChangeState(GameState.Options);
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

	IEnumerator OptionsState()
	{
		//Enter
		// Get the target position of panel
		Vector3 relativeLocation = new Vector3(280.0f, 0.0f, 0.0f);
		Vector3 targetLocation = OptionsPanel.transform.position + relativeLocation;
		float timeDelta = 0.1f;

		// Get target fade value
		float targetOpacity = 1;



		// Start coroutine
		StartCoroutine(SmoothMove(targetLocation, timeDelta));
		StartCoroutine(FadeText(VideoText, targetOpacity, timeDelta));
		StartCoroutine(FadeText(AudioText, targetOpacity, timeDelta));
		StartCoroutine(FadeText(ControlsText, targetOpacity, timeDelta));


		while (currentState == GameState.Options){ yield return null; } // running.

		//Exit

		// Get the target position
		Vector3 relativeLocation2 = new Vector3(-280.0f, 0.0f, 0.0f);
		Vector3 targetLocation2 = OptionsPanel.transform.position + relativeLocation2;

		// Get target fade value
		float targetOpacity2 = 0;

		// Start coroutine
		StartCoroutine(SmoothMove(targetLocation2, timeDelta));
		StartCoroutine(FadeText(VideoText, targetOpacity2, timeDelta));
		StartCoroutine(FadeText(AudioText, targetOpacity2, timeDelta));
		StartCoroutine(FadeText(ControlsText, targetOpacity2, timeDelta));
	}
	IEnumerator MainMenuState()
	{
		//Enter

		while (currentState == GameState.Options){ yield return null; } // running.

		//Exit
	}




	//--------------------Movment--------------------
	IEnumerator SmoothMove(Vector3 target, float delta)
	{
		cooldown = false;
		// will need to perform some of this process and yeild until next frames
		float closeEnough = 0.2f;
		float distance = (OptionsPanel.transform.position - target).magnitude;

		// GC will trigger unless we define this ahead of time
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		// continue until we're there
		while(distance>=closeEnough)
		{
			// Confirm thaht it's moving
			Debug.Log("Executing Movment");

			// Move a bit then wait until next frame
			OptionsPanel.transform.position = Vector3.Lerp(OptionsPanel.transform.position, target, delta);
			yield return wait;

			// Check if we should repeat
			distance = (OptionsPanel.transform.position - target).magnitude;
		}
		// Complete the motion to prevent sliding
		OptionsPanel.transform.position = target;

		// Comfirm End
		Debug.Log("Movment Complete");
		cooldown = true;
	}

	//--------------------Fade--------------------
	IEnumerator FadeText(Text text, float target, float delta)
	{
		// will need to perform some of this process and yeild until next frames
		float closeEnough = 0.2f;
		float difference = Mathf.Abs(text.color.a - target);
		// GC will trigger unless we define this ahead of time
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		// continue until we're there
		while (difference >= closeEnough)
		{
			// Confirm thaht it's moving
			Debug.Log("Executing Movment");

			// Move a bit then wait until next frame
			float temp = Mathf.Lerp(text.color.a, target, delta);
			text.color = new Color(text.color.r, text.color.g, text.color.b, temp);
			yield return wait;

			// Check if we should repeat
			difference = Mathf.Abs(text.color.a - target);
		}
		// Complete the motion to prevent sliding
		text.color = new Color(text.color.r, text.color.g, text.color.b, target);

		// Comfirm End
		Debug.Log("Movment Complete");
	}


}
