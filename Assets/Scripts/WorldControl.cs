using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class WorldControl : MonoBehaviour
{
    public static WorldControl Instance {get; private set;}

    public GameObject SpiritWorldObjects;
    public GameObject RealWorldObjects;

    [Header("World Status")]
    public bool isRealWorld;

    public bool canSwitch;

    public PlayerInputControl playerInput;
    private Pet pet;
    public UnityEvent onSwitchWorld;

    public GameObject PauseScreen;
    private bool isToggling = false;

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
        pet = GameObject.FindGameObjectWithTag("Pet").GetComponent<Pet>();
        
        SpiritWorldObjects.SetActive(false);
        RealWorldObjects.SetActive(true);
        
        isRealWorld = true;
    }

    void ToggleActive(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    IEnumerator DelayToggleActive(GameObject[] gameObject, float delay)
    {
        if (delay > 0.00001f) // which means time to play hug animation
        {
            pet.SetAnimationState(2 ,"HugTrigger", 1);
        }
        yield return new WaitForSeconds(delay);
        foreach (GameObject obj in gameObject)
        {
            ToggleActive(obj);
        }

        // set world status
        isRealWorld = !isRealWorld;
        pet.SetAnimationState(1 ,"IsReality", isRealWorld);
        onSwitchWorld?.Invoke();
        isToggling = false;
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

            float delay = 0.000000001f;
            if (!SpiritWorldObjects.activeSelf && RealWorldObjects.activeSelf)
            {
                // if real world -> spirit world, set delay for animation
                Debug.Log("Switching to Spirit World");
                delay = 0.2f;
            }

            if(SpiritWorldObjects != null && RealWorldObjects != null && !isToggling)
            {
                isToggling = true;
                GameObject[] temp = new GameObject[2];
                temp[0] = SpiritWorldObjects;
                temp[1] = RealWorldObjects;
                StartCoroutine(DelayToggleActive(temp, delay));
            }
        }
 
    }

    // Respond to OnLanternFirstPickedUp event in GameManager
    public void SetCanSwitch()
    {
        DataManager.Instance.playerData.canSwitchWorld = true;
    }
}
