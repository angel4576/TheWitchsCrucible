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
    public bool isSpiritWorld;

    private bool spiritInitActive = false;
    private bool realInitActive = true;

    public bool canSwitch;

    public PlayerInputControl playerInput;


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
        
        playerInput.Gameplay.SwitchWorld.started += context => SwitchWorld();

        playerInput.Enable();
    }


    void Start()
    {
        SetInitialState(SpiritWorldObjects, spiritInitActive);
        SetInitialState(RealWorldObjects, realInitActive);
    }

    public void SetInitialState(GameObject parent, bool initiallyActive)
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(initiallyActive);
        }
    }

    void ToggleChildrenActive(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }

    public void SwitchWorld()
    {
        if(!canSwitch)
        {
            return;
        }

        if (SpiritWorldObjects != null)
        {
            ToggleChildrenActive(SpiritWorldObjects);
        }

        if (RealWorldObjects != null)
        {
            ToggleChildrenActive(RealWorldObjects);
        }
    }

    public void CheckCurrentWorld()
    {
        
    }

}
