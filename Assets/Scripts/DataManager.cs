using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class Vec3{
    public float x;
    public float y;
    public float z;

    public Vec3(float x, float y, float z){
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vec3(Vector3 v){
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
    }

    public static implicit operator Vector3(Vec3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static implicit operator Vec3(Vector3 v)
    {
        return new Vec3(v.x, v.y, v.z);
    }
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    // directory path
    private string playerDataPath;
    private string settingsDataPath;
    private string worldDataPath;
    private string configPath;
    private string checkpointPath;

    [Header("Data")]
    public PlayerData playerData;
    public SettingsData settingsData;
    public WorldData worldData;
    public ConfigData configData;
    public CheckpointData checkpointData;
    public CheckpointData levelStartData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // init path
        playerDataPath = Path.Combine(Application.persistentDataPath, "PlayerData.json");
        settingsDataPath = Path.Combine(Application.persistentDataPath, "SettingsData.json");
        worldDataPath = Path.Combine(Application.persistentDataPath, "WorldData.json");
        configPath = Path.Combine(Application.streamingAssetsPath, "ConfigData.json");

        LoadAllData();
    }

    // Player data
    public void SavePlayerData()
    {
        string json = JsonConvert.SerializeObject(playerData, Formatting.Indented);
        File.WriteAllText(playerDataPath, json);
        Debug.Log("Player data saved.");
    }

    // Setting data
    public void SaveSettingsData()
    {
        string json = JsonConvert.SerializeObject(settingsData, Formatting.Indented);
        File.WriteAllText(settingsDataPath, json);
        Debug.Log("Settings data saved.");
    }

    // World data
    public void SaveWorldData()
    {
        string json = JsonConvert.SerializeObject(worldData, Formatting.Indented);
        File.WriteAllText(worldDataPath, json);
        Debug.Log("World data saved.");
    }

    // Load all data
    public void LoadAllData()
    {
        playerData = LoadData<PlayerData>(playerDataPath) ?? new PlayerData(); // 如果文件不存在，初始化新数据
        settingsData = LoadData<SettingsData>(settingsDataPath) ?? new SettingsData();
        worldData = LoadData<WorldData>(worldDataPath) ?? new WorldData();
    }

    // Load config data
    public void LoadConfigData()
    {
        configData = LoadData<ConfigData>(configPath) ?? new ConfigData();
    }

    // checkpoint
    public void WriteCheckpointData(Vector3 position, bool isLevelStart = false)
    {
        checkpointData.position = position;
        checkpointData.playerData = new PlayerData();
        checkpointData.playerData.level = playerData.level;
        checkpointData.playerData.maxHealth = playerData.maxHealth;
        checkpointData.playerData.currentHealth = playerData.currentHealth;
        checkpointData.playerData.light = playerData.light;
        checkpointData.playerData.hasPickedUpLantern = playerData.hasPickedUpLantern;
        checkpointData.playerData.canSwitchWorld = playerData.canSwitchWorld;

        checkpointData.isInRealWorld = WorldControl.Instance.isRealWorld;
        // when loading the scene, isRealWorld is not ready, so manually assigned true
        if(isLevelStart){
            checkpointData.isInRealWorld = true;
        }
        

        WriteEnemies(checkpointData);
        WriteItems(checkpointData);
    }

    public void WriteLevelStartData()
    {
        levelStartData.position = new Vec3(0, 0, 0);
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        if(player != null)
        {
            levelStartData.position = new Vec3(player.position);
        }
        levelStartData.playerData = new PlayerData();
        levelStartData.playerData.level = playerData.level;
        levelStartData.playerData.maxHealth = playerData.maxHealth;
        levelStartData.playerData.currentHealth = playerData.currentHealth;
        levelStartData.playerData.light = playerData.light;
        levelStartData.playerData.hasPickedUpLantern = playerData.hasPickedUpLantern;
        levelStartData.playerData.canSwitchWorld = playerData.canSwitchWorld;

        // assume in real world when level start
        levelStartData.isInRealWorld = true;

        // enermy
        WriteEnemies(levelStartData);
        WriteItems(levelStartData);
    }

    // WriteEnemies and WriteItems can be optimized by acquiring the reference once and storing it
    public void WriteEnemies(CheckpointData data){
        data.enemies = new List<EnemyData>();
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject monster in monsters)
        {
            EnemyData enemyData = new EnemyData();
            enemyData.position = new Vec3(monster.transform.position);
            enemyData.rotation = new Vec3(monster.transform.eulerAngles);
            enemyData.scale = new Vec3(monster.transform.localScale);
            data.enemies.Add(enemyData);
        }
    }

    public void WriteItems(CheckpointData data){
        data.items = new List<ItemData>();
        GameObject[] items = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (GameObject item in items)
        {
            ItemData itemData = new ItemData();
            itemData.gameObject = item;
            itemData.itemType = ItemData.ItemType.Light; // only lights for now
            itemData.isInteracted = !item.activeSelf;
            data.items.Add(itemData);
        }
    }

    public void ResetDataToLastCheckpoint(Transform player = null){
        if (player == null)
        {
            try{
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
            catch{
                Debug.Log("Player not found");
            }
        }
        if(player != null){ 
            player.position = checkpointData.position;
            // remove any velocity on player
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            // check lantern
            if(!checkpointData.playerData.hasPickedUpLantern){
                player.GetComponent<PlayerController>().lantern.gameObject.SetActive(false);
            }
        }
        playerData = checkpointData.playerData;

        // load enemies
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        for (int i = 0; i < monsters.Length; i++)
        {
            monsters[i].transform.position = checkpointData.enemies[i].position;
            monsters[i].transform.eulerAngles = checkpointData.enemies[i].rotation;
            monsters[i].transform.localScale = checkpointData.enemies[i].scale;

            // check lantern
            if(!checkpointData.playerData.hasPickedUpLantern){
                monsters[i].GetComponent<Monster>().lantern = null;
            }
            else{
                monsters[i].GetComponent<Monster>().lantern = player.GetComponentInChildren<Lantern>();
            }
        }

        // load items
        foreach (ItemData item in checkpointData.items)
        {
            if(item.itemType == ItemData.ItemType.Light && !item.isInteracted){
                item.gameObject.SetActive(true);
            }
            else{
                item.gameObject.SetActive(false);
            }
        }

        // reset world
        if(checkpointData.isInRealWorld != WorldControl.Instance.isRealWorld){
            WorldControl.Instance.SwitchWorldNoInvoke();
        }

        // reset lantern status to off
        player.GetComponent<PlayerController>().lantern.ResetLantern();
        
        // fade in
        StartCoroutine(SceneManager.Instance.FadeInRoutine());
    }

    public void ResetDataToLevelStart(){
        playerData = levelStartData.playerData;
        SceneManager.Instance.ReloadScene();
    }

    // template method for loading data
    private T LoadData<T>(string filePath) where T : class
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json);
        }
        else
        {
            Debug.Log($"No data found at {filePath}, initializing new data.");
            return null;
        }
    }
}

