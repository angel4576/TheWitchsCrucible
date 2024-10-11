using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpStartPoint : MonoBehaviour
{
    public Transform endPointTransform;
    [Header("Jump Parameter")]
    public float moveSpeed;
    public float jumpSpeed;

    private NavPoint startP;
    private NavPoint endP;

    // Start is called before the first frame update
    void Start()
    {
        startP = NavManager.Instance.FindNearestNavPoint(transform.position);
        endP = NavManager.Instance.FindNearestNavPoint(endPointTransform.position);

        startP.AddJumpLink(endP.i, endP.j, moveSpeed, jumpSpeed);
    }

    private Vector2 GetJumpPosition(float t)
    {
        float x = transform.position.x + moveSpeed * t;
        float y = transform.position.y + jumpSpeed * t + 0.5f * Physics2D.gravity.y * t * t;
        return new Vector2(x, y);
    }

    private void OnDrawGizmos() 
    {
        float jumpDuration = 2 * (jumpSpeed / -Physics2D.gravity.y);
        Gizmos.color = Color.red;
        
        // Draw jump trajectory
        for(float t = 0; t <= jumpDuration; t+=Time.fixedDeltaTime)
        {
            Gizmos.DrawSphere(GetJumpPosition(t), 0.1f);
        }
    }

}
