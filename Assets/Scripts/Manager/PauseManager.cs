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

    public void RestartCurrentScene()
    {
        GameSceneManager.Instance?.RestartScene();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    /*public void RestartLevel()
    {
        Resume();
        if(DataManager.Instance != null){
            DataManager.Instance.ResetDataToLevelStart();
        }
        else{
            // not suppose to be enter here
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
        }
    }*/

    /*public void StartFromCheckPoint()
    {
        //loadfromcheckpoint
        Resume();
        if(DataManager.Instance != null){
            //DataManager.Instance.ResetDataToLastCheckpoint();
            SceneManager.Instance.RestartFromCheckPoint();
        }
        else{
            // not suppose to be enter here
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
        }
    }*/
}
