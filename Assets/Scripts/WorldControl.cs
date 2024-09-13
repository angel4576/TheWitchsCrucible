using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class WorldControl : MonoBehaviour
{
    public static WorldControl Instance {get; private set;}

    public GameObject SpiritWorldObjects;
    public GameObject RealWorldObjects;

    [Header("World Status")]
    public bool isRealWorld;

    public bool canSwitch;

    private PlayerInputControl playerInput;

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

        playerInput = new PlayerInputControl();
        playerInput.Enable();
        
        playerInput.Gameplay.SwitchWorld.performed += context => SwitchWorld();
    }

    void Start()
    {
        SpiritWorldObjects.SetActive(false);
        RealWorldObjects.SetActive(true);
        isRealWorld = true;
    }

    void ToggleActive(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void SwitchWorld()
    {
        if(!canSwitch)
        {
            return;
        }

        if (SpiritWorldObjects != null)
        {
            ToggleActive(SpiritWorldObjects);
        }

        if (RealWorldObjects != null)
        {
            ToggleActive(RealWorldObjects);
            isRealWorld ^= true;
        }
    }
}
