using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }
    
    public CinemachineImpulseSource impulseSource;
    public CustomCameraShake customCameraShake;

    private bool isPause;
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
        impulseSource.GenerateImpulse();
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
