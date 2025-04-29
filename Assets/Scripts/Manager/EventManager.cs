using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class EventManager
{
    // Cutscene
    public static event Action OnCutsceneStart;
    public static event Action OnCutsceneReachLight;
    public static event Action OnCutsceneEnd;

    public static void BroadcastCutsceneStart()
    {
        OnCutsceneStart?.Invoke();
    }
    
    public static void BroadcastCutsceneReachLight()
    {
        OnCutsceneReachLight?.Invoke();
    }

}
