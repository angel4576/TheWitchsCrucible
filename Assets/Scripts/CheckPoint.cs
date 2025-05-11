using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public string checkpointID;
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player reached checkpoint");
            // Save check point data
            DataManager.Instance.checkpointData.checkpointID = checkpointID;
            DataManager.Instance.checkpointData.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            DataManager.Instance.checkpointData.playerPosition = transform.position;
            
            GameSceneManager.Instance.SaveCheckpoint();
            
            // Save to file
            DataManager.Instance.SaveCheckPointData();
            
        }
    }
    
    
}
