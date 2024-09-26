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
    
    [Header("Physics Material")]
    public PhysicsMaterial2D smooth;
    public PhysicsMaterial2D friction;

    [Header("Status")]
    public bool isOnGround;
    private Rigidbody2D rb;

    // public Vector2 leftOffset;
    // public Vector2 rightOffset;

    [Header("Fall Detection")]
    public float fallThreshold = -10f;  
    private CinemachineImpulseSource impulseSource;
    private bool hasStartedFalling = false;
    private float maxFallSpeed = 0f;

  
    private bool hasChangedFOV = false;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
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
