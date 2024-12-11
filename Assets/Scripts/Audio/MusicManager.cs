using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip pursuitMusic;
    public AudioClip SpiritMusic;
    public AudioClip RealMusic;
    private AudioSource audioSource;

    public Monster monster;
    private WorldControl worldControl;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        worldControl = FindObjectOfType<WorldControl>();
        // monster = FindObjectOfType<Monster>();
        if (monster == null)
        {
            Debug.LogError("Monster script not found in the scene!");
        }

        if (worldControl == null)
        {
            Debug.LogError("WorldControl not found in the scene!");
        }
    }

    void Update()
    {
        if (worldControl.isRealWorld)
        {
            if (monster != null && monster.isChasing)
            {
                PlayMusic(pursuitMusic);
            }
            else
            {
                PlayMusic(RealMusic);
            }
        }
        else
        {
            PlayMusic(SpiritMusic);
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}

