using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    public PlayerInputControl inputActions;
    private Vector2 inputDirection;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private SpriteRenderer rend;
    private int faceDir;

    private PhysicsCheck physicsCheck;
    
    public Pet pet;

    [Header("Movement")]
    public float speed;

    [Header("Jump")]
    public float jumpForce;

    [Header("Pet Control")]
    public float petMoveDelayTime;
    public float petJumpDelayTime;

    private void Awake() 
    {
        inputActions = new PlayerInputControl();

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
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        rend = GetComponent<SpriteRenderer>();

        // Get script reference
        physicsCheck = GetComponent<PhysicsCheck>();

    }

    // Update is called once per frame
    void Update()
    {
        // Read Vector2 value in Move action
        inputDirection = inputActions.Gameplay.Move.ReadValue<Vector2>();

        FlipDirection();
    }

    private void FixedUpdate() 
    {
        Move();
    }

    private void Move()
    {
        if(inputDirection.y != 0)
        {
            inputDirection.x *= math.sqrt(2);
        }

        rb.velocity = new Vector2(inputDirection.x * speed, rb.velocity.y);
        if(inputDirection.x != 0 && !pet.canMove)
        {
            Invoke(nameof(ControlPetMovement), petMoveDelayTime);
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if(physicsCheck.isOnGround){
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);

            // Pet jump
            Invoke(nameof(ControlPetJump), petJumpDelayTime);
        }
    }

    private void ControlPetMovement()
    {
        pet.canMove = true;
    }

    private void ControlPetJump()
    {
        pet.canJump = true;
    }

    private void FlipDirection()
    {
        faceDir = Math.Sign(transform.localScale.x);

        if(inputDirection.x < 0)
        {
            faceDir = -1;
        }
        else if(inputDirection.x > 0)
        {
            faceDir = 1;
        }

        transform.localScale = new Vector3(faceDir * Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        
    }


    // Player dies, for testing purposes
    public void Die()
    {
        Debug.Log("Player died");
    }

    public void OnPlayerSwitchWorld()
    {
        // Play animation
        if(WorldControl.Instance.isRealWorld)
            rend.color = Color.white;
        else
            rend.color = Color.black;
    }
}
