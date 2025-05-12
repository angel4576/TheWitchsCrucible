using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public string checkpointID;
    [Tooltip("Boss spawn position relative to the checkpoint (top left to checkpoint)")]
    public float relativePositionX = 45;
    public float relativePositionY = 20;
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player reached checkpoint");
            // Save check point data
            DataManager.Instance.checkpointData.checkpointID = checkpointID;
            DataManager.Instance.checkpointData.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            // Save position relative to checkpoint
            DataManager.Instance.checkpointData.playerPosition = transform.position;
            DataManager.Instance.checkpointData.bossPosition = new Vector3(transform.position.x - relativePositionX
                , transform.position.y + relativePositionY, 0);
            
            GameSceneManager.Instance.SaveCheckpoint();
            
            // Save to file
            DataManager.Instance.SaveCheckPointData();
            
        }
    }
    
    
}
