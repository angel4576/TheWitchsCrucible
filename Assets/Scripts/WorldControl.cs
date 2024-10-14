using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class WorldControl : MonoBehaviour
{
    public static WorldControl Instance {get; private set;}

    public GameObject SpiritWorldObjects;
    public GameObject RealWorldObjects;

    [Header("World Status")]
    public bool isRealWorld;

    public bool canSwitch;

    public PlayerInputControl playerInput;
    public UnityEvent onSwitchWorld;

    public GameObject PauseScreen;

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
        // if(!canSwitch)
        // {
        //     return;
        // }

        if (!PauseScreen.GetComponent<PauseManager>().isPaused 
            && DataManager.Instance.playerData.canSwitchWorld)
        {
            if (SpiritWorldObjects != null)
            {
                ToggleActive(SpiritWorldObjects);
            }

            if (RealWorldObjects != null)
            {
                ToggleActive(RealWorldObjects);
                isRealWorld ^= true;
            }

            onSwitchWorld?.Invoke();
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
