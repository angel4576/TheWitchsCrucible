using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using Spine.Unity;
using System.Linq;

public class PlayerController : MonoBehaviour, ICheckpointRestore
{
    public PlayerInputControl inputActions;
    private Vector2 inputDirection;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private SpriteRenderer rend;
    [HideInInspector]public int faceDir;
    public GameObject PauseScreen;

    private PhysicsCheck physicsCheck;
    
    public Pet pet;

    [Header("Movement")]
    public float speed;

    [Header("Jump")]
    [HideInInspector]public float jumpForce;
    public float jumpSpeed; // initial speed
    public float gravity;
    public bool jumpTriggered;
    private bool isJump;

    [Header("Lantern")]
    public Lantern lantern;
    private bool hasLantern;
    
    [Header("Monster")]
    public GameObject monsterAnimation;
    public MonsterAppearanceController monsterAppearController;

    [Header("Death Effect")]
    public float dissolveSpeed;
    private float dissolveThreshold = 2;

    public bool isDead;
    // private bool isDead;
    public float deathDelay; // seconds before player dies and start the process of restarting
    
    [Header("Switch Effect")]
    public GameObject switchEffect;
    public Vector3 switchEffectOffset;

    [Header("Spine Animation")]
    private Animator ani;
    public SkeletonMecanim skeletonMecanim;  // Use SkeletonMecanim instead of SkeletonAnimation
    private Spine.Slot[] cloakSlots;
    private Spine.Slot[] lampSlots;
    private Dictionary<Spine.Slot, Spine.Attachment> originalCloakAttachments = new Dictionary<Spine.Slot, Spine.Attachment>();

    
    [Header("Attacks")]
    [HideInInspector]public float meleeDamage;
    [HideInInspector]public float meleeLightHeal;
    //[SerializeField]
    [HideInInspector]private float meleeAttackSpeedPerSec, meleeAttackRange, rangeAttackCost, rangeAttackSpeedPerSec;
    public float rangeAttackDamage, rangeAttackRange;
    //[SerializeField]
    [HideInInspector]private bool canAttack = true;

    [HideInInspector]public GameObject meleeProj;
    [HideInInspector]public GameObject rangedProj;
    
    [Header("Invulnerability")]
    public float invulnerabilityTime;
    public bool isInvulnerable;
    public Color invulnerabilityColor;
    // public Color hurtColor; 

    [Header("Hurt")] 
    public float pauseTime;
    public float hurtForce;
    public float hurtTime;
    public PostProcessingEffect hurtEffect;
    public ParticleSystem hurtParticles;
    // public float knockDistance;
    public bool isHurt;
    
    [Header("Dialogue")]
    public DialogueController dialogueController;
    
    // Material
    private Material material;
    
    // Cutscene control
    private bool isCutscenePlay = false;

    #region Lifecycle 
    private void Awake()
    {
        // inputActions = new PlayerInputControl();
        inputActions = InputManager.Instance.GetActions();
        
        // inputActions.Gameplay.Fire.started += MeleeAttack;
        // inputActions.Gameplay.RangeAttack.started += RangeAttack;
        
        // dialogueController.SetInputAction(inputActions);
    }

    private void OnEnable()
    {
        // +=: register actions to action binding
        inputActions.Gameplay.Jump.started += Jump; // call jump when the moment corresponding button is pressed
        inputActions.Gameplay.Pause.started += Pause;

        inputActions.Gameplay.Enable();
        EventManager.OnCutsceneStart += HandleCutsceneStart;
        
        GameSceneManager.Instance?.RegisterCheckpointObject(this);
    }


