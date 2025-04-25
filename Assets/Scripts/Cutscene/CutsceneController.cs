using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CutsceneController : MonoBehaviour
{
    public InputActionAsset inputActions;
    [SerializeField] private PlayerController player;
    [SerializeField] private Transform targetTransform; 
    
    private Transform playerTransform;
    private Rigidbody2D playerRigidbody;
    
    private bool triggered = false;
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = player.transform;
        playerRigidbody = player.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Cutscene Controller] trigger cutscene");
            triggered = true;
            StartCoroutine(PlayCutscene());
        }
        
    }

    IEnumerator PlayCutscene()
    {
        player.inputActions.Gameplay.Disable();
        Vector2 targetPos = new Vector2(targetTransform.position.x, playerTransform.position.y);
        while (Vector2.Distance(playerTransform.position, targetPos) > 1e-2)
        {
            Debug.Log("[Cutscene Controller] auto move player");
            playerRigidbody.velocity = new Vector2(player.speed, playerRigidbody.velocity.y);
            // playerTransform.position = Vector2.MoveTowards(playerTransform.position, targetPos
            //     , player.speed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }
        
        playerRigidbody.velocity = Vector2.zero;
        
        // Play player animation
        
        // Play light tower animation
        
    }
    
}
