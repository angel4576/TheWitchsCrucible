using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.InputSystem;

public class Monster : MonoBehaviour
{
    public Transform playerTransform;
    public PlayerController playerScript;
    public Lantern lantern;
    public Healthbar healthBar;
    public float xDistanceToPlayer; // distance to player when switch to real world
    
    [Header("Monster Appear")]
    public GameObject monsterAnimation;
    public MonsterAppearanceController monsterAppearController;
    public float animationDuration;
    
    [Header("Monster Attributes")]
    public EnemyType type;
    public float moveSpeed;
    public float chaseRange;
    public Vector2 centerOffset;
    
    [Header("Melee Attack")]
    public float meleeAttackRange;
    public float meleeAttackDamage;
    public float meleeAttackDelay; // the time before each attack
    public float meleeAttackCooldown; // the time after a attack before another attack can be made

    [Header("Range Attack")]
    public float rangeAttackRange;
    public float rangeAttackDamage;
    public float rangeAttackDelay; // the time before each attack
    public float rangeAttackCooldown; // the time after a attack before another attack can be made
    public float rangeAttackProjectileSpeed;
    public GameObject rangeAttackProjectile;

    [Header("Monster Status")]
    public bool isKillable;
    public float maxHealth = 100;
    public float currentHealth;
    public bool isDead;

    [Header("For testing purposes")]
    public bool idleIfPlayerOutOfRange = false;
    public bool disableUpdate  = false;
    
    // status
    [HideInInspector]public bool isChasing;
    [HideInInspector]public bool isAttacking;
    /*[HideInInspector]*/public bool canMove;
    /*[HideInInspector]*/public bool canChase;
    /*[HideInInspector]*/public bool canAttack;
    [HideInInspector]private Vector2 initialPosition;
    // public bool isAppear;
    public bool foundLantern;

    // flags
    [HideInInspector]private bool isPlayerDead = false; // ensure that player is killed only once
    
    private Animator animator;

    // temp: melee attack visualization
    public Transform arm;
    private Transform pivot;
    private Vector3 originalPosition;
    private Vector3 originalRotation;

    // prefab of a spirit world platform
    public GameObject spiritWorldPlatformPrefab;
    public GameObject spiritWorldPlatform;
    public MeshRenderer monsterMeshRenderer;
    public float platformWidth = 3.0f;
    public float platformHeight = 0.5f;
    bool isdisabled = false;


    
    
    #region Life Cycle
    private void Awake()
    {
        // retrieve references
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        // lantern = GameObject.Find("Lantern").GetComponent<Lantern>();   
        healthBar = GetComponentInChildren<Healthbar>();

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("No animator attached");
        }
        
        // for now, switching world enables/disables the monsters, if changed, a reference to the real/mental world must be retrieved
        // and this script must be updated

        // register to game manager
        if(GameManager.Instance != null && !GameManager.Instance.monsters.Contains(this))
        {
            GameManager.Instance.monsters.Add(this);
        }
        
