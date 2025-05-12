using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.IO;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }


    [Header("Scene Names")] 
    public string persistentName;
    public string level1Name;
    public string level2Name;

    [Header("Scene Configuration")] 
    public List<SceneConfig> sceneConfigs;

    [Header("Fade")]
    // public FadeController fadeController;
    public float fadeDuration = 1.0f;
    public Canvas fadeCanvas;
    public Image fadeImage;

    [Header("Input System")]
    public InputActionAsset inputActions;

    private int currentSceneIndex;
    private string currentSceneName;
    
    private List<ISceneReferenceReceiver> receivers = new List<ISceneReferenceReceiver>();
    private List<ICheckpointRestore> checkpointObjects = new List<ICheckpointRestore>();
    
    private bool isReloading = false;
    // private SceneConfigurationData sceneConfig;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (fadeCanvas == null || fadeImage == null)
        {
            CreateFadeCanvas();
        }
    }

    public void RegisterCheckpointObject(ICheckpointRestore obj)
    {
        if(!checkpointObjects.Contains(obj))
            checkpointObjects.Add(obj);
    }
    
    public void UnregisterCheckpointObject(ICheckpointRestore obj)
    {
        checkpointObjects.Remove(obj);
    }

    public SceneConfig GetCurrentSceneConfig()
    {
        SceneConfig config = sceneConfigs.Find(c => c.sceneName == currentSceneName);

        return config;
    }

    private void ApplySceneConfig()
    {
        /*SceneConfig config = null;
        foreach (var c in sceneConfigs)
        {
            if (c.sceneName == scene.name)
            {
                config = c;
            }
        }*/
        
        // SceneConfig config = sceneConfigs.Find(c => c.sceneName == scene.name);
        SceneConfig config = GetCurrentSceneConfig();
        
        if (config != null)
        {
            DataManager.Instance.playerData.hasPickedUpLantern = config.hasPickupLantern;
            DataManager.Instance.playerData.canSwitchWorld = config.canSwitchWorld;
            
            // Reset light value
            DataManager.Instance.playerData.light = config.lightStartValue;
            
        }
    }

    public void SaveCheckpoint()
    {
        foreach (var obj in checkpointObjects)
        {
            obj.SaveToCheckpoint(DataManager.Instance.checkpointData);
        }
    }

    public void LoadCheckpoint()
    {
        Debug.Log("[Game Scene Manager] Load check point data");
        foreach (var obj in checkpointObjects)
        {
            obj.LoadFromCheckpoint(DataManager.Instance.checkpointData);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[Game Scene Manager] Load Scene {scene.name} Complete; Current Scene Name: {currentSceneName}");
        InputManager.Instance.ResetGameplayLock();
        ResumeGame();

        // Reach checkpoint, restart from last checkpoint
        DataManager.Instance.LoadCheckpointData();
        if (!string.IsNullOrEmpty(DataManager.Instance.checkpointData.checkpointID) /* if there is checkpoint data */
            && DataManager.Instance.checkpointData.sceneName == scene.name) /* if in the same scene */ 
        {
            LoadCheckpoint();
        }
        else
        {
            // Scene initial configuration
            ApplySceneConfig();
        }
        
    }

    public string GetCurrentSceneName()
    {
        return currentSceneName;
    }
    
    // Scene Transition
    public void LoadGameScene(string sceneName)
    {
        StartCoroutine(TransitionToScene(sceneName));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        yield return FadeOutRoutine();
        
        // Get current scene name
        currentSceneName = sceneName;
        
        // Unload old scene（except Persistent Scene）
        Scene current = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (current.name != persistentName) // temp
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(current);
        }

        // Load new scene
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName));
        

        yield return FadeInRoutine();
    }

    public void NewGame()
    {
        // New game, reset checkpoint data
        string path = Path.Combine(Application.persistentDataPath, "CheckpointData.json");
        if (File.Exists(path))
            File.Delete(path);
        
        LoadGameScene(level1Name);
    }

    public void RestartScene()
    {
        // UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
        // StartCoroutine(RestartAdditiveScene(currentSceneName));
        LoadGameScene(currentSceneName);
    }

    IEnumerator RestartAdditiveScene(string sceneName)
    {
        // Unload then load
        yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
    }

    public void RestartFromCheckPoint()
    {
        
    }

    IEnumerator RestartWithFade()
    {
        yield return FadeOutRoutine();
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
        yield return FadeInRoutine();
    }
    
    private void PauseGame()
    {
        Time.timeScale = 0;
        inputActions.Disable();
        Debug.Log("Game Paused");
    }
    
    private void ResumeGame()
    {
        Time.timeScale = 1;
        inputActions.Enable();
        Debug.Log("Game Resumed");
    }
    
    #region FadeInFadeOut

    public IEnumerator FadeInRoutine()
    {
        PauseGame();
        fadeCanvas.gameObject.SetActive(true);
        fadeImage.gameObject.SetActive(true);
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeImage.color = new Color(0, 0, 0, (1 - timer / fadeDuration));
            yield return null;
        }
        ResumeGame();
        fadeCanvas.gameObject.SetActive(false);
        fadeImage.gameObject.SetActive(false);
    }

    public IEnumerator FadeOutRoutine()
    {
        PauseGame();
        fadeCanvas.gameObject.SetActive(true);
        fadeImage.gameObject.SetActive(true);
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeImage.color = new Color(0, 0, 0, timer / fadeDuration);
            yield return null;
        }
        ResumeGame();
    }
    
    private void CreateFadeCanvas()
    {
        // Create FadeCanvas
        fadeCanvas = new GameObject("FadeCanvas").AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        //fadeCanvas.gameObject.AddComponent<CanvasGroup>().alpha = 0;
        fadeCanvas.gameObject.AddComponent<CanvasRenderer>();
        fadeCanvas.gameObject.SetActive(true);
        fadeCanvas.transform.SetParent(transform);
        fadeCanvas.gameObject.layer = LayerMask.NameToLayer("UI");

        // Create FadeImage as a child of FadeCanvas
        GameObject fadeImageObject = new GameObject("FadeImage");
        fadeImage = fadeImageObject.AddComponent<Image>();
        fadeImage.color = Color.black;
        fadeImageObject.transform.SetParent(fadeCanvas.transform);

        // Set FadeImage to cover the full screen
        RectTransform fadeRectTransform = fadeImage.GetComponent<RectTransform>();
        fadeRectTransform.anchorMin = Vector2.zero; // Bottom-left corner
        fadeRectTransform.anchorMax = Vector2.one;  // Top-right corner
        fadeRectTransform.offsetMin = Vector2.zero; // Resetting position offsets
        fadeRectTransform.offsetMax = Vector2.zero;

        // Optionally, set FadeImage to inactive initially
        fadeImageObject.SetActive(false);
        
        DontDestroyOnLoad(fadeCanvas.gameObject);
        Debug.Log("FadeCanvas created");
    }

    #endregion
    
    
}
