using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInputControl inputActions;
    private Vector2 inputDirection;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;

    private PhysicsCheck physicsCheck;

    [Header("Movement")]
    public float speed;

    [Header("Jump")]
    public float jumpForce;

    private void Awake() 
    {
        inputActions = new PlayerInputControl();

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();

        // Get script reference
        physicsCheck = GetComponent<PhysicsCheck>();

        // +=: register actions to action binding
        inputActions.Gameplay.Jump.started += Jump; // call jump when the moment corresponding button is pressed

    }


    private void OnEnable() 
    {
        inputActions.Enable();
    }

    private void OnDisable() 
    {
        inputActions.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Read Vector2 value in Move action
        inputDirection = inputActions.Gameplay.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate() 
    {
        Move();    
    }

    private void Move()
    {
        rb.velocity = new Vector2(inputDirection.x * speed, rb.velocity.y);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if(physicsCheck.isOnGround){
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        }
    }


}
