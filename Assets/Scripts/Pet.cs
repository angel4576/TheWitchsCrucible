using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pet : MonoBehaviour, IInteractable
{
    public Transform targetTrans;
    public Transform player;
    
    [Header("Pet Movement")]
    public float speed;
    public float jumpForce;
    private int faceDir;
    private float xMoveDir;
    
    [Header("Pet Respawn")]
    public float loseDistance;
    public float respawnTime;
    
    [Header("Obstacle Check")]
    public LayerMask groundLayer;
    public float rayLength;
    public Vector2 rayOffset;

    [Header("Pet Status")]
    public float idleYDistance; // how long away from player to set to be idle
    private bool isIdle;

    [HideInInspector]public bool canMove;
    [HideInInspector]public bool canJump;
    private bool isMiss;
    
    // private PhysicsCheck physicsCheck;
    private Rigidbody2D rb;
    private Animator ani;
    private CapsuleCollider2D coll;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        coll = GetComponent<CapsuleCollider2D>();

        canMove = true;

    }

    private void OnEnable() 
    {

    }

    private void OnDisable() 
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckDistance(); // Check distance between pet and player
        
        if(isMiss)
        {
            ResetPosition();
        }

        
    }

    private void FixedUpdate() 
    {   
        if(canMove && !isIdle)
        {
            MoveToPlayer(); 
            FlipDirection();
        }
        
        if(canJump && !isIdle)
        {
            Jump();
        }
        
        // CheckForwardObstacle();
    }

    private void MoveToPlayer()
    {
        Vector2 moveDir = (targetTrans.position - transform.position).normalized;
        xMoveDir = moveDir.x;
        rb.velocity = new Vector2(moveDir.x * speed, rb.velocity.y);
        // set animation state
        ani.SetBool("IsRunning", true);

        // Stop when reach chase point or within a certain distance with player
        if(Vector2.Distance(transform.position, targetTrans.position) < 0.01f || 
        Vector2.Distance(transform.position, player.position ) < 2f )
        {
            rb.velocity = Vector2.zero;
            canMove = false;
            // set animation state
            ani.SetBool("IsRunning", false);
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);

        canJump = false;

    }

    private void CheckDistance()
    {
        // Check distance between Pet and Pet chase target
        if(Vector2.Distance(transform.position, targetTrans.position) > loseDistance)
        {  
            ani.SetBool("IsRunning", false);
            isMiss = true;
        }
        else
        {
            isMiss = false;
        }

        Vector2 yPetPos = new Vector2(0, transform.position.y);
        Vector2 yTargetPos =  new Vector2(0, targetTrans.position.y);
        if(Vector2.Distance(yPetPos, yTargetPos) > idleYDistance)
        {  
            ani.SetBool("IsRunning", false);
            isIdle = true;
        }
        else
        {
            isIdle = false;
        }
        
    }

    private void ResetPosition()
    {
        transform.position = targetTrans.position;
        rb.velocity = Vector2.zero;
    }

    public void Interact()
    {
        // Debug.Log("Interact with Pet");
        
    }

    private void FlipDirection()
    {
        faceDir = Math.Sign(transform.localScale.x);

        if (xMoveDir < 0)
        {
            faceDir = -1;
        }
        else if (xMoveDir > 0)
        {
            faceDir = 1;
        }

        transform.localScale = new Vector3(faceDir * Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void CheckForwardObstacle()
    {
        Vector2 rayDirection = new Vector2(faceDir, 0);
        Vector2 offset = new Vector2(rayOffset.x * faceDir, rayOffset.y);
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + offset, rayDirection, rayLength, groundLayer);

        if (hit)
        {
            ani.SetBool("IsRunning", false);
        }

        Color rayColor = hit ? Color.red : Color.green;

        Debug.DrawRay((Vector2)transform.position + offset, rayDirection * rayLength, rayColor);
    }

    // bind to OnSwitchWorld in World Control (inspector)
    public void OnPetSwitchWorld()
    {
        Debug.Log("On pet switch");
        // Play animation
        gameObject.SetActive(WorldControl.Instance.isRealWorld);
        ResetPosition();
    }
    
}
