using UnityEngine;

public class CloseMusic : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查触发物体的标签是否为 "Player"
        if (other.CompareTag("Player"))
        {
            // 查找场景中的 MusicManager 物体
            GameObject musicManager = GameObject.Find("MusicManager");

            if (musicManager != null)
            {
                // 禁用 MusicManager 物体
                musicManager.SetActive(false);
                Debug.Log("MusicManager has been disabled.");
            }
            else
            {
                Debug.LogWarning("MusicManager not found in the scene.");
            }
        }
    }
}