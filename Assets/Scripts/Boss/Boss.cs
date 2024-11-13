using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    
    public Transform player;
    [HideInInspector]public Animator animator;
    
    [Header("Boss Stats")]
    public float health;
    public float moveSpeed;
    public int faceDirection; // 1 right
    
    [Header("Melee Attack")]
    public float meleeDamage;
    public float swingRange;
    public float smashRange;
    public Vector2 attackCenterOffset;
    
    [Header("Perception")]
    public float detectRange;

    public LayerMask target;
    private Vector2 attackCenter;
    
    // State Machine
    private BossState currentState;
    private BossState idleState;
    private BossState chaseState;
    private BossState armSwingAttackState;
    private BossState smashAttackState;

    #region Unity Callbacks
    private void Awake()
    {
        // initialize states
        idleState = new IdleState();
        chaseState = new ChaseState();
        armSwingAttackState = new ArmSwingAttackState();
        smashAttackState = new SmashAttackState();
        
        currentState = idleState;
    }

    private void OnEnable()
    {
        currentState.OnEnter(this);
    }

    private void OnDisable()
    {
        currentState.OnExit();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>(); 
        
        faceDirection = 1;
    }

    // Update is called once per frame
    void Update()
    {
        currentState.LogicUpdate();
        // FlipTo(player);
    }

    private void FixedUpdate()
    {
        currentState.PhysicsUpdate();
    }
    #endregion

    #region Movement

    public void Move()
    {
        
    }
    
    public void FlipTo(Transform pointToGo)
    {
        if(pointToGo.position.x - transform.position.x > 0.1f) // target is on the right of enemy
        {
            // face right
            // transform.localScale = new Vector3(Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            faceDirection = 1;
        }
        else
        {
            // face left
            // transform.localScale = new Vector3(-1 * Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            faceDirection = -1;
        }
        
        transform.localScale = new Vector3(faceDirection * Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        // attackCenter = (Vector2)transform.position + new Vector2(attackCenterOffset.x * faceDirection, attackCenterOffset.y);
    }
    #endregion
    
    #region Attack
    // Arm swing attack
    public bool CheckPlayerInSwingRange()
    {
        Vector2 offset = new Vector2(attackCenterOffset.x * faceDirection, attackCenterOffset.y);
        return Physics2D.OverlapCircle((Vector2)transform.position + offset, swingRange, target);
    }
    
    public bool CheckPlayerInSmashRange()
    {
        Vector2 offset = new Vector2(attackCenterOffset.x * faceDirection, attackCenterOffset.y);
        return Physics2D.OverlapCircle((Vector2)transform.position + offset, smashRange, target);
    }
    
    #endregion

    public bool DetectPlayer()
    {
        return Physics2D.OverlapCircle(transform.position, detectRange, target);
    }

    public void SwitchState(BossStateType state)
    {
        var newState = state switch
        {
            BossStateType.Idle => idleState,
            BossStateType.Chase => chaseState,
            BossStateType.SmashAttack => smashAttackState,
            BossStateType.ArmSwingAttack => armSwingAttackState,

            _ => null
        };
        currentState.OnExit();
        
        currentState = newState;
        currentState?.OnEnter(this);
    }
    
    private void OnDrawGizmos()
    {
        Vector2 offset = new Vector2(attackCenterOffset.x * faceDirection, attackCenterOffset.y);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + offset, swingRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + offset, smashRange);
        
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
