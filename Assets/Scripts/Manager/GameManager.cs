using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using UnityEngine.InputSystem;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("✅ Space key input detected!");
        }
    }

    // register events for level 1
    // this is a temporary solution
    public void RegisterEventLevel1(){
        OnLanternFirstPickedUp.RemoveAllListeners();

        player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerController>();
        // monster = GameObject.FindGameObjectsWithTag("Monster")[0].GetComponent<Monster>();
        monster = GameObject.Find("Monster")?.GetComponent<Monster>();
        world = GameObject.Find("WorldControl").GetComponent<WorldControl>();

        // OnLanternFirstPickedUp.AddListener(player.SetLanternStatus);
        // OnLanternFirstPickedUp.AddListener(monster.AcquireLantern);
        // OnLanternFirstPickedUp.AddListener(world.SetCanSwitch);    
    }

    // monster and item registration
    public void RegisterMonster(Monster monster){
        // check if the monster is already registered
        if(!monsters.Contains(monster)){
            monsters.Add(monster);
        }
    }

    public void RegisterItem(WorldItem item){
        // check if the item is already registered
        if(!items.Contains(item)){
            items.Add(item);
        }
    }

    public void FindAndRegisterMonsters(){
        // find all monsters in the scene and register them
        // GameObject[] monsterObjects = GameObject.FindGameObjectsWithTag("Monster"); // only find active objects
        Monster[] monsterObjects = GameObject.FindObjectsOfType<Monster>(true); // find all monster objects
        foreach(Monster monster in monsterObjects){
            //Monster monster = monsterObject.GetComponent<Monster>();
            RegisterMonster(monster);
        }
        Debug.Log("Monsters registered: " + monsters.Count);
    }


}
