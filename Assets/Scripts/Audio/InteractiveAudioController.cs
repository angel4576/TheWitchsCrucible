using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveAudioController : MonoBehaviour
{
    public AudioClip[] pickupLightSounds;
    private AudioSource audioSource;

    private InteractionArea interactionArea;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        interactionArea = GetComponent<InteractionArea>();
    }

    void Update()
    {
        if (interactionArea != null && interactionArea.isInteracting)
        {
            PlayPickupLightSound();
        }
    }

    private void PlayPickupLightSound()
    {
        PlayRandomSound(pickupLightSounds);
    }

    private void PlayRandomSound(AudioClip[] soundArray)
    {
        if (soundArray.Length > 0)
        {
            int randomIndex = Random.Range(0, soundArray.Length);
            audioSource.PlayOneShot(soundArray[randomIndex]);
        }
    }
}
