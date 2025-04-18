using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [Header("Scene List")]
    public string[] scenes;

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
        
        /*if (fadeCanvas == null || fadeImage == null)
        {
            CreateFadeCanvas();
        }*/
    }

    public void Register(ISceneReferenceReceiver receiver)
    {
        if (!receivers.Contains(receiver))
        {
            receivers.Add(receiver);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[Game Scene Manager] Load Scene Complete");
        ResumeGame();
        
        // if restart
        DataManager.Instance.playerData.hasPickedUpLantern = false;
        
        // save level start data for restart purpose
        /*if(!isReloading)
        {
            DataManager.Instance.WriteLevelStartData();
            DataManager.Instance.WriteCheckpointData(DataManager.Instance.levelStartData.position, true);
        }
        isReloading = false;*/

        
    }

    public void RestartScene()
    {
        // StartCoroutine(RestartWithFade());
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
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
