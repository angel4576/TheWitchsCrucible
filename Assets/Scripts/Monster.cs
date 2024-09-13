using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public Transform playerTransform;
    public PlayerController playerScript;
    public Lantern lantern;

    [Header("Monster Attributes")]
    public float moveSpeed;
    public float chaseRange;
    public float attackRange;

    [Header("For testing purposes")]
    public bool idleIfPlayerOutOfRange = false;

    // status
    [HideInInspector]private bool isChasing;
    [HideInInspector]private bool canMove;
    [HideInInspector]private bool canChase;
    [HideInInspector]private Vector2 initialPosition;

    // flags
    [HideInInspector]private bool isPlayerDead = false; // ensure that player is killed only once

    private void Awake()
    {
        // retrieve references
        playerTransform = GameObject.Find("Player").transform;
        playerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        lantern = GameObject.Find("Lantern").GetComponent<Lantern>();
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
    }

    // Update is called once per frame
    private void Update()
    {
        if(!lantern.IsLanternOn){
            canMove = true;
            canChase = CheckPlayerInChaseRange();
        }
        else
        {
            canMove = false;
            canChase = false;
        }
        if(CheckPlayerInAttackRange() && !isPlayerDead)
        {
            KillPlayer();
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

    private void KillPlayer()
    {
        // kill player
        isPlayerDead = true;
        playerScript.Die();
    }

    // helper functions
    private bool CheckPlayerInChaseRange()
    {
        return Vector2.Distance(transform.position, playerTransform.position) < chaseRange;
    }

    private bool CheckPlayerInAttackRange()
    {
        return Vector2.Distance(transform.position, playerTransform.position) < attackRange;
    }


    // visualizing chase range
    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
