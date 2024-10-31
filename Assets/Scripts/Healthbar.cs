using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public Image background;
    public Image fill;

    [Range(0, 1)]
    public float currentValue = 1;

    public void SetHealth(float currentHealth, float maxHealth)
    {
        currentValue = currentHealth / maxHealth;
        fill.GetComponent<RectTransform>().localScale = new Vector3(currentValue, 1, 1);
    }

    public void OnValidate(){
        SetHealth(currentValue, 1);
    }
}