using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        Debug.Log("Checkpoint reached");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player reached checkpoint");
            //DataManager.Instance.playerData.checkPoint = transform.position;
            DataManager.Instance.WriteCheckpointData(transform.position);
        }
    }
}
