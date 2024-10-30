using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using Spine.Unity;

public class PlayerController : MonoBehaviour
{
    public PlayerInputControl inputActions;
    private Vector2 inputDirection;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private SpriteRenderer rend;
    private int faceDir;
    public GameObject PauseScreen;

    private PhysicsCheck physicsCheck;

    public Pet pet;

    [Header("Movement")]
    public float speed;

    [Header("Jump")]
    public float jumpForce;

    [Header("Pet Control")]
    public float petMoveDelayTime;
    public float petJumpDelayTime;

    [Header("Lantern")]
    public Lantern lantern;
    private bool hasLantern;

    [Header("Animation")]
    private Animator ani;
    public SkeletonMecanim skeletonMecanim;  // Use SkeletonMecanim instead of SkeletonAnimation
    private Spine.Slot[] cloakSlots;
    private Dictionary<Spine.Slot, Spine.Attachment> originalCloakAttachments = new Dictionary<Spine.Slot, Spine.Attachment>();

    [Header("Attacks")]
    [SerializeField]
    private float meleeDamage;
    [SerializeField]
    private float meleeLightHeal, meleeAttackSpeedPerSec, meleeAttackRange, rangeAttackCost, rangeAttackDamage, rangeAttackSpeedPerSec, rangeAttackRange;
    private bool canAttack = true;


    private void Awake()
    {
        inputActions = new PlayerInputControl();

        // +=: register actions to action binding
        inputActions.Gameplay.Jump.started += Jump; // call jump when the moment corresponding button is pressed
        inputActions.Gameplay.Pause.started += Pause;
        inputActions.Gameplay.Fire.started += MeleeAttack;
        inputActions.Gameplay.RangeAttack.started += RangeAttack;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }


    private void OnDisable()
    {
        inputActions.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        rend = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();
        skeletonMecanim = GetComponent<SkeletonMecanim>();  // Use SkeletonMecanim

        // Get script reference
        physicsCheck = GetComponent<PhysicsCheck>();

        // Get all slots related to cape
        GetAllCloakSlots();
        if (WorldControl.Instance.isRealWorld)
        {
            DisableCloak();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Read Vector2 value in Move action
        inputDirection = inputActions.Gameplay.Move.ReadValue<Vector2>();
        if (!PauseScreen.GetComponent<PauseManager>().isPaused)
        {
            FlipDirection();
        }
    }

    private void FixedUpdate()
    {
        Move();

        if (!physicsCheck.isOnGround)
        {
            rb.velocity += Vector2.down * Physics2D.gravity.y * Time.fixedDeltaTime;
//            Debug.Log("applying velocity" + rb.velocity);
        }
    }

    private void Move()
    {
        if (inputDirection.y != 0)
        {
            inputDirection.x *= math.sqrt(2);
        }
        rb.velocity = new Vector2(inputDirection.x * speed, rb.velocity.y);
        // set animation state
        if (inputDirection.x != 0)
        {
            ani.SetBool("IsRunning", true);
        }
        else
        {
            ani.SetBool("IsRunning", false);
        }

        if (inputDirection.x != 0 && !pet.canMove)
        {
            Invoke(nameof(ControlPetMovement), petMoveDelayTime);
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (physicsCheck.isOnGround && !PauseScreen.GetComponent<PauseManager>().isPaused)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);

            // Pet jump
            Invoke(nameof(ControlPetJump), petJumpDelayTime);
        }
    }

    private void ControlPetMovement()
    {
        if(pet.gameObject.activeSelf)
            pet.canMove = true;
    }

    private void ControlPetJump()
    {
        if(pet.gameObject.activeSelf)
            pet.canJump = true;
    }

