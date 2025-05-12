using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class Vec3{
    public float x;
    public float y;
    public float z;

    public Vec3() { }

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
    private string checkpointDataPath;
    private string configPath;

    [Header("Data")]
    public PlayerData playerData;
    public SettingsData settingsData;
    public WorldData worldData;
    public ConfigData configData;
    public CheckpointData checkpointData;

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
        Debug.Log(playerDataPath);
        settingsDataPath = Path.Combine(Application.persistentDataPath, "SettingsData.json");
        worldDataPath = Path.Combine(Application.persistentDataPath, "WorldData.json");
        checkpointDataPath = Path.Combine(Application.persistentDataPath, "CheckpointData.json");
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

    public void SaveCheckPointData()
    {
        string json = JsonConvert.SerializeObject(checkpointData, Formatting.Indented);
        File.WriteAllText(checkpointDataPath, json);
        Debug.Log("[Data Manager] Checkpoint data saved");
    }

    // Load all data
    public void LoadAllData()
    {
        playerData = LoadData<PlayerData>(playerDataPath) ?? new PlayerData(); // 如果文件不存在，初始化新数据
        settingsData = LoadData<SettingsData>(settingsDataPath) ?? new SettingsData();
        worldData = LoadData<WorldData>(worldDataPath) ?? new WorldData();
        // checkpointData = LoadData<CheckpointData>(checkpointDataPath) ?? new CheckpointData();
    }

    public void LoadCheckpointData()
    {
        checkpointData = LoadData<CheckpointData>(checkpointDataPath) ?? new CheckpointData();
    }

    // Load config data
    public void LoadConfigData()
    {
        configData = LoadData<ConfigData>(configPath) ?? new ConfigData();
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
    public float light = 0f;
    public float maxLight = 3f;
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
    public string checkpointID;
    public string sceneName;
    
    // Player
    public Vec3 playerPosition;
    public float currentLight;
    public bool hasPickedUpLantern;
    public bool canSwitchWorld;
    
    public Vec3 bossPosition;
    
    /*public Vec3 position;
    public PlayerData playerData;
    public bool isInRealWorld;

    public List<EnemyData> enemies;
    public List<ItemData> items;*/
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

    public Monster monsterReference;
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