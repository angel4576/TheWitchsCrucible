using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class InitialLoader : MonoBehaviour
{
    [Tooltip("Set Menu Scene Name")]
    public string menuName = "Menu";

    void Start()
    {
        // NewGame();
        LoadMainMenu();
    }

    public void LoadMainMenu()
    {
        GameSceneManager.Instance?.LoadGameScene(menuName);
    }

    /*public void NewGame()
    {
        // New game, reset checkpoint data
        string path = Path.Combine(Application.persistentDataPath, "CheckpointData.json");
        if (File.Exists(path))
            File.Delete(path);
        
        // Load first scene（additive）
        // UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(firstLevelScene, LoadSceneMode.Additive);
        GameSceneManager.Instance?.LoadGameScene(menuName);
    }*/
}
