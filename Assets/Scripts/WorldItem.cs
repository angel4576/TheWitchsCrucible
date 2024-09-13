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
        DataManager.Instance.playerData.light += lightAmount;
        Debug.Log("Light picked up! Current light count: " + DataManager.Instance.playerData.light);

        Destroy(gameObject);
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
