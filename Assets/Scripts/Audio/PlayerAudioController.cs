using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    public AudioClip[] runningSounds;
    public AudioClip[] jumpingSounds;
    public AudioClip[] deathSounds;
    private Rigidbody2D rb2d;
    private AudioSource audioSource;
    private bool isMoving = false;
    private bool isJumping = false;
    private float stepTimer = 0f;
    private float stepInterval = 0.3f;

    private PlayerController playerController;
    private PhysicsCheck physicsCheck;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();
        physicsCheck = GetComponent<PhysicsCheck>();
    }

    void Update()
    {
        if (playerController != null && playerController.jumpTriggered)
        {
            if (!isJumping)
            {
                isJumping = true;
                PlayJumpSound();
            }
            return;
        }

        isJumping = false;

        
        if (rb2d.velocity.magnitude > 0.1f && physicsCheck.isOnGround)
        {
            if (!isMoving)
            {
                isMoving = true;
                stepTimer = stepInterval;
            }

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstepSound();
                stepTimer = stepInterval;
            }
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
            }
        }

        if (playerController != null && playerController.isDead)
        {
            PlayDeathSound();
        }
    }

    private void PlayJumpSound()
    {
        if (jumpingSounds != null)
        {
            PlayRandomSound(jumpingSounds);
        }
    }

    private void PlayFootstepSound()
    {
        if (runningSounds != null)
        {
            PlayRandomSound(runningSounds);
        }
    }

    private void PlayDeathSound()
    {
        if(deathSounds != null)
        {
            PlayRandomSound(deathSounds);
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