    private void OnDisable()
    {
        inputActions.Gameplay.Jump.started -= Jump;
        inputActions.Gameplay.Pause.started -= Pause;
        
        inputActions.Gameplay.Disable();
        EventManager.OnCutsceneStart -= HandleCutsceneStart;
        
        GameSceneManager.Instance?.UnregisterCheckpointObject(this);

    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        rend = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();
        skeletonMecanim = GetComponent<SkeletonMecanim>();  // Use SkeletonMecanim

        // Get first material
        material = skeletonMecanim.skeletonDataAsset.atlasAssets[0].Materials.FirstOrDefault();
        if (material != null)
        {
            material.SetFloat("_DissolveThreshold", dissolveThreshold);
            material.SetColor("_InvulnerabilityColor", Color.white); 
        }

        // Get script reference
        physicsCheck = GetComponent<PhysicsCheck>();

        // Get all slots related to cape
        GetAllCloakSlots();
        GetLampSlot();
        if (WorldControl.Instance.isRealWorld)
        {
            DisableCloak();
        }
        
        GameManager.Instance?.RegisterPlayer(this);
        GameManager.Instance?.OnLanternFirstPickedUp.AddListener(SetLanternStatus);
    }

    private void OnDestroy()
    {
        GameManager.Instance?.OnLanternFirstPickedUp.RemoveListener(SetLanternStatus);
    }

    // Update is called once per frame
    void Update()
    {
        
        // Read Vector2 value in Move action
        inputDirection = inputActions.Gameplay.Move.ReadValue<Vector2>();
        if (!PauseScreen.GetComponent<PauseManager>().isPaused && !isHurt)
        {
            FlipDirection();
        }

        // Set animation state
        SetAnimation();

        // Test Player Death 
        if(Keyboard.current.f1Key.wasPressedThisFrame)
        {
            isDead = true;
            ani.SetTrigger("DieTrigger");
        }

        if(isDead)
            PlayDissolve();

    }

    private void FixedUpdate()
    {
        
        
        if(isHurt)
            return;
        
        if(dialogueController != null && dialogueController.IsDialoguePlaying())
            return;

        if (isCutscenePlay)
            return;
        
        Move();
        
        // Jump
        if(isJump)
            UpdateVelocityY();
        
        if (!physicsCheck.isOnGround)
        {
            rb.velocity += Vector2.down * Physics2D.gravity.y * Time.fixedDeltaTime;

            // Debug.Log("applying velocity" + rb.velocity);
            ani.SetBool("IsLanded", false);
        }
        else
        {
            ani.SetBool("IsLanded", true);
        }
    }
    
    #endregion

    #region Character Movement
    private void Move()
    {
        if (inputDirection.y != 0)
        {
            inputDirection.x *= math.sqrt(2);
        }
        
        // if(physicsCheck.isTouchForward && physicsCheck.isOnGround)
        //     rb.velocity = new Vector2(0, rb.velocity.y);
        // else
        rb.velocity = new Vector2(inputDirection.x * speed, rb.velocity.y);
        
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (physicsCheck.isOnGround && !PauseScreen.GetComponent<PauseManager>().isPaused && !jumpTriggered)
        {
            jumpTriggered = true;
            // delay jump
            StartCoroutine(DelayJump(0.00000001f));

            // Pet jump
            // Invoke(nameof(ControlPetJump), petJumpDelayTime);
        }
    }

    private IEnumerator DelayJump(float delay)
    {
        ani.SetTrigger("JumpTrigger");
        yield return new WaitForSeconds(delay);
        
        isJump = true;
        // Ignore x_velocity when touch wall
        if(physicsCheck.isTouchForward)
            // rb.velocity = new Vector2(0, jumpSpeed);
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        else
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed); // initial y speed

        // rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        
        yield return new WaitForSeconds(0.05f);
        jumpTriggered = false;
    }

    private void UpdateVelocityY()
    {
        // Ignore x_velocity when touch wall during update
        /*if(physicsCheck.isTouchForward)
            rb.velocity = new Vector2(0, rb.velocity.y);*/

        // V = V0 - gt
        float yVelocity = rb.velocity.y;
        yVelocity -= gravity * Time.fixedDeltaTime; // update y speed
        rb.velocity = new Vector2(rb.velocity.x, yVelocity);

        // When fall and land on ground
        if (physicsCheck.isOnGround && rb.velocity.y <= 0)
            isJump = false;

    }

    private void ControlPetMovement()
    {
        if(pet.gameObject.activeSelf)
            pet.canMove = true;
    }

    private void ControlPetJump()
    {
        if(pet.gameObject.activeSelf)
            pet.canJump = true;
    }

    private void FlipDirection()
    {
        faceDir = Math.Sign(transform.localScale.x);

        if (inputDirection.x < 0)
        {
            faceDir = -1;
        }
        else if (inputDirection.x > 0)
        {
            faceDir = 1;
        }

        transform.localScale = new Vector3(faceDir * Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
    #endregion

    public void TakeDamage(float damage, Transform attacker)
    {
        // Take damage
        if (!isInvulnerable)
        {
            // Visual effect
            CameraShakeManager.Instance.PauseTime(pauseTime); 
            PlayHurtParticleEffect();
            hurtEffect.PlayHitEffect();
            
            // Get hurt
            ani.SetTrigger("HitTrigger");
            Vector2 attackDirection = new Vector2(transform.position.x - attacker.position.x, 0).normalized;
            StartCoroutine(Knockback(attackDirection));
            
            // DataManager.Instance.playerData.currentHealth -= damage;
            
            // Update light when take damage
            DataManager.Instance.playerData.light -= damage;
            UIManager.Instance.BroadcastMessage("UpdateLight");
            CameraShakeManager.Instance.GenerateHurtShake();
            
            TriggerInvulnerability();
        }
        
        if (DataManager.Instance.playerData.light < 0 && !isDead)
        {
            isDead = true;
            ani.SetTrigger("DieTrigger");
            StartCoroutine(DieCoroutine());
        }
        
    }

    private void PlayHurtParticleEffect()
    {
        if (hurtParticles != null)
        {
            hurtParticles.Play();
        }
    }

    IEnumerator Knockback(Vector2 direction)
    {
        isHurt = true;
        rb.velocity = direction * hurtForce;
        // rb.AddForce(direction * hurtForce, ForceMode2D.Impulse);

        /*Vector2 startPos = rb.position;
        float distanceTraveled = 0f;
        float elapsedTime = 0f;
        
        while (distanceTraveled < knockDistance && elapsedTime < hurtTime)
        {
            distanceTraveled = Vector2.Distance(rb.position, startPos);
            elapsedTime += Time.deltaTime;
            yield return null;
        }*/
        
        yield return new WaitForSeconds(hurtTime);
        
        rb.velocity = Vector2.zero; 

        yield return new WaitForSeconds(0.3f); // stun time
        isHurt = false;
    }
    

    #region Invulnerability
    private void TriggerInvulnerability()
    {
        isInvulnerable = true;
        StartCoroutine(InvulnerabilityDuration(invulnerabilityTime));
    }

    IEnumerator InvulnerabilityDuration(float duration)
    {
        material.SetColor("_InvulnerabilityColor", invulnerabilityColor); 
        
        yield return new WaitForSeconds(duration);

        material.SetColor("_InvulnerabilityColor", Color.white); 
        isInvulnerable = false;
    }
    #endregion

    // Player dies, for testing purposes
    public void Die()
    {
        Debug.Log("[Player Controller] Player died");
        GameSceneManager.Instance.RestartScene();
    }

    IEnumerator DieCoroutine(){
        // wait for a few seconds before dying
        // disable player control
        // Debug.Log("death coroutine started");
        inputActions.Disable();
        yield return new WaitForSeconds(deathDelay);
        // Debug.Log("death coroutine ended");
        Die();
    }

    #region Animation Control
    // Set animation state
    private void SetAnimation()
    {
        ani.SetFloat("X_velocity", math.abs(rb.velocity.x));
        ani.SetFloat("Y_velocity", rb.velocity.y);
        // ani.SetBool("IsGrounded", physicsCheck.isOnGround);
    }
    #endregion

    private void PlayDissolve()
    {
        material.SetFloat("_DissolveThreshold", dissolveThreshold);

        dissolveThreshold -= dissolveSpeed * Time.deltaTime;
        if(dissolveThreshold <= 0.001f)
        {
            dissolveThreshold = 0;
        }
        
    }

    IEnumerator PlaySwitchEffect()
    {
        GameObject obj = Instantiate(switchEffect, transform.position + switchEffectOffset, Quaternion.identity);
        yield return new WaitForSeconds(0.5f);
        Destroy(obj);
    }

    #region Event

    public void OnPlayerSwitchWorld()
    {
        // Play animation
        if (WorldControl.Instance.isRealWorld)
        {
            // disable cape's spine slot
            DisableCloak();
            ani.SetBool("IsLanternOn", false);

            // switch effect
            if(switchEffect != null){
                StartCoroutine(PlaySwitchEffect());
            }
        }
        else
        {
            // enable cape's spine slot
            Invoke(nameof(EnableCloak), 0.2f);
            ani.SetBool("IsLanternOn", true);

            // switch effect
            if(switchEffect != null){
                StartCoroutine(PlaySwitchEffect());
            }
        }
    }
    #endregion

    #region Skeleton
    private void GetAllCloakSlots()
    {
        // 获取 Skeleton
        var skeleton = skeletonMecanim.skeleton;

        cloakSlots = new Spine.Slot[9];
        cloakSlots[0] = skeleton.FindSlot("Cape");
        cloakSlots[1] = skeleton.FindSlot("Cape Left Sleeve");
        cloakSlots[2] = skeleton.FindSlot("Cape Right Sleeve");
        cloakSlots[3] = skeleton.FindSlot("Eyes");
        cloakSlots[4] = skeleton.FindSlot("Hood Front");
        cloakSlots[5] = skeleton.FindSlot("Hood Back1");
        cloakSlots[6] = skeleton.FindSlot("Hood Back2");
        cloakSlots[7] = skeleton.FindSlot("Left Ear2");
        cloakSlots[8] = skeleton.FindSlot("Right Ear2");

        // Cache the original attachments
        foreach (var slot in cloakSlots)
        {
            if (slot != null && slot.Attachment != null)
            {
                originalCloakAttachments[slot] = slot.Attachment;  // Cache the attachment
            }
        }
    }

    private void DisableCloak()
    {
        // 遍历斗篷相关的所有插槽，将其 Attachment 设置为 null 来禁用它们
        foreach (var slot in cloakSlots)
        {
            if (slot != null)
            {
                slot.Attachment = null;  // 将 Attachment 设置为 null 来禁用插槽
            }
        }
    }

    private void EnableCloak()
    {
        // 遍历斗篷相关的所有插槽，恢复其原来的 Attachment
        foreach (var slot in cloakSlots)
        {
            if (slot != null && originalCloakAttachments.ContainsKey(slot))
            {
                slot.Attachment = originalCloakAttachments[slot];  // 恢复缓存的附件
            }
        }
    }

    private void GetLampSlot(){
        var skeleton = skeletonMecanim.skeleton;
        lampSlots = new Spine.Slot[2];
        lampSlots[0] = skeleton.FindSlot("Lamp");
        lampSlots[1] = skeleton.FindSlot("Handle");

        foreach (var slot in lampSlots)
        {
            if (slot != null && slot.Attachment != null)
            {
                Debug.Log("lamp slot: " + slot);
                originalCloakAttachments[slot] = slot.Attachment;
            }
        }

    }

    public void DelayDisableLamp(float delay)
    {
        // current this method is not used as told by XIAODAN, the lantern will be carried most of the time by the player even when player is idle
        // later on this method might be used when player havent got the lantern yet at the early stage of the game

        // Invoke(nameof(DisableLamp), delay);
    }
    
    public void DisableLamp()
    {
        Debug.Log("Disable lamp");
        if (lampSlots != null)
        {
            foreach (var slot in lampSlots)
            {
                if (slot != null)
                {
                    slot.Attachment = null;
                }
            }
            ani.SetBool("IsLanternOn", false);
        }
    }

    public void EnableLamp()
    {
        Debug.Log("Enable lamp");
        if (lampSlots != null)
        {
            foreach (var slot in lampSlots)
            {
                if (slot != null && originalCloakAttachments.ContainsKey(slot))
                {
                    slot.Attachment = originalCloakAttachments[slot];
                }
            }
            ani.SetBool("IsLanternOn", true);
        }
    }
    #endregion


    private void Pause(InputAction.CallbackContext context)
    {
        PauseScreen.GetComponent<PauseManager>().TogglePause();
    }
 
    #region Character Attack
    private void MeleeAttack(InputAction.CallbackContext context)
    {
        if (canAttack 
            && DataManager.Instance.playerData.hasPickedUpLantern 
            && SceneManager.Instance.GetSceneConfiguration().enableAttack)
        {
            // Set melee animation
            ani.SetTrigger("MeleeTrigger");

            canAttack = false;
            bool hitEnemy = false;
            Collider2D[] enemiesInsideArea;
            if (faceDir == 1)
            {
                enemiesInsideArea = Physics2D.OverlapAreaAll(new Vector2(transform.position.x + meleeAttackRange - 0.1f, transform.position.y - 0.1f), new Vector2(transform.position.x + meleeAttackRange + 0.1f, transform.position.y + 0.1f));
                Instantiate(meleeProj, new Vector3(transform.position.x + meleeAttackRange, transform.position.y), Quaternion.identity);
            }
            else
            {
                enemiesInsideArea = Physics2D.OverlapAreaAll(new Vector2(transform.position.x - meleeAttackRange - 0.1f, transform.position.y - 0.1f), new Vector2(transform.position.x - meleeAttackRange + 0.1f, transform.position.y + 0.1f));
                Instantiate(meleeProj, new Vector3(transform.position.x - meleeAttackRange, transform.position.y), Quaternion.identity);
            }
            foreach (Collider2D currEnemy in enemiesInsideArea)
            {
                Debug.Log(currEnemy);
                if (currEnemy.gameObject.CompareTag("Monster"))
                {
                    currEnemy.gameObject.GetComponent<Monster>().TakeDamage(meleeDamage);
                    hitEnemy = true;
                }
            }
            if (hitEnemy)
            {
                DataManager.Instance.playerData.light += meleeLightHeal;
                UIManager.Instance.BroadcastMessage("UpdateLight");
            }
            StartCoroutine(attackCooldown(meleeAttackSpeedPerSec));
        }
    }
    
    private IEnumerator attackCooldown(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);
        canAttack = true;
    }

    private void RangeAttack(InputAction.CallbackContext context)
    {
        if(canAttack 
            && DataManager.Instance.playerData.light >= rangeAttackCost 
            && DataManager.Instance.playerData.hasPickedUpLantern
            && SceneManager.Instance.GetSceneConfiguration().enableAttack)
        {
            // Set attack animation
            ani.SetTrigger("RangedTrigger");
            
            Debug.Log($"Player light when range attack:{DataManager.Instance.playerData.light}");
            
            DataManager.Instance.playerData.light -= rangeAttackCost;
            UIManager.Instance.BroadcastMessage("UpdateLight");
            canAttack = false;
            Debug.Log("creating ranged");
            Instantiate(rangedProj, new Vector3(transform.position.x, transform.position.y), Quaternion.identity);
            Debug.Log("created ranged");
            StartCoroutine(attackCooldown(rangeAttackSpeedPerSec));
        }
    }
    #endregion

    #region Input System

    public void DisableGameplayInput()
    {
        inputActions.Gameplay.Disable();
    }

    public void EnableGameplayInput()
    {
        inputActions.Gameplay.Enable();
    }

    #endregion
    
    public bool isFacingRight()
    {
        return faceDir == 1;
    }

    // Respond to OnLanternFirstPickedUp event in GameManager
    public void SetLanternStatus()
    {
        // hasLantern = true;
        lantern.gameObject.SetActive(true);
        
        // set boss entrance animation active
        monsterAnimation?.SetActive(true);
        monsterAppearController?.TriggerBossAppearance();
        
    }

    private void HandleCutsceneStart()
    {
        isCutscenePlay = true;
    }

    public void SaveToCheckpoint(CheckpointData data)
    {
        data.hasPickedUpLantern = true;
        data.canSwitchWorld = true;
        data.currentLight = 3;
    }

    public void LoadFromCheckpoint(CheckpointData data)
    {
        transform.position = data.playerPosition;
        lantern.gameObject.SetActive(true);
        
        DataManager.Instance.playerData.hasPickedUpLantern = data.hasPickedUpLantern;
        DataManager.Instance.playerData.canSwitchWorld = data.canSwitchWorld;
        DataManager.Instance.playerData.light = data.currentLight;
    }
}
