using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialLoader : MonoBehaviour
{
    [Tooltip("Set First Scene Name")]
    public string firstLevelScene = "Level2_SceneTest";

    void Start()
    {
        // Load first scene（additive）
        // UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(firstLevelScene, LoadSceneMode.Additive);
        GameSceneManager.Instance.LoadGameScene(firstLevelScene);
    }
}
