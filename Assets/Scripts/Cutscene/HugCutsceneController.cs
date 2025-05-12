using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HugCutsceneController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform pet;
    [SerializeField] private Transform petJumpStart;
    [SerializeField] private float xSpeed = 5f;
    [SerializeField] private float ySpeed = 5f;
    // [SerializeField] private float pounceDuration = 0.3f;
    
    [Header("Hug Settings")]
    [SerializeField] private Vector3 hugOffset = new Vector3(-0.5f, 0, 0); // pos relative to player
    [SerializeField] private float hugTime = 3f;
    [SerializeField] private GameObject hugPrefab;
    
    private PlayerController playerController;
    private MeshRenderer playerRenderer;
    private MeshRenderer petRenderer;
    private Animator petAnimator;
    private Rigidbody2D petRb; 
    
    private bool hugTriggerReached = false;

    // Start is called before the first frame update
    void Start()
    {
        petRb = pet.GetComponent<Rigidbody2D>();
        playerRenderer = player.GetComponent<MeshRenderer>();
        playerController = player.GetComponent<PlayerController>();
        petRenderer = pet.GetComponent<MeshRenderer>();
        petAnimator = pet.GetComponent<Animator>();
        
        // StartHugSequence();
    }

    private void OnEnable()
    {
        EventManager.OnCutscenePetReachPlayer += OnPetReachHugZone;
    }

    private void OnDisable()
    {
        EventManager.OnCutscenePetReachPlayer -= OnPetReachHugZone;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnPetReachHugZone()
    {
        hugTriggerReached = true;
    }

    public void ShowPet()
    {
        StartCoroutine(ShowPetCoroutine());
    }
    
    IEnumerator ShowPetCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        petRenderer.enabled = true;
    }
    
    public void StartHugSequence()
    {
        StartCoroutine(HugSequence());
    }

    IEnumerator HugSequence()
    {
        // playerController.DisableGameplayInput();
        InputManager.Instance.LockGameplayInput();
        hugTriggerReached = false;
            
        yield return new WaitForSeconds(1f);
        
        // Pet jumps to hug position
        petRb.velocity = new Vector2(xSpeed, ySpeed);
        petAnimator.SetTrigger("JumpTrigger");
        
        /*while (!hugTriggerReached)
        {
            yield return null; 
        }*/
        yield return new WaitUntil(() => hugTriggerReached);
        
        // Reach target position
        // Debug.Log("[Hug Cutscene] Pet reach hug position");
        StartHugging();
        
        yield return new WaitForSeconds(hugTime);
        
        AfterHug();

        
    }

    private void StartHugging()
    {
        Vector3 targetPos = player.position + hugOffset;
        
        pet.position = targetPos;
        petRb.velocity = Vector2.zero;
        petRb.isKinematic = true;
        
        // Use sprite (temp)
        petRenderer.enabled = false;
        playerRenderer.enabled = false;
        
        hugPrefab.SetActive(true);
    }

    private void AfterHug()
    {
        // Show characters and hide hug sprite
        petRenderer.enabled = true;
        petRb.isKinematic = false;
        playerRenderer.enabled = true;
        
        hugPrefab.SetActive(false);
        
        InputManager.Instance.UnlockGameplayInput();
    }

    private Vector2 GetJumpPosition(float t)
    {
        float x = petJumpStart.position.x + xSpeed * t;
        float y = petJumpStart.position.y + ySpeed * t + 0.5f * (Physics2D.gravity.y * 5) * t * t;
        return new Vector2(x, y);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position + hugOffset, 0.2f);
        
        float jumpDuration = 2 * (ySpeed / -(Physics2D.gravity.y * 5));
        Gizmos.color = Color.red;
        
        // Draw jump trajectory
        for(float t = 0; t <= jumpDuration; t+=Time.fixedDeltaTime)
        {
            Gizmos.DrawSphere(GetJumpPosition(t), 0.1f);
        }
    }
}
