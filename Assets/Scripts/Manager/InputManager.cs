using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private PlayerInputControl inputActions;
    private int inputLockCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        inputActions = new PlayerInputControl();
        inputActions.Enable();
    }

    public void LockGameplayInput()
    {
        inputLockCount++;
        inputActions.Disable();
        // Debug.Log($"Input Locked. Count = {inputLockCount}");
    }

    public void UnlockGameplayInput()
    {
        inputLockCount = Mathf.Max(0, inputLockCount - 1);
        if (inputLockCount == 0)
        {
            inputActions.Enable();
            // Debug.Log("Input Re-enabled");
        }
    }

    public void ResetGameplayLock()
    {
        inputLockCount = 0;
        inputActions.Gameplay.Enable();
    }

    public PlayerInputControl GetActions()
    {
        return inputActions;
    }
}

