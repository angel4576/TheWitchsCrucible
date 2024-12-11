using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;
    [SerializeField] private float typeSpeed = 10f;
    [SerializeField] private float autoNextDelay = 1.5f; // Delay before auto-playing the next paragraph

    private Queue<string> paragraphs = new Queue<string>();

    private bool conversationOver;
    private string p;
    private bool isTyping;
    private Coroutine typeDialogueCoroutine;

    private MonoBehaviour parent;

    private const float MAX_TYPE_TIME = 0.1f;

    private DialogueText dialogueTextGlobal;

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

    public void StartConversation(DialogueText dialogueText)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        NameText.text = dialogueText.speakerName;

        for (int i = 0; i < dialogueText.paragraphs.Length; i++)
        {
            paragraphs.Enqueue(dialogueText.paragraphs[i]);
            //Debug.Log(dialogueText.paragraphs[i]);
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
}