// Player
[System.Serializable]
public class PlayerData
{
    public int level = 1;
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float light = 3f;
    public Vec3 position = new Vec3(0, 0, 0);
    //public Vec3 checkPoint = null;
    public bool hasPickedUpLantern = false;
    public bool canSwitchWorld = false;

}

// World
[System.Serializable]
public class WorldData
{
    public int curSceneIndex = 0;
}

// Setting
[System.Serializable]
public class SettingsData
{
    public float volume = 1.0f;
    public bool isFullScreen = true;
}

// Config
[System.Serializable]
public class ConfigData
{
    public bool isInitialized = false;
    public List<EnemyData> enemies = new List<EnemyData>();
    public List<ItemData> items = new List<ItemData>();
}

// Checkpoint
[System.Serializable]
public class CheckpointData
{
    public Vec3 position;
    public PlayerData playerData;
    public bool isInRealWorld;
    
    public List<EnemyData> enemies;
    public List<ItemData> items;
}

[System.Serializable]
public class EnemyData
{
    public enum EnemyType
    {
        Melee,
        Range,
        Shiled
    };

    public EnemyType enemyType;
    public Vec3 position;
    public Vec3 rotation;
    public Vec3 scale;
}

[System.Serializable]
public class ItemData
{
    public enum ItemType
    {
        Door,
        Light,
    };

    public GameObject gameObject;

    public ItemType itemType;
    public bool isInteracted;
}