    private void FlipDirection()
    {
        faceDir = Math.Sign(transform.localScale.x);

        if (inputDirection.x < 0)
        {
            faceDir = -1;
        }
        else if (inputDirection.x > 0)
        {
            faceDir = 1;
        }

        transform.localScale = new Vector3(faceDir * Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    public void TakeDamage(float damage)
    {
        DataManager.Instance.playerData.currentHealth -= damage;
        if (DataManager.Instance.playerData.currentHealth <= 0)
        {
            Die();
        }
    }

    // Player dies, for testing purposes
    public void Die()
    {
        Debug.Log("Player died");
        SceneManager.Instance.RestartFromCheckPoint();
        //SceneManager.Instance.ReloadScene();
    }

    public void OnPlayerSwitchWorld()
    {
        // Play animation
        if (WorldControl.Instance.isRealWorld)
        {
            // rend.color = Color.white;
            // disable cape's spine slot
            DisableCloak();
        }
        else
        {
            // rend.color = Color.black;
            // enable cape's spine slot
            EnableCloak();
        }
    }

    private void GetAllCloakSlots()
    {
        // 获取 Skeleton
        var skeleton = skeletonMecanim.skeleton;

        cloakSlots = new Spine.Slot[9];
        cloakSlots[0] = skeleton.FindSlot("Cape");
        cloakSlots[1] = skeleton.FindSlot("Cape Left Sleeve");
        cloakSlots[2] = skeleton.FindSlot("Cape Right Sleeve");
        cloakSlots[3] = skeleton.FindSlot("Eyes");
        cloakSlots[4] = skeleton.FindSlot("Hood Front");
        cloakSlots[5] = skeleton.FindSlot("Hood Back1");
        cloakSlots[6] = skeleton.FindSlot("Hood Back2");
        cloakSlots[7] = skeleton.FindSlot("Left Ear2");
        cloakSlots[8] = skeleton.FindSlot("Right Ear2");

        // Cache the original attachments
        foreach (var slot in cloakSlots)
        {
            if (slot != null && slot.Attachment != null)
            {
                originalCloakAttachments[slot] = slot.Attachment;  // Cache the attachment
            }
        }
    }

    private void DisableCloak()
    {
        // 遍历斗篷相关的所有插槽，将其 Attachment 设置为 null 来禁用它们
        foreach (var slot in cloakSlots)
        {
            if (slot != null)
            {
                slot.Attachment = null;  // 将 Attachment 设置为 null 来禁用插槽
            }
        }
    }

    private void EnableCloak()
    {
        // 遍历斗篷相关的所有插槽，恢复其原来的 Attachment
        foreach (var slot in cloakSlots)
        {
            if (slot != null && originalCloakAttachments.ContainsKey(slot))
            {
                slot.Attachment = originalCloakAttachments[slot];  // 恢复缓存的附件
            }
        }
    }

    private void Pause(InputAction.CallbackContext context)
    {
        PauseScreen.GetComponent<PauseManager>().TogglePause();
    }

    private void MeleeAttack(InputAction.CallbackContext context)
    {
        if (canAttack && DataManager.Instance.playerData.hasPickedUpLantern)
        {
            canAttack = false;
            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Monster");
            bool hitEnemy = false;
            foreach (GameObject currEnemy in allEnemies)
            {
                if ((transform.position.y) >= currEnemy.transform.position.y - 4.0 && (transform.position.y) <= currEnemy.transform.position.y + 4.0)
                {
                    if (faceDir == 1)
                    {
                        if ((transform.position.x + meleeAttackRange) >= (currEnemy.transform.position.x - 3.0) && (transform.position.x + meleeAttackRange) <= (currEnemy.transform.position.x + 3.0))
                        {
                            currEnemy.GetComponent<Monster>().TakeDamage(meleeDamage);
                            hitEnemy = true;
                        }
                    }
                    else
                    {
                        if ((transform.position.x - meleeAttackRange) >= (currEnemy.transform.position.x - 3.0) && (transform.position.x - meleeAttackRange) <= (currEnemy.transform.position.x + 3.0))
                        {
                            currEnemy.GetComponent<Monster>().TakeDamage(meleeDamage);
                            hitEnemy = true;
                        }
                    }
                }
            }
            if (hitEnemy)
            {
                DataManager.Instance.playerData.light += meleeLightHeal;
                UIManager.Instance.BroadcastMessage("UpdateLight");
            }
            StartCoroutine(attackCooldown(meleeAttackSpeedPerSec));
        }
    }
    
    private IEnumerator attackCooldown(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);
        canAttack = true;
    }

    private void RangeAttack(InputAction.CallbackContext context)
    {
        if(canAttack && DataManager.Instance.playerData.light >= rangeAttackCost && DataManager.Instance.playerData.hasPickedUpLantern)
        {
            DataManager.Instance.playerData.light -= rangeAttackCost;
            UIManager.Instance.BroadcastMessage("UpdateLight");
            canAttack = false;
            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Monster");
            foreach (GameObject currEnemy in allEnemies)
            {
                if ((transform.position.y) >= currEnemy.transform.position.y - 4.0 && (transform.position.y) <= currEnemy.transform.position.y + 4.0)
                {
                    if (faceDir == 1)
                    {
                        if ((transform.position.x + rangeAttackRange) >= (currEnemy.transform.position.x - 3.0) && (transform.position.x + rangeAttackRange) <= (currEnemy.transform.position.x + 3.0))
                        {
                            currEnemy.GetComponent<Monster>().TakeDamage(rangeAttackDamage);
                        }
                    }
                    else
                    {
                        if ((transform.position.x - rangeAttackRange) >= (currEnemy.transform.position.x - 3.0) && (transform.position.x - rangeAttackRange) <= (currEnemy.transform.position.x + 3.0))
                        {
                            currEnemy.GetComponent<Monster>().TakeDamage(rangeAttackDamage);
                        }
                    }
                }
            }
            StartCoroutine(attackCooldown(rangeAttackSpeedPerSec));
        }
    }

    // Respond to OnLanternFirstPickedUp event in GameManager
    public void SetLanternStatus()
    {
        // hasLantern = true;
        lantern.gameObject.SetActive(true);

        // reset the lantern to off
        // this is not working because the lantern's Start() is queued to be called in next frame
        //lantern.ResetLantern();
    }
}
