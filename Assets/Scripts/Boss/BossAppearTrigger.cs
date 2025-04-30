using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAppearTrigger : MonoBehaviour
{
    // public Boss boss;
    public GameObject monsterAnimation;
    public MonsterAppearanceController monsterAppearController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Boss Appear
            monsterAnimation?.SetActive(true);
            monsterAppearController?.TriggerBossAppearance();
        }
    }
}
