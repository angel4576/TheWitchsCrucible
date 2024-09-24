using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManagerSaver : MonoBehaviour
{
    public static InputManagerSaver Instance { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        if (Instance != this) Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
