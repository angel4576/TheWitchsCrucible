using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    public CutsceneController controller;
    private bool triggered = false;

    /*private void FixedUpdate()
    {
        if (triggered) return;
        float playerX = controller.player.transform.position.x;
        if (playerX > transform.position.x)
        {
            triggered = true;
            controller.StartCutscene();
        }
    }*/

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            controller.StartCutscene();
        }
        
    }
    
    
    
}
