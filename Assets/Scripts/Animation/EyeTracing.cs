using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTracing : MonoBehaviour
{
    public float traceSpeed;
    public float traceRadius;
    public float deadZone;
    public float moveDelay;

    // private Transform eye;
    
    private Transform eyeBall;
    private Transform decoration;
    private Transform pupil;

    public Transform player;
    
    // Start is called before the first frame update
    void Start()
    {
        eyeBall = transform.GetChild(1); 
        decoration = transform.GetChild(2);
        pupil = transform.GetChild(3);

        // traceRadius -= 1.5f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection = player.position - eyeBall.position;
        moveDirection.Normalize();
        float distanceToPlayer = moveDirection.magnitude;

        if (distanceToPlayer < deadZone)
        {
            return;
        }
        float currentDistance = ((Vector2)eyeBall.position - (Vector2)transform.position).magnitude;
        // float currentDistance = Vector3.Distance(eyeBall.position, transform.position);
        
        if (currentDistance > traceRadius)
        {
            Vector2 directionToEyeCenter = eyeBall.position - transform.position;
            directionToEyeCenter.Normalize();
            // reset position to edge
            eyeBall.position = (Vector2)transform.position + directionToEyeCenter * traceRadius;
            decoration.position = (Vector2)transform.position + directionToEyeCenter * traceRadius;
            pupil.position = (Vector2)transform.position + directionToEyeCenter * traceRadius;
        }

        StartCoroutine(MoveEye(moveDelay, moveDirection));
        
        
        
    }

    IEnumerator MoveEye(float time, Vector2 moveDirection)
    {
        yield return new WaitForSeconds(time);
        
        eyeBall.Translate(Time.deltaTime * traceSpeed * moveDirection);
        decoration.Translate(Time.deltaTime * traceSpeed * moveDirection);
        pupil.Translate(Time.deltaTime * traceSpeed * moveDirection);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, traceRadius);
    }
}
