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

    [Header("Monster Attributes")]
    public EnemyType type;
    public float moveSpeed;
    public float chaseRange;

    public float meleeAttackRange;
    public float meleeAttackDamage;
    public float meleeAttackDelay; // the time before each attack
    public float meleeAttackCooldown; // the time after a attack before another attack can be made

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

    [Header("For testing purposes")]
    public bool idleIfPlayerOutOfRange = false;

    // status
    [HideInInspector]private bool isChasing;
    [HideInInspector]private bool isAttacking;
    [HideInInspector]private bool canMove;
    [HideInInspector]private bool canChase;
    [HideInInspector]private bool canAttack;
    [HideInInspector]private Vector2 initialPosition;

    // flags
    [HideInInspector]private bool isPlayerDead = false; // ensure that player is killed only once

    private void Awake()
    {
        // retrieve references
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        // lantern ?= GameObject.Find("Lantern").GetComponent<Lantern>();   
        healthBar = GetComponentInChildren<Healthbar>();
        
        // for now, switching world enables/disables the monsters, if change, a reference to the real/mental world must be retrieved
        // and this script must be updated
    }

    // Start is called before the first frame update
    private void Start()
    {
        initialPosition = transform.position;
        isChasing = false;
        canChase = false;
        canMove = true;
        canAttack = false;

        currentHealth = maxHealth;

        // disable health bar if it is not killable
        if(!isKillable){
            healthBar.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // for test purpose
        if (Keyboard.current.digit7Key.wasPressedThisFrame)
        {
            TakeDamage(7);
        }

        if(lantern == null) // if player does not have lantern
        {
            canMove = false;
            canChase = false;
            canAttack = false;
        }
        else
        {
            if(lantern.IsLanternOn){
                // if lantern is off, monster can move and chase player in range
                canMove = false;
                canChase = false;
                canAttack = false;
            }
            else
            {   
                canMove = true;
                canChase = CheckPlayerInChaseRange();
                canAttack = true;
            }
        }

        // if(canAttack && CheckPlayerInMeleeAttackRange() && !isPlayerDead)
        // {
        //     KillPlayer();
        // }
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
        }
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

    // behaviors
    // attack
    public void MeleeAttack()
    {
        StartCoroutine(MeleeAttackCoroutine());
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        isAttacking = true;
        
        // wait for the delay
        yield return new WaitForSeconds(meleeAttackDelay);

        // perform the attack
        if(CheckPlayerInMeleeAttackRange())
        {
            Debug.Log("melee attack");
            DamagePlayer(meleeAttackDamage);
        }

        // wait for the cooldown
        yield return new WaitForSeconds(meleeAttackCooldown);

        isAttacking = false;
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

    public void TakeDamage(float damage)
    {
        if(isKillable)
        {
            currentHealth -= damage;
            healthBar.SetHealth(currentHealth, maxHealth);
            if(currentHealth <= 0)
            {
                // die
                gameObject.SetActive(false);
            }
        }
    }

 
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

    private void DamagePlayer(float damage)
    {
        playerScript.TakeDamage(damage);
    }

    private void KillPlayer()
    {
        // kill player
        isPlayerDead = true;
        // playerScript.Die();
        DamagePlayer(99999);
    }

    // helper functions
    private bool CheckPlayerInChaseRange()
    {
        return Vector2.Distance(transform.position, playerTransform.position) < chaseRange;
    }

    private bool CheckPlayerInMeleeAttackRange()
    {
        return Vector2.Distance(transform.position, playerTransform.position) < meleeAttackRange;
    }

    private bool CheckPlayerInRangeAttackRange()
    {
        return Vector2.Distance(transform.position, playerTransform.position) < rangeAttackRange;
    }

    // Respond to OnLanternFirstPickedUp event in Game Manager
    public void AcquireLantern()
    {
        lantern = playerScript.GetComponentInChildren<Lantern>();
    }

    // visualizing chase range
    private void OnDrawGizmosSelected() 
    {
        // chase range
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // melee attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);

        // range attack range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangeAttackRange);
    }

    public void OnValidate(){
        if(isKillable){
            healthBar.gameObject.SetActive(true);
        }
        else{
            healthBar.gameObject.SetActive(false);
        }
    }

    public enum EnemyType
    {
        Melee,
        Range,
        Shiled
    };
}
