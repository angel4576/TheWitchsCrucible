using UnityEngine;
using static ItemData;

public enum ItemType
{
    Light,
    Door,
    // Add more types as needed
}

public class WorldItem : MonoBehaviour, IInteractable
{
    [Header("World Settings")]
    public bool isRealWorldItem;   
    public bool isSpiritWorldItem; 

    [Header("Item Type")]
    public ItemType type;
    public bool isObstacle;

    [Header("Light Item Settings")]
    [SerializeField]
    private float lightAmount = 1f;

    private void Awake()
    {
        // register the item
        Debug.Log("Registering item: " + gameObject.name);
        Debug.Log(GameManager.Instance);
        if (GameManager.Instance != null && !GameManager.Instance.items.Contains(this))
        {
            Debug.Log("Registering item: " + gameObject.name + " confirmed.");
            GameManager.Instance.items.Add(this);
        }
    }

    private void OnDestroy()
    {
        // unregister the item
        if (GameManager.Instance != null && GameManager.Instance.items.Contains(this))
        {
            GameManager.Instance.items.Remove(this);
        }
    }

    public void Interact()
    {
        switch (type)
        {
            case ItemType.Light:
                PickUpLight();
                break;
            case ItemType.Door:
                HandleDoorInteraction();
                break;
            default:
                Debug.Log("Interacting with a generic item.");
                break;
        }
    }

    private void PickUpLight()
    {
        Debug.Log("setting light");
        int curScene = DataManager.Instance.worldData.curSceneIndex;
        bool hasPlayerPickUpLantern = DataManager.Instance.playerData.hasPickedUpLantern;

        if(curScene == 0 && !hasPlayerPickUpLantern) // in level 1 & has not picked up light before
        {
            DataManager.Instance.playerData.hasPickedUpLantern = true;
            GameManager.Instance.OnLanternFirstPickedUp?.Invoke();
        }
        DataManager.Instance.playerData.light = DataManager.Instance.playerData.maxLight;
        Debug.Log("Light picked up! Current light count: " + DataManager.Instance.playerData.light);
        UIManager.Instance.BroadcastMessage("SetMaxLight");
        //DataManager.Instance.playerData.light += lightAmount;

        //Destroy(gameObject);
        gameObject.SetActive(false);

        //checkpoint
        DataManager.Instance.WriteCheckpointData(transform.position);
    }

    private void HandleDoorInteraction()
    {
        Debug.Log("Interacting with Door.");
    }

    private void OnEnable()
    {
        GetComponent<Collider2D>().enabled = true;
    }

    private void OnDisable()
    {
        GetComponent<Collider2D>().enabled = false;
    }

    private void OnValidate()
    {
        //lightAmount is irrelevant if its not light
        if (type != ItemType.Light)
        {
            lightAmount = 0f; 
        }
    }
}
