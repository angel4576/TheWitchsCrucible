using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Control player light when player is in the light zone
 */
public class LightZoneController : MonoBehaviour
{
    public PlayerController player;
    public PlayerLightController playerLight;

    [Header("Light Zone")]
    public float distance;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Light zone start and end
        float start = transform.position.x;
        float end = transform.position.x + distance;
        float t = Mathf.InverseLerp(start, end, player.transform.position.x);
        
        // Set player light
        playerLight.SetLightOuterRadius(t);
        playerLight.SetLightIntensity(t);
    }

    private void OnDrawGizmos()
    {
        // Visualize light zone area
        Gizmos.color = Color.yellow;
        Vector3 start = transform.position;
        Vector3 end = new Vector3(transform.position.x + distance, transform.position.y, 0f);
        Vector3 center = (start + end) * 0.5f;
        Vector3 size = new Vector3(end.x - start.x, 10f, 0f);
        
        Gizmos.DrawWireCube(center, size);
    }
}
