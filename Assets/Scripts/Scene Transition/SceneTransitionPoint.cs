using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionPoint : MonoBehaviour
{
    public string targetSceneName;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameSceneManager.Instance.LoadGameScene(targetSceneName);
        }
    }
}
