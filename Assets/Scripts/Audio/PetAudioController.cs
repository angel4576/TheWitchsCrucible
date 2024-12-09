using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetAudioController : MonoBehaviour
{
    public AudioClip[] runningSounds;
    public AudioClip[] jumpingSounds;
    private Rigidbody2D rb2d;
    private AudioSource audioSource;
    private bool isMoving = false;
    private bool isJumping = false;
    private float stepTimer = 0f;
    private float stepInterval = 0.3f;

    private Pet pet;
    private PhysicsCheck physicsCheck;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        pet = GetComponent<Pet>();
        physicsCheck = GetComponent<PhysicsCheck>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pet != null && pet.isJumping)
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

    private void PlayRandomSound(AudioClip[] soundArray)
    {
        if (soundArray.Length > 0)
        {
            int randomIndex = Random.Range(0, soundArray.Length);
            audioSource.PlayOneShot(soundArray[randomIndex]);
        }
    }
}
