using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CustomCameraShake : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer transposer;
    private CinemachineBasicMultiChannelPerlin noise;
    
    private float shakeTimer;
    
    private Vector3 originalOffset;
    public float shakeStrength;

    private void Awake()
    {
        
        noise = GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
        transposer = GetComponentInChildren<CinemachineFramingTransposer>();
        //originalOffset = transposer.VirtualCamera.transform.position;
        
        // if (noise == null)
        // {
        //     Debug.LogError("No MultiChannelPerlin on the virtual camera.", this);
        // }
        // else
        // {
        //     Debug.Log($"Noise Component: {noise}");
        // }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (shakeTimer > 0f)
        {
            Debug.Log("Update Timer");
            shakeTimer -= Time.unscaledDeltaTime;
            
            float offsetX = UnityEngine.Random.Range(-1f, 1f) * shakeStrength;
            float offsetY = UnityEngine.Random.Range(-1f, 1f) * shakeStrength;
            transposer.m_TrackedObjectOffset = originalOffset + new Vector3(offsetX, offsetY, 0f);
            Debug.Log($"OFFSET: {transposer.m_TrackedObjectOffset}");
            if (shakeTimer <= 0f)
            {
                // noise.m_AmplitudeGain = 0f;
                // noise.m_FrequencyGain = 0f;
            }
        }
        else
        {
            transposer.m_TrackedObjectOffset = originalOffset;
        }
    }

    public void HurtShake(float amplitude, float frequency, float duration)
    {
        // noise.m_AmplitudeGain = amplitude;
        // noise.m_FrequencyGain = frequency;
        shakeTimer = duration;
    }
}
