using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneConfig
{
    public string sceneName;
    public bool hasPickupLantern;
    public float lightStartValue;
    public bool canSwitchWorld;
    
    // Boss
    public BossHitBehavior onBossHit;
}
