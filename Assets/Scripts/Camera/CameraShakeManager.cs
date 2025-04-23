using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }
    
    public CinemachineImpulseSource impulseSource;
    // public CustomCameraShake customCameraShake;

    private bool isPause;
    private void Awake()
    {
        // Singleton for local scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
            
            // DontDestroyOnLoad(gameObject);
        }
        
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateHurtShake()
    {
        // customCameraShake.HurtShake(3.0f, 10.0f, 1.2f);
        // CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
        else
        {
            Debug.LogWarning("ImpulseSource is not assigned!");
        }
    }

    public void PauseTime(float time)
    {
        if(!isPause)
            StartCoroutine(PauseCoroutine(time));
    }
    
    private IEnumerator PauseCoroutine(float pauseDuration)
    {
        isPause = true; 
        
        float originalTimeScale = Time.timeScale; // time flow speed
        Time.timeScale = 0f; 
        
        yield return new WaitForSecondsRealtime(pauseDuration);
        
        Time.timeScale = originalTimeScale;
        isPause = false;
    }
}
