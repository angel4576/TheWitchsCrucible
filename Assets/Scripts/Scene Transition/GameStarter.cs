using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameStarter : MonoBehaviour
{
    [Tooltip("Set First Level Scene Name")]
    public string firstScene = "Level1";

    public void NewGame()
    {
        // New game
        // Reset checkpoint data
        string path = Path.Combine(Application.persistentDataPath, "CheckpointData.json");
        if (File.Exists(path))
            File.Delete(path);
        
        // Load first scene（additive）
        // UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(firstLevelScene, LoadSceneMode.Additive);
        GameSceneManager.Instance?.LoadGameScene(firstScene);
    }
}
