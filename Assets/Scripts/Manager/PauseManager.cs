using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public bool isPaused = false;
    public GameObject PauseScreen;
    
    
    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
    }

    public void TogglePause()
    {
        if (!isPaused)
        {
            Pause();
        }
        else
        {
            Resume();
            
        }
    }

    public void Pause()
    {
        isPaused = true;
        PauseScreen.SetActive(true);
        Time.timeScale = 0.0f;
    }

    public void Resume()
    {
        isPaused = false;
        PauseScreen.SetActive(false);
        Time.timeScale = 1.0f;
    }

    public void RestartFromBeginning()
    {
        GameSceneManager.Instance?.ReturnToMenu();
        // GameSceneManager.Instance?.NewGame();
    }
    
    public void RestartFromLastCheckpoint()
    {
        GameSceneManager.Instance?.RestartScene();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
}
