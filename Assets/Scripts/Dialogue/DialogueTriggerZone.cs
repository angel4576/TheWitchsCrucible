using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueTriggerZone : MonoBehaviour
{
    [SerializeField] private DialogueController dialogueControl;
    [SerializeField] private DialogueText dialogueTextToPlay;
    [SerializeField] private Transform player;
    private float triggerXValue;
    private float triggerYValue;
    private float endTriggerXValue;
    private float endTriggerYValue;
    [SerializeField] private AudioClip dialogueAudioClip; // Assign your audio clip here
    [SerializeField] private bool SpiritOnly;

    private AudioSource audioSource;
    private bool hasTriggeredDialogue = false;
    private bool hasEndedDialogue = false;

    private const float positionTolerance = 3.5f; // Tolerance value for trigger range

    private void Start()
    {
        // Ensure the AudioSource is attached to this GameObject
        audioSource = gameObject.AddComponent<AudioSource>();

        if (dialogueAudioClip != null)
        {
            audioSource.clip = dialogueAudioClip;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (!hasTriggeredDialogue)
        {
            if((SpiritOnly && !(WorldControl.Instance.isRealWorld)) || (!SpiritOnly && WorldControl.Instance.isRealWorld))
            {
                hasTriggeredDialogue = true;
                dialogueControl.StartConversation(dialogueTextToPlay);

                // Play the dialogue audio
                if (dialogueAudioClip != null)
                {
                    audioSource.Play();
                }
            }
        }
        
    }

    void Update()
    {
        // Trigger dialogue if player's position is within trigger range
        if (!hasTriggeredDialogue &&
            Mathf.Abs(player.position.x - triggerXValue) <= positionTolerance &&
            Mathf.Abs(player.position.y - triggerYValue) <= positionTolerance)
        {
            /*if((SpiritOnly && !(WorldControl.Instance.isRealWorld)) || (!SpiritOnly && WorldControl.Instance.isRealWorld))
            {
                hasTriggeredDialogue = true;
                dialogueControl.StartConversation(dialogueTextToPlay);
            
                // Play the dialogue audio
                if (dialogueAudioClip != null)
                {
                    audioSource.Play();
                }
            
                Debug.Log("Player y and x value" + player.position.y + " : " + player.position.x);
                Debug.Log("triggerzone y and x value" + triggerYValue + " : " + triggerXValue);
                Debug.Log("Dialogue is triggered", dialogueTextToPlay);
            }*/
        }

        // End dialogue if player's position is within end trigger range
        /*if ((!hasEndedDialogue && hasTriggeredDialogue &&
            ((Mathf.Abs(player.position.x - endTriggerXValue) <= positionTolerance &&
            Mathf.Abs(player.position.y - endTriggerYValue) <= positionTolerance) ||
            ((SpiritOnly && WorldControl.Instance.isRealWorld) || (!SpiritOnly && !WorldControl.Instance.isRealWorld)))))
        {
            hasEndedDialogue = true;
            dialogueControl.EndConversation();

            // Stop the audio if it's still playing
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            Debug.Log("Trigger stop talking");
            Debug.Log("Player y and x value" + player.position.y + " : " + player.position.x);
            Debug.Log("triggerzone y and x value" + endTriggerYValue + " : " + endTriggerXValue);
            Debug.Log("Dialogue is ended", dialogueTextToPlay);
            gameObject.SetActive(false);
        }*/
    }
}