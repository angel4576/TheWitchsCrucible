using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightUIController : MonoBehaviour
{
    public static LightUIController Instance { get; private set; }
    public Slider slider;
    public GameObject LanternGO;


    private void Awake()
    {
        /*if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }*/
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
    }

    public void SetMaxLight ()
    {
        slider.maxValue = LanternGO.GetComponent<Lantern>().lightMaxLimit;
        slider.value = DataManager.Instance.playerData.light;
    }

    public void UpdateLight()
    {
        slider.value = DataManager.Instance.playerData.light;
    }
}
