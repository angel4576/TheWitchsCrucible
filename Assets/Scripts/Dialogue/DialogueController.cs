using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Events for a dialogue line
[System.Serializable]
public class DialogueLineEventBinding
{
    public string key;
    public UnityEvent onLineFinished;
}

// Events for a dialogue
[System.Serializable]
public class DialogueEventBinding
{
    public string key;
    public UnityEvent onDialogueFinished;
}

public class DialogueController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;
    [SerializeField] private float typeSpeed = 10f;
    [SerializeField] private float autoNextDelay = 1.5f; // Delay before autoplaying the next paragraph

    private PlayerInputControl inputActions;
    [Header("Dialogue Settings")] 
    public bool isAutoPlay;
    
    [SerializeField] private List<DialogueLineEventBinding> dialogueLineEvents;
    [SerializeField] private List<DialogueEventBinding> dialogueEvents;
    
    // [SerializeField] private Canvas dialogueCanvas;
    
    [Header("Dialogue Asset")]
    [SerializeField] private Sprite playerBubble;
    [SerializeField] private Sprite petBubble;
    [SerializeField] private Sprite playerIcon;
    [SerializeField] private Sprite petIcon;

    [Header("Dialogue Visualization")]
    [SerializeField] private Image bubbleImage;
    [SerializeField] private Image iconImage;
    
    private Queue<DialogueLine> paragraphs = new Queue<DialogueLine>();

    private bool conversationOver;
    private DialogueLine p;
    private bool isTyping;
    private Coroutine typeDialogueCoroutine;

    private MonoBehaviour parent;

    private const float MAX_TYPE_TIME = 0.1f;

    private DialogueText dialogueTextGlobal;
    
    // private bool isDialoguePlaying;

    private void Start()
    {
        /*inputActions = InputManager.Instance.GetActions();
        inputActions.UI.Confirm.performed += OnDialogueConfirmPressed;*/
    }

    private void OnEnable()
    {
        InputManager.Instance.GetActions().UI.Confirm.performed += OnDialogueConfirmPressed;
    }

    private void OnDisable()
    {
        InputManager.Instance.GetActions().UI.Confirm.performed -= OnDialogueConfirmPressed;
    }

    public void SetInputAction(PlayerInputControl actions)
    {
        inputActions = actions;

        inputActions.UI.Confirm.performed += OnDialogueConfirmPressed;
        // inputActions.UI.Confirm.Enable();
    }

    private void OnDialogueConfirmPressed(InputAction.CallbackContext obj)
    {
        Debug.Log("[Dialogue Controller] UI F");
        /*Debug.Log($"[DialogueController] confirm pressed on instance: {this}", this);
        Debug.Log($"bubbleImage is null? {bubbleImage == null}");*/
        DisplayNextParagraph(dialogueTextGlobal);
    }

    public void SetParent(MonoBehaviour parentObject)
    {
        parent = parentObject;
    }

    public void DisplayNextParagraph(DialogueText dialogueText)
    {
        if (paragraphs.Count == 0)
        {
            if (!conversationOver)
            {
                StartConversation(dialogueText);
                dialogueTextGlobal = dialogueText;

            }
            else if (conversationOver && !isTyping)
            {
                EndConversation();
                return;
            }
        }

        if (!isTyping)
        {
            p = paragraphs.Dequeue();

            typeDialogueCoroutine = StartCoroutine(TypeDialogueText(p));
        }
        else
        {
            FinishParagraphEarly();
        }
        
        // BroadcastLineFinish(p);

        if (paragraphs.Count == 0)
        {
            conversationOver = true;
        }
    }

    private void SetDialogueBubbleStyle(DialogueLine line)
    {
        if (line.speakerName == "Pet")
        {
            bubbleImage.color = Color.grey;
            iconImage.sprite = petIcon;
        }
        else if (line.speakerName == "Player")
        {
            // temp, change bubble sprite later
            bubbleImage.color = Color.white;
            iconImage.sprite = playerIcon;
        }
    }
    
    public void StartConversation(DialogueText dialogueText)
    {
        // Change input action map
        // inputActions.Gameplay.Disable();
        InputManager.Instance.LockGameplayInput();
        InputManager.Instance.GetActions().UI.Enable();
        
        /*if (!dialogueCanvas.enabled)
        {
             dialogueCanvas.enabled = true;   
        }*/
        
        // Show dialogue bubble
        if (!bubbleImage.gameObject.activeSelf)
        {
            bubbleImage.gameObject.SetActive(true);
        }

        // NameText.text = dialogueText.speakerName;

        for (int i = 0; i < dialogueText.lines.Length; i++)
        {
            paragraphs.Enqueue(dialogueText.lines[i]);
        }

        if (parent is NPC npcParent)
        {
            npcParent.SetTalking(true);
        }
        else
        {
            Debug.LogWarning("Parent is not of type NPC.");
        }

        // Start typing the first paragraph right away
        DisplayNextParagraph(dialogueText);
    }

    public void EndConversation()
    {
        // inputActions.Gameplay.Enable();
        InputManager.Instance.UnlockGameplayInput();
        InputManager.Instance.GetActions().UI.Disable();
        
        paragraphs.Clear();
        NPCDialogueText.text = "";
        conversationOver = false;

        /*if (dialogueCanvas.enabled)
        {
            dialogueCanvas.enabled = false;   
        }*/
        
        // Hide dialogue bubble
        if (bubbleImage.gameObject.activeSelf)
        {
            bubbleImage.gameObject.SetActive(false);
        }

        if (parent is NPC npcParent)
        {
            npcParent.SetTalking(false);
        }
        
    }

    private IEnumerator TypeDialogueText(DialogueLine p)
    {
        isTyping = true;
        
        SetDialogueBubbleStyle(p);

        int maxVisibleChars = 0;

        NPCDialogueText.text = p.content;
        NPCDialogueText.maxVisibleCharacters = maxVisibleChars;

        // Typing dialogue text
        foreach (char c in p.content.ToCharArray())
        {
            maxVisibleChars++;
            NPCDialogueText.maxVisibleCharacters = maxVisibleChars;

            yield return new WaitForSeconds(MAX_TYPE_TIME / typeSpeed);
        }

        isTyping = false; // finish typing
        
        // Trigger line event
        BroadcastLineFinish(p);
        
        if (!isTyping)
        {
            // Wait for some time before automatically showing the next paragraph
            if (paragraphs.Count > 0) // Check if more paragraphs are left
            {
                if (isAutoPlay)
                {
                    //Debug.Log("we have paragraphs numbers left:" + paragraphs.Count);
                    yield return new WaitForSeconds(autoNextDelay); // Added delay before next paragraph
                    DisplayNextParagraph(dialogueTextGlobal); // Automatically call next paragraph after typing finishes
                                                              //Debug.Log("DisplayNextParagraph called" + paragraphs.Count);
                }
            }
            else
            {
                // If no paragraphs are left, end the conversation after a brief delay
                yield return new WaitForSeconds(autoNextDelay);
                EndConversation();
            }

        }
        
    }

    private void FinishParagraphEarly()
    {
        StopCoroutine(typeDialogueCoroutine);
        NPCDialogueText.maxVisibleCharacters = p.content.Length;
        isTyping = false;
        
        BroadcastLineFinish(p);
    }

    public bool IsDialoguePlaying()
    {
        return isTyping || paragraphs.Count > 0;
    }

    private void BroadcastLineFinish(DialogueLine line)
    {
        // line.OnLineFinished?.Invoke();
        if (string.IsNullOrEmpty(line.eventKey)) return;

        foreach (var binding in dialogueLineEvents)
        {
            if (binding.key == line.eventKey)
            {
                binding.onLineFinished?.Invoke();
                break;
            }
        }
        
    }

    private void BroadcastDialogueFinish(DialogueText dialogueText)
    {
        if (string.IsNullOrEmpty(dialogueText.eventKey)) return;
        
        foreach (var binding in dialogueEvents)
        {
            if (binding.key == dialogueText.eventKey)
            {
                binding.onDialogueFinished?.Invoke();
                break;
            }
        }
        
    }
    
    
}