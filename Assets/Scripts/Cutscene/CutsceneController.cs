using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] public PlayerController player;
    [SerializeField] private Transform targetTransform; 
    
    [SerializeField] private Camera cam;
    [SerializeField] private CinemachineVirtualCamera vCam;
    [SerializeField] private CinemachineVirtualCamera vCamCloseUp;
    
    public GameObject lanternPrefab;
    public ParticleSystem flash;
    
    private Transform playerTransform;
    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;
    
    // Camera
    private CinemachineBrain brain;
    
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = player.transform;
        playerRigidbody = player.GetComponent<Rigidbody2D>();
        playerAnimator = player.GetComponent<Animator>();
        
        // brain = cam.GetComponent<CinemachineBrain>();
    }

    // Update is called once per frame
    void Update()
    {
           
    }

    public void StartCutscene()
    {
        StartCoroutine(PlayCutscene());
    }
    
    IEnumerator PlayCutscene()
    {
        player.inputActions.Gameplay.Disable();
        
        EventManager.BroadcastCutsceneStart();

        yield return StartCoroutine(AutoMoveToTarget());
        
        // --- Player reaches light tower ---
        playerRigidbody.velocity = Vector2.zero;
        EventManager.BroadcastCutsceneReachLight();
        
        // Camera movement
        SwitchVirtualCamera();
        
        // Play player animation
        playerAnimator.SetTrigger("DieTrigger");
        yield return new WaitForSeconds(1.5f);
        
        // Play light tower animation
        Instantiate(flash, targetTransform.position + new Vector3(0, 6, 0), Quaternion.identity);
        Instantiate(lanternPrefab, targetTransform.position + new Vector3(0, 6, 0), Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        
        // Transition to level 2
        GameSceneManager.Instance.LoadGameScene(GameSceneManager.Instance.level2Name);
    }

    IEnumerator AutoMoveToTarget()
    {
        Vector2 startPos = playerTransform.position;
        Vector2 targetPos = new Vector2(targetTransform.position.x, playerTransform.position.y);
        Vector2 moveDir = (targetPos - startPos).normalized;
        // Player auto move to light tower
        while (Vector2.Dot(moveDir, (targetPos - (Vector2)playerTransform.position).normalized) > 0f) // if move towards target
        {
            // targetPos = new Vector2(targetTransform.position.x, playerTransform.position.y);
            // Debug.Log("[Cutscene Controller] auto move player");
            playerRigidbody.velocity = new Vector2(player.speed * 1.5f, playerRigidbody.velocity.y);
            yield return new WaitForFixedUpdate();
        }
    }

    private void PlayParticleEffect()
    {
        if (flash != null)
        {
            flash.Play();
        }
    }
    private void SwitchVirtualCamera()
    {
        vCam.Priority = 10;
        vCamCloseUp.Priority = 20;
    }
    
}
