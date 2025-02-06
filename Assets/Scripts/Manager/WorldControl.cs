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
    
    [Header("Monster")]
    public GameObject[] monsters;
    
    [Header("Switch World Effect Settings")]
    public Transform playerTransform;
    public Material spiritWorldMat;
    public Material realWorldMat;
    public float sphereRadius;
    public float maxRadius;
    public float noiseFrequency;
    public float noiseOffset;
    public float changeSpeed;
    public bool canPlayEffect = false; 
    
    private int playerLayerID;
    private int realWorldLayerID;
    private int spiritWorldLayerID;
    
    public GameObject SpiritWorldObjects;
    public GameObject RealWorldObjects;

    [Header("World Status")]
    public bool isRealWorld;

    public bool canSwitch;

    public PlayerInputControl playerInput;
    private Pet pet;
    public UnityEvent onSwitchWorld;

    public GameObject PauseScreen;
    public bool isToggling = false;

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
        
        // SpiritWorldObjects.SetActive(false);
        SpiritWorldObjects.SetActive(true);
        RealWorldObjects.SetActive(true);
        
        isRealWorld = true;

        canSwitch = true;

        SetUpShaderParams();
        
        playerLayerID = LayerMask.NameToLayer("Player");
        realWorldLayerID = LayerMask.NameToLayer("RealWorld");
        spiritWorldLayerID = LayerMask.NameToLayer("SpiritWorld");
        // In real world, ignore spirit world collision
        Physics2D.IgnoreLayerCollision(playerLayerID, spiritWorldLayerID, true);
        Physics2D.IgnoreLayerCollision(playerLayerID, realWorldLayerID, false);

    }

    void Update()
    {
        UpdateShaderParams();

        PlaySwitchWorld();

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
        /*foreach (GameObject obj in gameObject)
        {
            ToggleActive(obj);
        }*/

        // set world status
        isRealWorld = !isRealWorld;
        pet.SetAnimationState(1 ,"IsReality", isRealWorld);
        onSwitchWorld?.Invoke();
        isToggling = false;

        // toggle monsters
        foreach (GameObject monster in monsters)
        {
            if(!monster.GetComponent<Monster>().isDead)
                monster.SetActive(!monster.activeSelf);
            
        }
        
        UpdateLayerCollision();
        
    }
    
    #region Shader Params
    private void SetUpShaderParams()
    {
        // Set up shader params
        realWorldMat.SetFloat("_SphereRadius", 0);
        realWorldMat.SetFloat("_NoiseFrequency", noiseFrequency);
        realWorldMat.SetFloat("_NoiseOffset", noiseOffset);
        
        spiritWorldMat.SetFloat("_SphereRadius", 0);
        spiritWorldMat.SetFloat("_NoiseFrequency", noiseFrequency);
        spiritWorldMat.SetFloat("_NoiseOffset", noiseOffset);
    }
    
    private void UpdateShaderParams()
    {
        realWorldMat.SetVector("_CenterPosition", playerTransform.position);
        realWorldMat.SetFloat("_SphereRadius", sphereRadius);
        
        spiritWorldMat.SetVector("_CenterPosition", playerTransform.position);
        spiritWorldMat.SetFloat("_SphereRadius", sphereRadius);
    }
    #endregion

    private void UpdateLayerCollision()
    {
        if (isRealWorld)
        {
            Physics2D.IgnoreLayerCollision(7, 8, true);
            Physics2D.IgnoreLayerCollision(7, 9, false);
        }
        else // spirit world
        {
            // Debug.Log("Ignore real world collision");
            Physics2D.IgnoreLayerCollision(7, 8, false);
            Physics2D.IgnoreLayerCollision(7, 9, true);
        }
    }
    
    private void PlaySwitchWorld()
    {   
        if(!canPlayEffect)
            return;
        
        sphereRadius += changeSpeed * Time.deltaTime;
        if (sphereRadius >= maxRadius) // effect ends 
        {
            
            changeSpeed = -changeSpeed;
            sphereRadius = maxRadius;
            canPlayEffect = false;
            canSwitch = true;
        }

        if (sphereRadius <= 0)
        {
            changeSpeed = -changeSpeed;
            sphereRadius = 0; 
            canPlayEffect = false;
            canSwitch = true;
        }
        
        
        /*else
        {
            sphereRadius -= changeSpeed * Time.deltaTime;
            if (sphereRadius <= 0) // effect ends 
            {
                sphereRadius = 0;
                canPlayEffect = false;
            }
        }*/
        
    }
    
    public void SwitchWorld()
    {
        
        if (!PauseScreen.GetComponent<PauseManager>().isPaused 
            && DataManager.Instance.playerData.canSwitchWorld
            && canSwitch)
        {
            float delay = 0.000000001f;
            if (!SpiritWorldObjects.activeSelf && RealWorldObjects.activeSelf)
            {
                // if real world -> spirit world, set delay for animation
                // Debug.Log("Switching to Spirit World");
                delay = 0.2f;
                // delay = 1;
            }

            if(SpiritWorldObjects != null && RealWorldObjects != null && !isToggling)
            {
                isToggling = true;
                GameObject[] temp = new GameObject[2];
                temp[0] = SpiritWorldObjects;
                temp[1] = RealWorldObjects;
                
                canPlayEffect = true;
                canSwitch = false;
                
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
