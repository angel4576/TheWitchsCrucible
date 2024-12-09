using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPC : MonoBehaviour
{
    [SerializeField] private DialogueText dialogueText1;
    [SerializeField] private DialogueText dialogueText2;
    [SerializeField] private DialogueText dialogueText3;
    public DialogueController dialogueController;

    public bool IsTalking = false;

    // Use the GameObject for controlling the visibility
    public GameObject dialogueImg;

    public void Start()
    {
        dialogueController.SetParent(this);

    }

    public void Update()
    {
        dialogueImg.SetActive(IsTalking);

        //Debug.Log("NPc talking is" + IsTalking);
    }

    public void Talk(DialogueText dialogueText)
    {
        IsTalking = true;
        // Start displaying dialogue, this can handle multiple texts
        dialogueController.StartConversation(dialogueText);


        //Debug.Log("NPc talk is trigged", dialogueText);
    }

    public void SetTalking(bool isTalking)
    {
        IsTalking = isTalking;

    }

    public void HideVisuals()
    {

        Renderer[] npcRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in npcRenderers)
        {
            renderer.enabled = false; // Hide each renderer
        }
        //Debug.Log("Hide visuals is trigged");
    }

    public void ShowVisuals()
    {

        Renderer[] npcRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in npcRenderers)
        {
            renderer.enabled = true; // Show each renderer
        }
    }
}