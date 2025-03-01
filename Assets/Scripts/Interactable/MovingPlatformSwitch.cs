using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformSwitch : MonoBehaviour, IInteractable
{
    public List<MovingPlatform> platforms;
    // public bool isActivated = false;
 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        // isActivated = !isActivated;
        // Debug.Log("Activate Switch!");
        foreach (var platform in platforms)
        {
            // Trigger moving platforms
            platform.SetActive(true);
        }
    }
    
}
