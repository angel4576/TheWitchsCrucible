using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLightController : MonoBehaviour
{
    public Light2D playerLight;
    private bool isLightOn = false;
    
    [Header("Light Settings")]
    public float maxOuterRadius;
    public float maxIntensity;
    
    private float minOuterRadius;
    private float minIntensity;
    
    // Start is called before the first frame update
    void Start()
    {
        if (playerLight == null)
        {
            playerLight = GetComponent<Light2D>();    
        }

        playerLight.enabled = isLightOn;

        minOuterRadius = playerLight.pointLightOuterRadius;
        minIntensity = playerLight.intensity;
    }
    
    public void TurnOnLight()
    {
        isLightOn = true;
        playerLight.enabled = isLightOn;
    }

    public void SetLightIntensity(float t)
    {
        t *= t;
        playerLight.intensity = Mathf.Lerp(minOuterRadius, maxOuterRadius, t);

    }

    public void SetLightOuterRadius(float t)
    {
        t *= t;
        playerLight.pointLightOuterRadius = Mathf.Lerp(minOuterRadius, maxOuterRadius, t);
    }
    
    
}
