using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;

public class SwitchWorldAudioController : MonoBehaviour
{
    public AudioClip[] spiritToRealSounds;
    public AudioClip[] realToSpiritSounds;
    private AudioSource audioSource;
    
    private WorldControl worldControl;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        worldControl = GetComponent<WorldControl>();
    }

    void Update()
    {
        if (worldControl != null && worldControl.isRealWorld && worldControl.isToggling)
        {
            PlayRealToSpiritSound();
        }
        else if (worldControl != null && !worldControl.isRealWorld && worldControl.isToggling)
        {
            PlaySpiritToRealSound();
        }
    }

    private void PlayRealToSpiritSound()
    {
        if (spiritToRealSounds != null) 
        {
            PlayRandomSound(realToSpiritSounds);
        }
    }

    private void PlaySpiritToRealSound()
    {
        if (realToSpiritSounds != null)
        {
            PlayRandomSound(spiritToRealSounds);
        }
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

