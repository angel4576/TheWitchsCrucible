using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Level4CutsceneController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Pet pet;
    private MeshRenderer playerRenderer;
    private MeshRenderer petRenderer;

    [Tooltip("Time to wait before start the first conversation")]
    public float openWaitTime;
    
    [SerializeField] private PlayerLightController playerLight;
    
    [Header("Dialogue")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueText dialogueTextToPlay;

    
    // Start is called before the first frame update
    void Start()
    {
        playerRenderer = player.GetComponent<MeshRenderer>();
        petRenderer = pet.GetComponent<MeshRenderer>();

        StartCoroutine(IntroSequence());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator IntroSequence()
    {
        playerRenderer.enabled = false;
        petRenderer.enabled = false;
        player.DisableGameplayInput();
        
        yield return new WaitForSeconds(openWaitTime);
        
        dialogueController.StartConversation(dialogueTextToPlay);

        yield return new WaitForSeconds(1f);
        
        playerLight.TurnOnLight();
        // Set player
        playerRenderer.enabled = true;
        player.EnableGameplayInput();
        
        yield return new WaitForSeconds(5f);
        
        // petRenderer.enabled = true;
        
    }
}
