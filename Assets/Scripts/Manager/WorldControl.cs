using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class WorldControl : MonoBehaviour
{
    public static WorldControl Instance {get; private set;}

    [Header("Switch World Effect Settings")]
    public Transform PlayerTransform;
    public Material PlatformMaterial;
    public float sphereRadius;
    public float maxRadius;
    public float noiseFrequency;
    public float noiseOffset;
    public float changeSpeed;
    private bool canPlayEffect = false; 
    
    public GameObject SpiritWorldObjects;
    public GameObject RealWorldObjects;

    [Header("World Status")]
    public bool isRealWorld;

    public bool canSwitch;

    public PlayerInputControl playerInput;
    private Pet pet;
    public UnityEvent onSwitchWorld;

    public GameObject PauseScreen;
    private bool isToggling = false;

    #region Unity Callbacks
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }

        playerInput = new PlayerInputControl();
        playerInput.Enable();
        
        playerInput.Gameplay.SwitchWorld.started += context => SwitchWorld(); // E key
    }

    void Start()
    {
        SpiritWorldObjects = GameObject.FindGameObjectsWithTag("SpiritWorld")[0];
        RealWorldObjects = GameObject.FindGameObjectsWithTag("RealWorld")[0];
        pet = GameObject.FindGameObjectWithTag("Pet").GetComponent<Pet>();
        
        SpiritWorldObjects.SetActive(false);
        RealWorldObjects.SetActive(true);
        
        isRealWorld = true;
        
        // Set up shader params
        PlatformMaterial.SetFloat("_SphereRadius", 0);
        PlatformMaterial.SetFloat("_NoiseFrequency", noiseFrequency);
        PlatformMaterial.SetFloat("_NoiseOffset", noiseOffset);
    }

    void Update()
    {
        SetShaderParams();

        if (canPlayEffect)
            sphereRadius += changeSpeed * Time.deltaTime;

        if (sphereRadius >= maxRadius) // effect ends 
        {
            sphereRadius = 0;
        }
    }
    
    #endregion
    
    
    void ToggleActive(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    IEnumerator DelayToggleActive(GameObject[] gameObject, float delay)
    {
        if (delay > 0.00001f) // which means time to play hug animation
        {
            pet.SetAnimationState(2 ,"HugTrigger", 1);
        }
        yield return new WaitForSeconds(delay);
        foreach (GameObject obj in gameObject)
        {
            ToggleActive(obj);
        }

        // set world status
        isRealWorld = !isRealWorld;
        pet.SetAnimationState(1 ,"IsReality", isRealWorld);
        onSwitchWorld?.Invoke();
        isToggling = false;
    }

    private void SetShaderParams()
    {
        PlatformMaterial.SetVector("_CenterPosition", PlayerTransform.position);
        PlatformMaterial.SetFloat("_SphereRadius", sphereRadius);
    }
    private void PlaySwitchWorld()
    {   
        canPlayEffect = true;

    }
    
    public void SwitchWorld()
    {
        // if(!canSwitch)
        // {
        //     return;
        // }

        if (!PauseScreen.GetComponent<PauseManager>().isPaused 
            && DataManager.Instance.playerData.canSwitchWorld)
        {

            float delay = 0.000000001f;
            if (!SpiritWorldObjects.activeSelf && RealWorldObjects.activeSelf)
            {
                // if real world -> spirit world, set delay for animation
                // Debug.Log("Switching to Spirit World");
                delay = 0.2f;
                delay = 3;
            }

            if(SpiritWorldObjects != null && RealWorldObjects != null && !isToggling)
            {
                isToggling = true;
                GameObject[] temp = new GameObject[2];
                temp[0] = SpiritWorldObjects;
                temp[1] = RealWorldObjects;
                
                PlaySwitchWorld();
                
                StartCoroutine(DelayToggleActive(temp, delay));
                
            }
            
        }
        
    }

    // used for restart game
    public void SwitchWorldNoInvoke()
    {
        if (SpiritWorldObjects != null)
        {
            ToggleActive(SpiritWorldObjects);
        }

        if (RealWorldObjects != null)
        {
            ToggleActive(RealWorldObjects);
            
        }
        isRealWorld ^= true;
    }

    // Respond to OnLanternFirstPickedUp event in GameManager
    public void SetCanSwitch()
    {
        DataManager.Instance.playerData.canSwitchWorld = true;
    }
}
