using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    // registered scenes
    [Header("Scenes")]
    public string[] scenes;

    public int currentSceneIndex { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("SceneManager started");
        LoadScene(0);
    }


    public void LoadScene(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;
        DataManager.Instance.worldData.curSceneIndex = currentSceneIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(scenes[sceneIndex]);
    }

    public void LoadScene(string sceneName)
    {
        currentSceneIndex = System.Array.IndexOf(scenes, sceneName);
        LoadScene(currentSceneIndex);
    }

    public void LoadNextScene()
    {
        if (currentSceneIndex < scenes.Length)
        {
            currentSceneIndex++;
            LoadScene(currentSceneIndex);
        }
        else
        {
            Debug.Log("Scene Index out of bounds");
        }
    }
}
