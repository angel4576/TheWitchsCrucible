using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public class WorldControl : MonoBehaviour
{
    public static WorldControl Instance {get; private set;}
    
    [Header("Monster")]
    public GameObject[] monsters;
    public GameObject boss;
    
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
    private int petLayerID;
    private int realWorldLayerID;
    private int spiritWorldLayerID;
    
    [Header("World Objects")]
    public GameObject SpiritWorldObjects;
    public GameObject RealWorldObjects;
    public GameObject spiritWorldPlatformPrefab;
    private List<GameObject> spiritWorldPlatforms = new List<GameObject>(); 
    
    [Header("World Status")]
    public bool isRealWorld;

    public bool canSwitch;

    private PlayerInputControl playerInput;
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

        /*playerInput = new PlayerInputControl();
        playerInput.Enable();*/

        playerInput = InputManager.Instance.GetActions();
        
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
        petLayerID = LayerMask.NameToLayer("Pet");
        realWorldLayerID = LayerMask.NameToLayer("RealWorld");
        spiritWorldLayerID = LayerMask.NameToLayer("SpiritWorld");
        // In real world, ignore spirit world collision
        Physics2D.IgnoreLayerCollision(playerLayerID, spiritWorldLayerID, true);
        Physics2D.IgnoreLayerCollision(playerLayerID, realWorldLayerID, false);
        
        Physics2D.IgnoreLayerCollision(petLayerID, spiritWorldLayerID, true);
        Physics2D.IgnoreLayerCollision(petLayerID, realWorldLayerID, false);
        
        // Add listener
        GameManager.Instance?.OnLanternFirstPickedUp.AddListener(SetCanSwitch);

    }

    private void OnEnable()
    {
        // playerInput.Gameplay.SwitchWorld.started += context => SwitchWorld(); // E key
        playerInput.Gameplay.SwitchWorld.started += SwitchWorld; // E key

    }

    private void OnDisable()
    {
        playerInput.Gameplay.SwitchWorld.started -= SwitchWorld; // E key
    }
    
    private void OnDestroy()
    {
        GameManager.Instance?.OnLanternFirstPickedUp.RemoveListener(SetCanSwitch);
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

    IEnumerator DelayToggleActive(float delay)
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
        if (boss != null) // temp
        {
            if (isRealWorld)
            {
                boss.SetActive(true);
            }
            else
            {
                boss.SetActive(false);
            }
        }
        
        foreach (GameObject monster in monsters)
        {
            if (!monster.GetComponent<Monster>().isDead)
            {
                // monster.SetActive(!monster.activeSelf);
                if (isRealWorld)
                { 
                    monster.SetActive(true);
                    DestroySpiritWorldPlatform();
                }
                else
                {
                    monster.SetActive(false);
                    SpawnSpiritWorldPlatform(monster);
                }
            }
            
        }
        
        UpdateLayerCollision();
        
    }

    private void SpawnSpiritWorldPlatform(GameObject monster)
    {
        if(spiritWorldPlatformPrefab == null)
        {
            Debug.LogError("Spirit world platform prefab is not set");
        }
        else
        {
             GameObject platform = Instantiate(spiritWorldPlatformPrefab, monster.transform.position, spiritWorldPlatformPrefab.transform.rotation);
             spiritWorldPlatforms.Add(platform);
        }
    }

    private void DestroySpiritWorldPlatform()
    {
        // Destory all spirit world platforms
        foreach (var platform in spiritWorldPlatforms)
        {
            if (platform != null)
            {
                Destroy(platform);
            }
        }
        spiritWorldPlatforms.Clear();
        
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
            // Player
            Physics2D.IgnoreLayerCollision(playerLayerID, spiritWorldLayerID, true); // ignore spirit world collision
            Physics2D.IgnoreLayerCollision(playerLayerID, realWorldLayerID, false);
            
            // Pet
            Physics2D.IgnoreLayerCollision(petLayerID, spiritWorldLayerID, true); 
            Physics2D.IgnoreLayerCollision(petLayerID, realWorldLayerID, false);
        }
        else // spirit world
        {
            Physics2D.IgnoreLayerCollision(playerLayerID, spiritWorldLayerID, false); 
            Physics2D.IgnoreLayerCollision(playerLayerID, realWorldLayerID, true); // ignore real world collision
            
            Physics2D.IgnoreLayerCollision(petLayerID, spiritWorldLayerID, false); 
            Physics2D.IgnoreLayerCollision(petLayerID, realWorldLayerID, true); 
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

    public void DoSwitchWorld()
    {
        Debug.Log("[World Control] Switch world");
        canPlayEffect = true;
        canSwitch = false;
            
        StartCoroutine(DelayToggleActive(1e-10f));
    }
    
    public void SwitchWorld(InputAction.CallbackContext context)
    {
        if (!PauseScreen.GetComponent<PauseManager>().isPaused 
            && DataManager.Instance.playerData.canSwitchWorld
            && canSwitch)
        {
            /*float delay = 0.000000001f;
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
                
                
            }*/
            
            // Debug.Log($"phase: {context.phase}");
            DoSwitchWorld();
            
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
