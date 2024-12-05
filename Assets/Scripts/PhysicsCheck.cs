using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    private CapsuleCollider2D col;
    [Header("Ground Check")]
    public Vector2 bottomOffset;
    public float checkRadius;
    public LayerMask groundLayer;
    
    [Header("Forward Check")]
    public Vector2 forwardOffset;
    public float forwardCheckRadius;
    public float forwardCheckLength;
    
    [Header("Physics Material")]
    public PhysicsMaterial2D smooth;
    public PhysicsMaterial2D friction;

    [Header("Status")]
    public bool isOnGround;
    public bool isTouchForward;
    private Rigidbody2D rb;

    // public Vector2 leftOffset;
    // public Vector2 rightOffset;

    [Header("Fall Detection")]
    public float fallThreshold = -10f;  
    private CinemachineImpulseSource impulseSource;
    private bool hasStartedFalling = false;
    private float maxFallSpeed = 0f;
    
    private bool hasChangedFOV = false;
    
    private PlayerController player;
    
    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<CapsuleCollider2D>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        // Assume the player starts in the air

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate() 
    {
        Check();
        SwitchPhysicsMaterial();
        DetectFallAndShake();
    }

    private void Check()
    {
        isOnGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, checkRadius, groundLayer);
        if (player != null)
        {
            // isTouchForward = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(forwardOffset.x * player.faceDir, forwardOffset.y)
            //     , forwardCheckRadius, groundLayer);
            
            var hitObj = Physics2D.Raycast((Vector2)transform.position + new Vector2(forwardOffset.x * player.faceDir, forwardOffset.y), 
                new Vector2(player.faceDir, 0), forwardCheckLength, groundLayer);

            if (hitObj) // Ray hits something
            {
                var platEffector = hitObj.collider.GetComponent<PlatformEffector2D>();
                if (platEffector != null && platEffector.isActiveAndEnabled) // not touch forward if platformEffector is active
                {
                    isTouchForward = false;
                }
                else
                {
                    isTouchForward = true;
                }
                
            }
            
            // isTouchForward = Physics2D.Raycast((Vector2)transform.position + new Vector2(forwardOffset.x * player.faceDir, forwardOffset.y), 
            //     new Vector2(player.faceDir, 0), forwardCheckLength, groundLayer);
            
        }
    }

    void SwitchPhysicsMaterial()
    {
        if(isOnGround)
        {
            col.sharedMaterial = friction;
        }
        else
        {
            col.sharedMaterial = smooth;
        }
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, checkRadius);

        if (player != null)
        {
            Gizmos.color = Color.green;
            // Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(forwardOffset.x * player.faceDir, forwardOffset.y), forwardCheckRadius);
            Gizmos.DrawRay((Vector2)transform.position + new Vector2(forwardOffset.x * player.faceDir, forwardOffset.y),
                new Vector2(player.faceDir, 0) * forwardCheckLength);
            
        }
    }

    private void DetectFallAndShake()
    {
        if (isOnGround)
        {
            //Debug.Log($"DetectFallAndShake: Landed with velocity.y = {rb.velocity.y}");

            if (maxFallSpeed <= fallThreshold)
            {
                //Debug.Log($"DetectFallAndShake: Triggering camera shake with fall speed {maxFallSpeed}");
                CameraManager.instance.CamereaShake(impulseSource);
            }
            else
            {
                //Debug.Log($"DetectFallAndShake: No shake, fall velocity not high enough. Max Fall Speed = {maxFallSpeed}");
            }

            // Reset max fall speed after landing
            maxFallSpeed = 0f;

            if (!hasChangedFOV)
            {
                CameraManager.instance.ChangeFOV(100f);
                hasChangedFOV = true;  
            }
        }



        // Track maximum fall speed during the fall
        if (!isOnGround && rb.velocity.y < maxFallSpeed)
        {
            maxFallSpeed = rb.velocity.y;
            //Debug.Log($"Updating max fall speed to {maxFallSpeed}");
        }
    }



}
