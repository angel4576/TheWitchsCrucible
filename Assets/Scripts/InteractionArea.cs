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
    private IInteractable f_interactableItem;
    public bool canInteract;

    public bool isInteracting;
    private void Awake() 
    {
        // inputActions = GetComponentInParent<PlayerController>().inputActions;
        inputActions = new PlayerInputControl();
        inputActions.Enable();

        inputActions.Gameplay.SwitchWorld.started += InteractWithPet; // E key
        inputActions.Gameplay.Interact.started += F_Interact; // F key
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!canInteract)
        {
            isInteracting = false;
        }
    }
    
    private void InteractWithPet(InputAction.CallbackContext context)
    {
        if(pet != null)
        {
            pet.Interact();
        }
    }

    // Item interaction
    private void F_Interact(InputAction.CallbackContext context)
    {
        if (canInteract && f_interactableItem != null) 
        {
            f_interactableItem.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // if(other.CompareTag("Pet"))
        // {
        //     WorldControl.Instance.canSwitch = true;
        //     pet = other.GetComponent<IInteractable>();
        // }
        if (other.CompareTag("Interactable"))
        {
            canInteract = true;
            interactableItem = other.GetComponent<IInteractable>();
            if (interactableItem != null)
            {
                isInteracting = true;
                interactableItem.Interact();
            }
        }
        // F_Interact
        if (other.CompareTag("F_Interactable"))
        {
            canInteract = true;
            f_interactableItem = other.GetComponent<IInteractable>();
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        // if(other.CompareTag("Pet"))
        // {
        //     // Leave pet
        //     WorldControl.Instance.canSwitch = false;
        // }

        if (other.CompareTag("Interactable"))
        {
            canInteract = false;
        }
        
        if(other.CompareTag("F_Interactable"))
        {
            canInteract = false;
            f_interactableItem = null;
        }
    }
}
