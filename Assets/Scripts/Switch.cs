using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour, IInteractable
{
    public bool isActivatedOnce = false;
    public Transform target;
    public Vector3 targetInitialPosition;
    public Vector3 targetFinalPosition;

    private void Start() 
    {
        targetInitialPosition = target.position;
    }

    public void Interact()
    {
        if(!isActivatedOnce)
        {
            isActivatedOnce = true;
            Debug.Log("Switch activated!");
            // do something
            MoveTarget();
        }
    }

    public void MoveTarget()
    {
        // enable target
        bool initialStatus = target.gameObject.activeSelf;
        target.gameObject.SetActive(true);
        // disable renderer to make it invisible
        target.GetComponent<SpriteRenderer>().enabled = false;
        target.position = targetFinalPosition;
        // enable renderer
        target.GetComponent<SpriteRenderer>().enabled = true;
        // disable target
        target.gameObject.SetActive(initialStatus);
    }

    public void ResetTarget()
    {
        bool initialStatus = target.gameObject.activeSelf;
        target.gameObject.SetActive(true);
        target.GetComponent<SpriteRenderer>().enabled = false;
        target.position = targetInitialPosition;
        target.GetComponent<SpriteRenderer>().enabled = true;
        target.gameObject.SetActive(initialStatus);
    }

    public void OnDrawGizmos(){
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetInitialPosition, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetFinalPosition, 0.5f);
    }

    private void OnValidate() 
    {
        if(target != null)
        {
            targetInitialPosition = target.position;
        }
    }
}
