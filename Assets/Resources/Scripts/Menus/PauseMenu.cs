using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public bool paused = false;
    public GameObject pauseMenuUI;
    public Transform owner;
    private PlayerController playerController;
    // Start is called before the first frame update
    void Start()
    {
        playerController = owner.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(paused)
            {
                UnPause();
            }
            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        paused = true;
        playerController.DisableInput = true;
    }

    public void UnPause()
    {
        pauseMenuUI.SetActive(false);
        paused = false;
        playerController.DisableInput = false;
    }
}
