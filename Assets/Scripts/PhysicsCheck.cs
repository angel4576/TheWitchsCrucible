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
    
    
    [Header("Status")]
    public bool isOnGround;

    // public Vector2 leftOffset;
    // public Vector2 rightOffset;

    private void Awake() 
    {
        col = GetComponent<CapsuleCollider2D>();

        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Check();
    }

    private void Check()
    {
        isOnGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, checkRadius, groundLayer);
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, checkRadius);    
    }

}
