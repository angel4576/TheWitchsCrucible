using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class WorldControl : MonoBehaviour
{
    public GameObject SpiritWorldObjects;

    public GameObject RealWorldObjects;

    private bool spiritInitActive = false;

    private bool realInitActive = true;

    private PlayerInputControl playerInput;

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

    void Start()
    {
        SetInitialState(SpiritWorldObjects, spiritInitActive);
        SetInitialState(RealWorldObjects, realInitActive);
    }

    private void Awake()
    {
        playerInput = new PlayerInputControl();
        
        playerInput.Gameplay.SwitchWorld.performed += context => SwitchWorld();
    }

    public void SwitchWorld()
    {
        if (SpiritWorldObjects != null)
        {
            ToggleChildrenActive(SpiritWorldObjects);
        }

        if (RealWorldObjects != null)
        {
            ToggleChildrenActive(RealWorldObjects);
        }
    }
}
