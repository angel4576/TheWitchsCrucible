using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}

    // temporary solution
    // the assignment made in inspector will be lost during reloading scene
    // for now using code to assign them here in start and when reloading scene
    public PlayerController player;
    public Monster monster;
    public WorldControl world;

    public UnityEvent OnLanternFirstPickedUp;

    // hold references to monsters and items
    // registered in their own scripts
    public List<Monster> monsters = new List<Monster>();
    public List<WorldItem> items = new List<WorldItem>();

    public DebugSettings debugSettings;
    public GameObject lanternPrefab;
    
    private void Awake() 
    {
        // to ensure game manager is instantiated before monsters and items try to register
        // a -50 script execution order is set in the script execution order settings
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        RegisterEventLevel1();
        
        // Debug mode
        if (debugSettings != null && debugSettings.enableDebugMode)
        {
            ApplyDebugSettings();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // register events for level 1
    // this is a temporary solution
    public void RegisterEventLevel1(){
        OnLanternFirstPickedUp.RemoveAllListeners();

        player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerController>();
        // monster = GameObject.FindGameObjectsWithTag("Monster")[0].GetComponent<Monster>();
        // monster = GameObject.Find("Monster")?.GetComponent<Monster>();
        world = GameObject.Find("WorldControl").GetComponent<WorldControl>();

        // OnLanternFirstPickedUp.AddListener(player.SetLanternStatus);
        // OnLanternFirstPickedUp.AddListener(monster.AcquireLantern);
        // OnLanternFirstPickedUp.AddListener(world.SetCanSwitch);    
    }

    private void ApplyDebugSettings()
    {
        if (debugSettings.giveLanternAtStart)
        {
            GivePlayerLantern();
        }
        
        if (debugSettings.freezeEnemies)
        {
            FreezeEnemies();
        }
    }

    private void GivePlayerLantern()
    {
        // create a lantern
        if(lanternPrefab != null)
            Instantiate(lanternPrefab, player.transform.position, Quaternion.identity);
    }
    
    private void FreezeEnemies()
    {
        // freeze small enemies (change their script later)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject enemy in enemies)
        {
            Monster smallEnemy = enemy.GetComponent<Monster>();
            smallEnemy.enabled = false;
        }
            
        // disable lv1 big enemy
        monster.enabled = false;
    }
    
}
