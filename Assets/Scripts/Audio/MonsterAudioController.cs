using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAudioController : MonoBehaviour
{
    public AudioClip[] monsterAttackSounds;
    public AudioClip monsterMovingSound;
    private AudioSource audioSource;
    private Monster monster;
    void Start()
    {
        monster = GetComponent<Monster>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (monster.isChasing) {
            PlayMusic(monsterMovingSound);
        }
        if (monster.isAttacking)
        {
            PlayAttackSound();
        }
    }

    private void PlayAttackSound()
    {
        if (monsterAttackSounds != null)
        {
            PlayRandomSound(monsterAttackSounds);
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

    private void PlayRandomSound(AudioClip[] soundArray)
    {
        if (soundArray.Length > 0)
        {
            int randomIndex = Random.Range(0, soundArray.Length);
            audioSource.PlayOneShot(soundArray[randomIndex]);
        }
    }
}
