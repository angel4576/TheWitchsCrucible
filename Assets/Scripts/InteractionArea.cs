using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionArea : MonoBehaviour
{   
    public PlayerInputControl inputActions;
    public Pet pet;
    private IInteractable interactableItem;
    private bool canInteract;

    private void Awake() 
    {
        // inputActions = GetComponentInParent<PlayerController>().inputActions;
        inputActions = new PlayerInputControl();
        inputActions.Enable();

        inputActions.Gameplay.SwitchWorld.started += InteractWithPet; // E key
        // inputActions.Gameplay.Interact.started += Interact; // F key
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void InteractWithPet(InputAction.CallbackContext context)
    {
        if(pet != null)
        {
            pet.Interact();
        }
    }

    // Item interaction
    private void Interact(InputAction.CallbackContext context)
    {
        if (canInteract && interactableItem != null) 
        {
            interactableItem.Interact();
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        canInteract = true;
        // if(other.CompareTag("Pet"))
        // {
        //     WorldControl.Instance.canSwitch = true;
        //     pet = other.GetComponent<IInteractable>();
        // }
        if (other.CompareTag("Interactable"))
        {
            interactableItem = other.GetComponent<IInteractable>();
            if (interactableItem != null)
            {
                interactableItem.Interact();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        canInteract = false;

        // if(other.CompareTag("Pet"))
        // {
        //     // Leave pet
        //     WorldControl.Instance.canSwitch = false;
        // }
        
    }
}
