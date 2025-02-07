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

    private float pursuitMusicTime = 0f;
    private float spiritMusicTime = 0f;
    private float realMusicTime = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        worldControl = FindObjectOfType<WorldControl>();

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
        AudioClip newClip = GetCurrentMusic();

        if (audioSource.clip != newClip)
        {
            SwitchMusic(newClip);
        }
    }

    private AudioClip GetCurrentMusic()
    {
        if (worldControl.isRealWorld)
        {
            return (monster != null && monster.isChasing) ? pursuitMusic : RealMusic;
        }
        else
        {
            return SpiritMusic;
        }
    }

    private void SwitchMusic(AudioClip newClip)
    {
        if (audioSource.clip == pursuitMusic)
        {
            pursuitMusicTime = audioSource.time;
        }
        else if (audioSource.clip == SpiritMusic)
        {
            spiritMusicTime = audioSource.time;
        }
        else if (audioSource.clip == RealMusic)
        {
            realMusicTime = audioSource.time;
        }

        audioSource.clip = newClip;

        if (newClip == pursuitMusic)
        {
            audioSource.time = pursuitMusicTime;
        }
        else if (newClip == SpiritMusic)
        {
            audioSource.time = spiritMusicTime;
        }
        else if (newClip == RealMusic)
        {
            audioSource.time = realMusicTime;
        }

        audioSource.Play();
    }
}
