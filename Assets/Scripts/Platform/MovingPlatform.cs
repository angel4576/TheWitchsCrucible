using System;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveSpeed;
    public Vector2 startOffset;
    public Vector2 destOffset;

    private Vector2 startPos;
    private Vector2 destPos;
    private Vector2 target;
    
    private bool canMove;
    // public bool isMoving;
    private bool isMovingToDest;

    private Vector2 initialPos;
    
    void Start()
    {
        startPos = (Vector2)transform.position + startOffset;
        destPos = (Vector2)transform.position + destOffset;
        initialPos = transform.position;

        target = destPos;
        isMovingToDest = true;
    }

    void Update()
    {
        if (canMove)
            Move();
        
    }

    void Move()
    {
        target = (isMovingToDest) ? destPos : startPos;
        
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, target) < 0.01f) // reached
        {
            transform.position = target;
            // target = ((Vector2)transform.position == startPos) ? destPos : startPos;
            
            isMovingToDest = !isMovingToDest; // change direction
            canMove = false;

            NavManager.Instance.GenerateNavMesh();
        }
    }

    public void SetActive(bool active)
    {
        canMove = active;
    }

    private void OnDrawGizmos()
    {
        Vector2 start; //= (Vector2)transform.position + startOffset;
        Vector2 dest; //= (Vector2)transform.position + destOffset;

        if (!Application.isPlaying)
        {
            start = (Vector2)transform.position + startOffset;
            dest = (Vector2)transform.position + destOffset;
            
        }
        else
        {
            start = initialPos + startOffset;
            dest = initialPos + destOffset;
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(start, 0.2f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(dest, 0.2f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, dest);
        
    }
}