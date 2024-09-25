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

    // public Vector2 leftOffset;
    // public Vector2 rightOffset;

    private void Awake() 
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<CapsuleCollider2D>();
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate() 
    {
        
        Check();
        SwitchPhysicsMaterial();
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

}
