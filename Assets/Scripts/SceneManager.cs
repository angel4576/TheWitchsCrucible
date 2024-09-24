using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }
    public int currentSceneIndex { get; private set; }

    // registered scenes
    [Header("Scenes")]
    public string[] scenes;

    [Header("Input System")]
    public InputActionAsset inputActionAsset;


    // use for fade in/out transition
    [Header("Fade Effect Settings")]
    public float fadeDuration = 1.0f;
    public Canvas fadeCanvas;
    public Image fadeImage;

    // track if the player is reloading
    private bool isReloading = false;

    private bool isFading = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(fadeCanvas);
        }
        else
        {
            Destroy(gameObject);
        }
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded; // subscribe to the scene loaded event
    }

    private void Start()
    {
        Debug.Log("SceneManager started");
        LoadScene(0);
    }

    private void Update()
    {
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeInRoutine());
        Debug.Log("Scene loaded: " + scene.name);
        // register the current player's position as the first checkpoint
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        if(player != null)
        {
            if (DataManager.Instance.playerData.checkPoint != null)
            {
                player.position = DataManager.Instance.playerData.checkPoint;
                // isReloading = false;
            }
            else
            {
                DataManager.Instance.playerData.checkPoint = new Vec3(player.position);
            }
        }
    }


    public void LoadScene(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;
        DataManager.Instance.worldData.curSceneIndex = currentSceneIndex; // Update the current scene index in the world data
        StartCoroutine(FadeOutAndLoadScene(sceneIndex));
        //UnityEngine.SceneManagement.SceneManager.LoadScene(scenes[sceneIndex]);
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

    public void ReloadScene()
    {
        isReloading = true;
        LoadScene(currentSceneIndex);
    }

    private IEnumerator FadeOutAndLoadScene(int sceneIndex)
    {
        yield return FadeOutRoutine();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scenes[sceneIndex]);
    }

    private IEnumerator FadeInRoutine()
    {
        PauseGame();
        fadeCanvas.gameObject.SetActive(true);
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeImage.color = new Color(0, 0, 0, 1 - timer / fadeDuration);
            yield return null;
        }
        ResumeGame();
        fadeCanvas.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutRoutine()
    {
        PauseGame();
        fadeCanvas.gameObject.SetActive(true);
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeImage.color = new Color(0, 0, 0, timer / fadeDuration);
            yield return null;
        }
        ResumeGame();
    }

    // Pause and Resume Game
    public void PauseGame()
    {
        Time.timeScale = 0;
        inputActionAsset.Disable();
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        inputActionAsset.Enable();
        Debug.Log("Game Resumed");
    }
    
}