        AddListeners();
    }

    private void OnDestroy() 
    {
        // unregister from game manager
        if(GameManager.Instance != null && GameManager.Instance.monsters.Contains(this))
        {
            GameManager.Instance.monsters.Remove(this);
        }
        
        RemoveListeners();
    }

    // Start is called before the first frame update
    private void Start()
    {
        initialPosition = transform.position;
        isChasing = false;
        canChase = false;
        canMove = true;
        canAttack = false;
        // foundLantern = false;
        // isAppear = false;

        currentHealth = maxHealth;

        // disable health bar if it is not killable
        if(!isKillable){
            healthBar.gameObject.SetActive(false);
        }

        if (arm != null)
        {
            pivot = arm.transform.GetChild(0); 
            // originalPosition = arm.position;
            originalRotation = arm.rotation.eulerAngles;
            // Debug.Log(originalPosition);
        }
        
        // get mesh renderer
        monsterMeshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        // reset status
        isChasing = false;
        isAttacking = false;
        canMove = false;
        canChase = false;
        canAttack = false;
        foundLantern = true;

        // ResetArm();
    }

    // Update is called once per frame
    private void Update()
    {
       
        // for plaform
        if(isdisabled){
            return;
        }

        // for test purpose
        if (Keyboard.current.digit7Key.wasPressedThisFrame)
        {
            TakeDamage(7);
        }

        
        //if(lantern == null) // if player does not have lantern
        if(!foundLantern)
        {
            canMove = false;
            canChase = false;
            canAttack = false;
        }
        else // if player has lantern
        {
            if(lantern.IsLanternOn){
                // if lantern is off, monster can move and chase player in range
                canMove = false;
                canChase = false;
                canAttack = false;
            }
            else
            {   
                /*if (!isAppear) // monster first appears
                {
                    // isAppear = true;
                    monsterStartAnimation.SetActive(true);
                    StartCoroutine("StartAnimationCountdown"); // cannot move until animation finishes
                    
                }
               */
                canMove = true;
                canChase = CheckPlayerInChaseRange();
                canAttack = true;
                
                
            }
        }
        
        if(canAttack && !isAttacking)
        {
            if(type == EnemyType.Melee && CheckPlayerInMeleeAttackRange())
            {
                MeleeAttack();
            }
            else if(type == EnemyType.Range && CheckPlayerInRangeAttackRange())
            {
                RangeAttack();
            }
        }

        if(isAttacking)
        {
            canMove = false; // stop moving while attacking
            MeleeAttackVisualization();
        }
        
        FlipDirection();
        
    }

    private void FixedUpdate() 
    {
        if(canMove)
        {
            if(canChase)
            {
                ChasePlayer();
            }
            else
            {
                // for test purposes
                if(idleIfPlayerOutOfRange)
                {
                    Idle();
                }
                else
                {
                    ReturnToInitialPosition();
                }
            }
        }
        else
        {
            Idle();
        }
    }
    #endregion
    
    #region Attack
    // behaviors
    // attack
    public void MeleeAttack()
    {
        StartCoroutine(MeleeAttackCoroutine());
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        isAttacking = true;
        MeleeAttackVisualization();
        
        // wait for the delay
        yield return new WaitForSeconds(meleeAttackDelay);

        // perform the attack
        if(CheckPlayerInMeleeAttackRange())
        {
            // Debug.Log("Monster melee attack");
            animator.SetTrigger("Lv1Attack");
            DamagePlayer(meleeAttackDamage, transform);
            
        }

        // wait for the cooldown
        yield return new WaitForSeconds(meleeAttackCooldown);

        isAttacking = false;
        // temp
        // ResetArm();
    }

    // for temp testing 
    private void MeleeAttackVisualization()
    {
        if (transform.position.x < playerTransform.position.x) 
        {
            arm.transform.Translate(Vector3.right * 7.0f * Time.deltaTime);
        }
        else
        {
            arm.transform.Translate(Vector3.left * 7.0f * Time.deltaTime);
        }
        // arm.transform.RotateAround(pivot.transform.position, new Vector3(0, 0, 1), -1);
    }

    private void ResetArm()
    {
        originalPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        arm.transform.position = originalPosition; 
        arm.transform.eulerAngles = originalRotation;
    }

    public void RangeAttack()
    {
        Vector2 direction = (playerTransform.position + new Vector3(0, 1.2f, 0) - transform.position).normalized; // offset to the player's body
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        StartCoroutine(RangeAttackCoroutine(direction, distance, rangeAttackProjectileSpeed, rangeAttackDamage));
    }

    private IEnumerator RangeAttackCoroutine(Vector2 direction, float distance, float flyingSpeed, float damage)
    {
        isAttacking = true;
        
        // wait for the delay
        yield return new WaitForSeconds(rangeAttackDelay);

        // perform the attack
        Debug.Log("range attack");
        GameObject projectile = Instantiate(rangeAttackProjectile, transform.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().Initialize(flyingSpeed, 3f, damage, direction); // 3 seconds lifetime

        // wait for the cooldown
        yield return new WaitForSeconds(rangeAttackCooldown);

        isAttacking = false;
    }
    #endregion

    private void AddListeners()
    {
        lantern.OnLanternActivated.AddListener(ReactOnLantern);
    }

    private void RemoveListeners()
    {
        lantern.OnLanternActivated.RemoveListener(ReactOnLantern);
    }
    
    public void TakeDamage(float damage)
    {
        // Debug.Log("took damage");
        if(isKillable)
        {
            currentHealth -= damage;
            healthBar.SetHealth(currentHealth, maxHealth);
            if(currentHealth <= 0)
            {
                // die
                isDead = true;
                gameObject.SetActive(false);
            }
        }
    }

    #region Movement
    // movement
    private void ChasePlayer()
    {
        isChasing = true;
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
    }

    private void ReturnToInitialPosition()
    {
        transform.position = Vector2.MoveTowards(transform.position, initialPosition, moveSpeed * Time.deltaTime);
    }

    private void Idle()
    {
        // do nothing
    }

    private void DamagePlayer(float damage, Transform attacker)
    {
        playerScript.TakeDamage(damage, attacker);
        // playerScript.GetHurt();
    }

    private void KillPlayer()
    {
        // kill player
        isPlayerDead = true;
        // playerScript.Die();
        DamagePlayer(99999, transform);
    }

    private void FlipDirection()
    {
        if (CheckPlayerInChaseRange())
        {
            // monster -> player
            if (playerTransform.position.x - transform.position.x > 0) // player on the right
            {
                transform.localScale = new Vector3(Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-1 * Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            
        }
    }

    /*IEnumerator StartAnimationCountdown()
    {
        yield return new WaitForSeconds(animationDuration);
        
        /*canMove = true;
        canChase = CheckPlayerInChaseRange();
        canAttack = true;#1#
        // isAppear = true;
        
        monsterStartAnimation.SetActive(false);
        // show monster
        // gameObject.SetActive(true);
    }*/
    
    #endregion
    
    #region Helper Functions
    // helper functions
    private bool CheckPlayerInChaseRange()
    {
        return Vector2.Distance((Vector2)transform.position + centerOffset, playerTransform.position) < chaseRange;
    }

    private bool CheckPlayerInMeleeAttackRange()
    {
        return Vector2.Distance((Vector2)transform.position + centerOffset, playerTransform.position) < meleeAttackRange;
    }

    private bool CheckPlayerInRangeAttackRange()
    {
        return Vector2.Distance((Vector2)transform.position + centerOffset, playerTransform.position) < rangeAttackRange;
    }
    #endregion
    
    // Respond to OnLanternFirstPickedUp event in Game Manager
    public void AcquireLantern()
    {
        // lantern = playerScript.GetComponentInChildren<Lantern>();
        foundLantern = true;
        
        // set boss entrance animation active
        // monsterAnimation?.SetActive(true);
        // monsterAppearController?.TriggerBossAppearance();
    }

    public void ResetPosition()
    {
        Debug.Log("Reset monster position");
        transform.position = new Vector2(playerTransform.position.x + xDistanceToPlayer, transform.position.y);
    }

    // visualizing chase range
    private void OnDrawGizmosSelected() 
    {
        // chase range
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere((Vector2)transform.position + centerOffset, chaseRange);

        // melee attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + centerOffset, meleeAttackRange);

        // range attack range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere((Vector2)transform.position + centerOffset, rangeAttackRange);
    }

    public void OnValidate(){
        if(isKillable){
            healthBar.gameObject.SetActive(true);
        }
        else{
            healthBar.gameObject.SetActive(false);
        }
    }

    public void ReactOnLantern()
    {
        if (lantern.IsLanternOn)
        {
            animator.SetTrigger("FlashOff");
        }
        else
        {
            animator.SetTrigger("FlashOn");
        }
    }

    #region Plaform Spawn
    public void MonsterOnSwitchWorld(bool switchToSpiritWorld)
    {
        if(switchToSpiritWorld)
        {
            isdisabled = true;
            if(spiritWorldPlatform == null)
            {
                if(spiritWorldPlatformPrefab == null)
                {
                    Debug.LogError("Spirit world platform prefab is not set");
                    return;
                }
                spiritWorldPlatform = Instantiate(spiritWorldPlatformPrefab, transform.position, spiritWorldPlatformPrefab.transform.rotation);
                spiritWorldPlatform.transform.parent = transform;
                spiritWorldPlatform.transform.localPosition = new Vector3(0, 0, 0);
                spiritWorldPlatform.transform.localScale = new Vector3(platformHeight, platformWidth, 0);
                //spiritWorldPlatform.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                spiritWorldPlatform.SetActive(true);
            }
            monsterMeshRenderer.enabled = false;
            // gameObject.SetActive(false);
            
            canMove = false;
            canChase = false;
            canAttack = false;
        }
        else
        {
            isdisabled = false;
            if(spiritWorldPlatform != null)
            {
                spiritWorldPlatform.SetActive(false);
            }
            monsterMeshRenderer.enabled = true;
            // gameObject.SetActive(true);
        }
    }
    
    #endregion

    public enum EnemyType
    {
        Melee,
        Range,
        Shiled
    };
}
