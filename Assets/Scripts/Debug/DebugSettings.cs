using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugSettings", menuName = "Debug/DebugSettings")]
public class DebugSettings : ScriptableObject
{
    [Header("Debug Settings")]
    public bool enableDebugMode;

    [Header("Player")] 
    public bool giveLanternAtStart;
    
    [Header("Enemy")]
    public bool freezeEnemies;
    
    

}
