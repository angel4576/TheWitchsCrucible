using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;  // For 2D lights

public class Lantern : MonoBehaviour
{
    // 2D Light component attached to the object
    private Light2D lanternLight;

    // Boolean to track if the lantern is on or off
    private bool isLanternOn = false;

    // Rate at which light decreases per second
    public float lightDecayRate = 0.1f;

    // Expose parameters in the Inspector
    [Header("Light Settings")]
    public float maxLightRadius = 10f;   // Maximum light radius when the lantern is fully charged
    public float minLightRadius = 1f;    // Minimum light radius when the lantern is almost out of light
    public float lightIntensity = 1f;    // Light intensity
    public float minLightIntensity = 0.2f; // Minimum light intensity

    public float lightMaxLimit = 3f;     // The maximum limit for the light value

    // Input Action for toggling the lantern
    private PlayerInputControl inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputControl();
        
        // Register input action to toggle lantern when the left mouse button is pressed
        inputActions.Gameplay.ToggleLantern.performed += ctx => ToggleLantern();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        // Get the Light2D component
        lanternLight = GetComponent<Light2D>();

        // Initialize the light state (start with lantern off)
        lanternLight.enabled = false;
        lanternLight.intensity = lightIntensity;
    }

    private void Update()
    {
        // If the lantern is on, reduce the light based on PlayerData's light value
        if (isLanternOn)
        {
            // Reduce light in PlayerData based on time
            if (DataManager.Instance.playerData.light > 0)
            {
                DataManager.Instance.playerData.light -= lightDecayRate * Time.deltaTime;

                // Make sure the light value doesn't go below 0
                DataManager.Instance.playerData.light = Mathf.Max(DataManager.Instance.playerData.light, 0);

                // Adjust lantern's light radius and intensity based on the remaining light
                AdjustLight();
            }

            // If light is 0, automatically turn off the lantern
            if (DataManager.Instance.playerData.light <= 0)
            {
                ToggleLanternOff();
            }
        }
    }

    private void AdjustLight()
    {
        float lightValue = DataManager.Instance.playerData.light;

        // If light value is greater than the maximum limit, set to maximum radius and intensity
        if (lightValue >= lightMaxLimit)
        {
            lanternLight.pointLightOuterRadius = maxLightRadius;
            lanternLight.intensity = lightIntensity;
        }
        else
        {
            // If light value is below the maximum limit, interpolate between min and max values
            float lightPercentage = lightValue / lightMaxLimit;
            lanternLight.pointLightOuterRadius = Mathf.Lerp(minLightRadius, maxLightRadius, lightPercentage);
            lanternLight.intensity = Mathf.Lerp(minLightIntensity, lightIntensity, lightPercentage);
        }
    }

    // Toggles the lantern on/off when left mouse button is pressed
    private void ToggleLantern()
    {
        if (isLanternOn)
        {
            ToggleLanternOff();
        }
        else
        {
            ToggleLanternOn();
        }
    }

    private void ToggleLanternOn()
    {
        if (DataManager.Instance.playerData.light > 0)
        {
            isLanternOn = true;
            lanternLight.enabled = true;  // Turn on the 2D light
        }
    }

    private void ToggleLanternOff()
    {
        isLanternOn = false;
        lanternLight.enabled = false;  // Turn off the 2D light
    }
}
