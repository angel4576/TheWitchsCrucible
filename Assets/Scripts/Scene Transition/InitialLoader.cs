using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class InitialLoader : MonoBehaviour
{
    [Tooltip("Set First Scene Name")]
    public string firstLevelScene = "Level1";

    void Start()
    {
        // New game, reset checkpoint data
        string path = Path.Combine(Application.persistentDataPath, "CheckpointData.json");
        if (File.Exists(path))
            File.Delete(path);
        
        // Load first scene（additive）
        // UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(firstLevelScene, LoadSceneMode.Additive);
        GameSceneManager.Instance.LoadGameScene(firstLevelScene);
    }
}
