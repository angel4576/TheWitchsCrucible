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
    private void Awake() 
    {
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
        
    }

    public void RegisterEventLevel1(){
        OnLanternFirstPickedUp.RemoveAllListeners();

        player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerController>();
        monster = GameObject.FindGameObjectsWithTag("Monster")[0].GetComponent<Monster>();
        world = GameObject.Find("WorldControl").GetComponent<WorldControl>();

        OnLanternFirstPickedUp.AddListener(player.SetLanternStatus);
        OnLanternFirstPickedUp.AddListener(monster.AcquireLantern);
        OnLanternFirstPickedUp.AddListener(world.SetCanSwitch);    
    }
}