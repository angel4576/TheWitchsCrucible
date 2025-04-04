using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController_v2 : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;
    [SerializeField] private float typeSpeed = 10f;
    [SerializeField] private float autoNextDelay = 1.5f; // Delay before autoplaying the next paragraph

    [SerializeField] private Sprite playerBubble;
    [SerializeField] private Sprite petBubble;
    [SerializeField] private Sprite playerIcon;
    [SerializeField] private Sprite petIcon;
    
    private Queue<string> paragraphs = new Queue<string>();
    private Queue<string> dialogLines = new Queue<string>();

    private bool conversationOver;
    private string p;
    private bool isTyping;
    private Coroutine typeDialogueCoroutine;

    private MonoBehaviour parent;

    private const float MAX_TYPE_TIME = 0.1f;

    private DialogueText dialogueTextGlobal;
    
    // private bool isDialoguePlaying;

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

        if (paragraphs.Count == 0)
        {
            conversationOver = true;
        }
    }

    private void SetDialogueBubbleStyle(DialogueText dialogueText)
    {
        if (dialogueText.speakerName == "Pet")
        {
            
        }
        else if (dialogueText.speakerName == "Player")
        {
            
        }
    }
    
    public void StartConversation(DialogueText dialogueText)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // NameText.text = dialogueText.speakerName;
        

        for (int i = 0; i < dialogueText.lines.Length; i++)
        {
            paragraphs.Enqueue(dialogueText.lines[i].content);
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
        paragraphs.Clear();
        NPCDialogueText.text = "";
        conversationOver = false;

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }

        if (parent is NPC npcParent)
        {
            npcParent.SetTalking(false);
        }
    }

    private IEnumerator TypeDialogueText(string p)
    {
        isTyping = true;

        int maxVisibleChars = 0;

        NPCDialogueText.text = p;
        NPCDialogueText.maxVisibleCharacters = maxVisibleChars;

        foreach (char c in p.ToCharArray())
        {
            maxVisibleChars++;
            NPCDialogueText.maxVisibleCharacters = maxVisibleChars;

            yield return new WaitForSeconds(MAX_TYPE_TIME / typeSpeed);
        }

        isTyping = false;
        if (!isTyping)
        {
            // Wait for some time before automatically showing the next paragraph
            if (paragraphs.Count > 0) // Check if more paragraphs are left
            {
                //Debug.Log("we have paragraphs numbers left:" + paragraphs.Count);
                yield return new WaitForSeconds(autoNextDelay); // Added delay before next paragraph
                DisplayNextParagraph(dialogueTextGlobal); // Automatically call next paragraph after typing finishes
                                                          //Debug.Log("DisplayNextParagraph called" + paragraphs.Count);
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
        NPCDialogueText.maxVisibleCharacters = p.Length;
        isTyping = false;
    }

    public bool IsDialoguePlaying()
    {
        return isTyping || paragraphs.Count > 0;
    }
    
    
}