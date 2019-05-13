using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Mirror;

public class Spectator : NetworkBehaviour
{
	public bool Spectating = false;
	public Text NameText;
	public Button NextButton, PrevButton;
	public List<Camera> Cameras;
	public int Index = 0;
	public GameObject SpectatorScreen;
	Canvas canvas; 

    // Start is called before the first frame update
    void Start()
    {
		canvas = GetComponent<Canvas>();
		NextButton.onClick.AddListener(Next);
		PrevButton.onClick.AddListener(Prev);
    }

    // Update is called once per frame
    void Update()
    {
		if (Spectating)
		{
			foreach (var camera in Cameras)
			{
				if (camera == null)
				{
					Cameras.Remove(camera);
					ChoseValidCamera();
				}
			}
			if (Input.GetKeyDown(KeyCode.E))
			{
				Next();
			}
			else if (Input.GetKeyDown(KeyCode.Q))
			{
				Prev();
			}

		}
    }

	public void StartSpectating()
	{
		Spectating = true;
		foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
		{
			Cameras.Add(player.GetComponentInChildren<Camera>());
		}
		Cameras[Index].enabled = true;
		canvas.worldCamera = Cameras[Index];
	}
	void Next()
	{
		Cameras[Index].enabled = false;
		if (Index < Cameras.Count-1)
		{
			Index++;
		}
		else
		{
			Index = 0;
		}
		Cameras[Index].enabled = true;
		canvas.worldCamera = Cameras[Index];
	}
	void Prev()
	{
		Cameras[Index].enabled = false;
		if (Index > 0)
		{
			Index--;
		}
		else
		{
			Index = Cameras.Count-1;
		}
		Cameras[Index].enabled = true;
		canvas.worldCamera = Cameras[Index];
	}
	void ChoseValidCamera()
	{
		Index = 0;
		Cameras[Index].enabled = true;
	}


}
