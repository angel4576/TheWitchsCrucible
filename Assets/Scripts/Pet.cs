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
    
    [Header("Pet Respawn")]
    public float loseDistance;
    public float respawnTime;

    // Status
    [HideInInspector]public bool canMove;
    [HideInInspector]public bool canJump;
    private bool isMiss;
    
    private PhysicsCheck physicsCheck;
    private Rigidbody2D rb;
    private Animator ani;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();

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
        if(canMove)
        {
            MoveToPlayer(); 
        }
        
        if(canJump)
        {
            Jump();
        }
        
    }

    private void MoveToPlayer()
    {
        Vector2 moveDir = (targetTrans.position - transform.position).normalized;
        rb.velocity = new Vector2(moveDir.x * speed, rb.velocity.y);
        // set animation state
        ani.SetBool("IsRunning", true);
        // Vector2 offset = new Vector2(2, 0);
        // Stop at a certain distance from player
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
            isMiss = true;
        }
        else
        {
            isMiss = false;
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

    public void OnPetSwitchWorld()
    {
        // Debug.Log("On pet switch");
        // Play animation
        gameObject.SetActive(WorldControl.Instance.isRealWorld);
    }

    
}
