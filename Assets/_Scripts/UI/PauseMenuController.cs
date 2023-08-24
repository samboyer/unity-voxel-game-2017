using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour {
    public SettingsMenuController settingsMenu;

    public static bool isPaused = false;
    //bool inSettings = false;

    Canvas pauseCanvas;

	void Start () {
        pauseCanvas = GetComponent<Canvas>();
    }
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (settingsMenu.isOpen)
                {
                    settingsMenu.FinishSettingsDialog();
                }
                else
                    Unpause();
            }
            else
                Pause();
        }
	}

    public void Pause()
    {
        Time.timeScale = 0;
        pauseCanvas.enabled = true;
        isPaused = true;
    }

    public void Unpause()
    {
        Time.timeScale = 1;
        pauseCanvas.enabled = false;
        isPaused = false;
    }
}
