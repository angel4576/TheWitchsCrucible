using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SceneConfiguration
{
    public bool enableAttack = true;
    public bool enableEnemyInstantKill = false;
}

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }
    public int currentSceneIndex { get; private set; }
    [SerializeField] private float debugTimescale;

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

    [Header("Scene Configuration")]
    private SceneConfiguration sceneConfig; 
    public bool enableSceneConfig;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if(fadeCanvas != null){
                DontDestroyOnLoad(fadeCanvas);
                Debug.Log("fadeCanvas is not null");
            } 
        }
        else
        {
            Destroy(gameObject);
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded; // subscribe to the scene loaded event
        sceneConfig = new SceneConfiguration();
    }

    private void Start()
    {
        Debug.Log("SceneManager started");
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Start Scene"){
            LoadScene(0); // load the first scene if starts from the start scene
        }
        
    }

    private void Update()
    {
        // test
        debugTimescale = Time.timeScale;
    }

    #region Scene Management
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        // save level start data for restart purpose
        if(!isReloading){
            // make sure monster data has been registered to game manager
            GameManager.Instance.FindAndRegisterMonsters();
            DataManager.Instance.WriteLevelStartData();
            DataManager.Instance.WriteCheckpointData(DataManager.Instance.levelStartData.position, true);
        }
        isReloading = false;

        if(enableSceneConfig)
            LoadSceneConfiguration(currentSceneIndex);
        

        // check if the fade canvas is assigned, if not create one
        if (fadeCanvas == null || fadeImage == null)
        {
            CreateFadeCanvas();
        }

        // re-register the event invoked by game manager
        GameManager.Instance.RegisterEventLevel1();
        
        StartCoroutine(FadeInRoutine());
    }

    private void LoadSceneConfiguration(int sceneIndex)
    {
        // Debug.Log("Load Scene Configuration" + sceneIndex);
        if(sceneIndex == 0) // level 1
        {
            sceneConfig.enableAttack = false;
            sceneConfig.enableEnemyInstantKill = true;
        }
        else
        {
            sceneConfig.enableAttack = true;
            sceneConfig.enableEnemyInstantKill = false;
        }
    }

    public SceneConfiguration GetSceneConfiguration()
    {
        return sceneConfig;
    }

    public void LoadScene(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;
        DataManager.Instance.worldData.curSceneIndex = currentSceneIndex; // Update the current scene index in the world data
        StartCoroutine(FadeOutAndLoadScene(sceneIndex));
        //UnityEngine.SceneManagement.SceneManager.LoadScene(scenes[sceneIndex]);

        // Set if player can switch world based on scene index
        if(sceneIndex != 0) // if not in level 1
        {
            DataManager.Instance.playerData.canSwitchWorld = true;
        }
        else if(sceneIndex == 0) 
        {
            // if(DataManager.Instance.playerData.checkPoint != null) // pass level 1 check point
            // {
            //     DataManager.Instance.playerData.canSwitchWorld = true;
            // }
            // else
            // {
            //     DataManager.Instance.playerData.canSwitchWorld = false;
            // }
        }

    }

    public void LoadScene(string sceneName)
    {
        currentSceneIndex = System.Array.IndexOf(scenes, sceneName);
        LoadScene(currentSceneIndex);
        LoadSceneConfiguration(currentSceneIndex);
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
    #endregion

    public void RestartFromCheckPoint()
    {
        StartCoroutine(FadeOutAndStartFromCheckPoint());
    }

    private IEnumerator FadeOutAndStartFromCheckPoint()
    {
        yield return FadeOutRoutine();
        DataManager.Instance.ResetDataToLastCheckpoint();
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeOutAndLoadScene(int sceneIndex)
    {
        yield return FadeOutRoutine();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scenes[sceneIndex]);
    }

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

    #region Pause and Resume
    // Pause and Resume Game
    public void PauseGame()
    {
        Time.timeScale = 0;
        // inputActionAsset.Disable();
        GameManager.Instance.player.inputActions.Disable();
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        // inputActionAsset.Enable();
        GameManager.Instance.player.inputActions.Enable();
        Debug.Log("Game Resumed");
    }
    #endregion

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
        Debug.Log("FadeCanvas created");
    }
}
