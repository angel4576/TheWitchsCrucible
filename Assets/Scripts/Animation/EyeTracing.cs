using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTracing : MonoBehaviour
{
    public float traceSpeed;
    public float traceRadius;
    public float deadZone;
    // public float moveDelay;
    
    // private CircleCollider2D coll;

    private float moveRange; // range where center of pupil can move 
    
    private Transform eyeBall;
    private Transform decoration;
    private Transform pupil;

    public Transform player;
    
    // Start is called before the first frame update
    void Start()
    {
        // coll = GetComponent<CircleCollider2D>();
        // moveRange = traceRadius - coll.radius;
        
        decoration = transform.GetChild(2);
        pupil = transform.GetChild(3);
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection = player.position - pupil.position;
        float distanceToPlayer = moveDirection.magnitude;
        moveDirection.Normalize();

        if (distanceToPlayer < deadZone)
        {
            return;
        }
        // Distance from eye center to pupil
        float currentDistance = ((Vector2)pupil.position - (Vector2)transform.position).magnitude;
        
        if (currentDistance > traceRadius)
        {
            Vector2 directionToPupil = pupil.position - transform.position;
            directionToPupil.Normalize();
            
            // reset position to edge
            Vector2 newPos = (Vector2)transform.position + directionToPupil * traceRadius;
            decoration.position = new Vector3(newPos.x, newPos.y, decoration.position.z);
            pupil.position = new Vector3(newPos.x, newPos.y, pupil.position.z);
        }

        StartCoroutine(MoveEye(0, moveDirection));
        
    }

    IEnumerator MoveEye(float time, Vector2 moveDirection)
    {
        yield return new WaitForSeconds(time);
        
        // eyeBall.Translate(Time.deltaTime * traceSpeed * moveDirection);
        decoration.Translate(Time.deltaTime * traceSpeed * moveDirection);
        pupil.Translate(Time.deltaTime * traceSpeed * moveDirection);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, traceRadius);
    }
}
