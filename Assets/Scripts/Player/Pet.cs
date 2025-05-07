using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Threading.Tasks;

public class Pet : MonoBehaviour, IInteractable
{
    public Transform targetTrans;
    public Transform player;
    private PhysicsCheck physicsCheck;
    
    [Header("Pet Movement")]
    public float speed;
    [HideInInspector]public float jumpForce;
    private int faceDir;
    [HideInInspector]private float xMoveDir;
    
    [Header("Pet Respawn")]
    public float loseDistance;
    public float respawnTime;
    
    // [Header("Obstacle Check")]
    // public LayerMask groundLayer;
    // public float rayLength;
    // public Vector2 rayOffset;

    // [Header("Pet Status")]
    private float idleYDistance; // how far away from player to set to be idle
    private bool isIdle;

    // Path Finding
    [Header("Path Finding")] 
    public float distanceToPoint; // distance to path point to decide if arrive the point
    public float distanceToPlayer;
    public float pathFindTimer;
    public float pathFindTimeInterval; // time between path updates

    private List<Link> path;
    private int curPointIndex;
    public bool isJumping;
    public bool canJump;

    [HideInInspector]public bool canMove;
    private bool isMiss;
    

    //private PhysicsCheck physicsCheck;
    private Rigidbody2D rb;
    private Animator ani;
    private CapsuleCollider2D coll;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        coll = GetComponent<CapsuleCollider2D>();
        // Get script reference
        physicsCheck = GetComponent<PhysicsCheck>();

        physicsCheck = GetComponent<PhysicsCheck>(); 

        canMove = true;
        canJump = true;
        
    }

    private void OnEnable() 
    {

    }

    private void OnDisable() 
    {

    }

    // Update is called once per frame
    void Update()
    {
        // CheckDistance(); // Check distance between pet and player
        SetAnimationState();
        // if(isMiss)
        // {
        //     ResetPosition();
        // }
        
        pathFindTimer -= Time.deltaTime;

        if(WorldControl.Instance.isRealWorld)
            UpdatePath();
    }

    private void FixedUpdate() 
    {   
        FollowPath();
        FlipDirection();
        // if(canMove && !isIdle)
        // {
        //     MoveToPlayer(); 
        //     FlipDirection();
        // }
        

        // if(canJump && !isIdle)
        // {
        //     Jump();
        // }
        
        ani.SetBool("IsLanded", physicsCheck.isOnGround);
    }

    private void SetAnimationState()
    {
        // set animation state
        ani.SetFloat("X_velocity", math.abs(rb.velocity.x));
        ani.SetFloat("Y_velocity", rb.velocity.y);
    }

    public void SetAnimationState<T>(int flag, string param, T value)
    {
        // Debug.Log($"SetAnimationState {flag} {param} {value}");
        ani.SetTrigger("HugTrigger");
        switch (flag)
        {
            case 0:
            {
                ani.SetFloat(param, Convert.ToSingle(value));
                break;
            }
            case 1:
            {
                ani.SetBool(param, Convert.ToBoolean(value));
                break;
            }
            case 2:
            {
                ani.SetTrigger(param);
                break;
            }
            default:
                break;
        }
    }

    private void MoveToPlayer()
    {
        Vector2 moveDir = (targetTrans.position - transform.position).normalized;
        xMoveDir = moveDir.x;
        rb.velocity = new Vector2(moveDir.x * speed, rb.velocity.y);

        // Stop when reach chase point or within a certain distance with player
        if(Vector2.Distance(transform.position, targetTrans.position) < 0.01f || 
        Vector2.Distance(transform.position, player.position ) < 2f )
        {
            rb.velocity = Vector2.zero;

            canMove = false;
        }
    }

    private void Jump(float hSpeed, float vSpeed) 
    {
        // canJump = false;
        StartCoroutine(DelayJump(0.267f, hSpeed, vSpeed));
    }

    IEnumerator DelayJump(float delay, float hSpeed, float vSpeed)
    {
        // rb.velocity = Vector2.zero;
        // isJumping = true;
        ani.SetTrigger("JumpTrigger");
        yield return new WaitForSeconds(0); // no delay for now
        // rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
        rb.velocity = new Vector2(hSpeed, vSpeed); // set preset jump parameters
    }

    private void CheckDistance()
    {
        // Check distance between Pet and Pet chase target
        if(Vector2.Distance(transform.position, targetTrans.position) > loseDistance)
        {  
            isMiss = true;
        }
        else
        {
            isMiss = false;
        }

        Vector2 yPetPos = new Vector2(0, transform.position.y);
        Vector2 yTargetPos =  new Vector2(0, targetTrans.position.y);
        if(Vector2.Distance(yPetPos, yTargetPos) > idleYDistance)
        {  
            isIdle = true;
        }
        else
        {
            isIdle = false;
        }
        
    }

    private void ResetPosition()
    {
        transform.position = targetTrans.position;
        rb.velocity = Vector2.zero;
    }

    public void Interact()
    {
        // Debug.Log("Interact with Pet");
        
    }

    private void FlipDirection()
    {
        faceDir = Math.Sign(transform.localScale.x);

        if (rb.velocity.x < 0)
        {
            faceDir = -1;
        }
        else if (rb.velocity.x > 0)
        {
            faceDir = 1;
        }

        transform.localScale = new Vector3(faceDir * Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    /*void CheckForwardObstacle()
    {
        Vector2 rayDirection = new Vector2(faceDir, 0);
        Vector2 offset = new Vector2(rayOffset.x * faceDir, rayOffset.y);
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + offset, rayDirection, rayLength, groundLayer);

        if (hit)
        {
            ani.SetBool("IsRunning", false);
        }

        Color rayColor = hit ? Color.red : Color.green;

        Debug.DrawRay((Vector2)transform.position + offset, rayDirection * rayLength, rayColor);
    }*/

    #region Path Finding

    void MoveTo(Vector2 nextPosition)
    {
        ani.SetBool("IsRunning", true);
        if(transform.position.x < nextPosition.x) // move right
        {
            // Debug.Log("move right");
            rb.velocity = new Vector2(speed, rb.velocity.y);
        }
        else if(transform.position.x > nextPosition.x)
        {
            // Debug.Log("move left");
            rb.velocity = new Vector2(-speed, rb.velocity.y);
        }
    }

    void FollowPath()
    {
        if(path == null)
            return;

        if(curPointIndex >= path.Count - 1) // arrive
        {
            // Debug.Log("Arrived Path Destination!!");
            ani.SetBool("IsRunning", false);
            rb.velocity = Vector2.zero;
            // rb.velocity = new Vector2(1.0f, 0);
            
            return;
        }

        int curI = path[curPointIndex].i; // current path point ij
        int curJ = path[curPointIndex].j;
        
        int nextI = path[curPointIndex + 1].i;
        int nextJ = path[curPointIndex + 1].j;

        Vector2 nextPos = NavManager.Instance.GetWorldPosition(nextI, nextJ);
        
        // NavPoint curP =  NavManager.Instance.FindNearestNavPoint(transform.position); // pet current grid pos

        // Debug.Log($"Current Pos: {(Vector2)transform.position}");
        // Debug.Log($"Next P Pos: {nextPos}");
        
        int linkType = path[curPointIndex].type; // link from cur to next 
        // Debug.Log(linkType);
        if(linkType == -1) // move
        {
            MoveTo(nextPos);
        }
        else if(linkType == -2) // fall
        {   
            if(physicsCheck.isOnGround)
                MoveTo(nextPos);
        }
        else // jump
        {
            if(!isJumping) 
            {
                JumpTrajectory trajectory = NavManager.Instance.GetJumpTrajectory(curI, curJ, linkType);
                // Debug.Log("Pet Jump!");
                Jump(trajectory.hJumpSpeed, trajectory.vJumpSpeed);
                isJumping = true;
                // set preset jump parameters
                // rb.velocity = new Vector2(trajectory.hJumpSpeed, trajectory.vJumpSpeed);
                // isJumping = true;
            }
            else if(physicsCheck.isOnGround) // adjust pet pos when landed
            {
                if(rb.velocity.x > 0 && transform.position.x > nextPos.x)
                    rb.velocity = new Vector2(-speed, rb.velocity.y);
                else if(rb.velocity.x < 0 && transform.position.x < nextPos.x)
                    rb.velocity = new Vector2(speed, rb.velocity.y);
                    
            }
            
            
        }

        NavPoint curP =  NavManager.Instance.FindNearestNavPoint(transform.position); // pet current grid pos
        // update waypoint index
        /*if(curP.i == nextI && curP.j == nextJ) // reach next point
        {
            // Debug.Log($"Reach Next Point: {curP.i}, {curP.j}");
            // Debug.Log($"Point World Pos: {(Vector2)transform.position}");
            curPointIndex++;
            isJumping = false;
        }*/
        
        float distToNextP = Vector2.Distance(transform.position, nextPos);
        if (distToNextP < distanceToPoint) // reach next point
        {
            curPointIndex++;
            isJumping = false;
        }
        

    }

    void UpdatePath()
    {
        if(NavManager.Instance == null)
        {
            Debug.Log("No NavManager in current scene");
            return;
        }


        float xDistToPlayer = Vector2.Distance(new Vector2(player.position.x, 0), new Vector2(transform.position.x, 0));
        // start path finding when pet is away from player
        if(xDistToPlayer < distanceToPlayer)
            return;

        // adjust time interval based on distance
        // pathFindTimeInterval = math.max(0, xDistToPlayer / math.abs(speed) - 0.4f);
        // pathFindTimeInterval = math.min(pathFindTimeInterval, 1.5f);
        if(pathFindTimer <= 0.001f)
        {
            pathFindTimer = pathFindTimeInterval;
        }
        else
        {
            return;
        }

        NavPoint startP = NavManager.Instance.FindNearestNavPoint(transform.position); // pet pos
        NavPoint endP = NavManager.Instance.FindNearestNavPoint(player.position);
        
        if(path != null && curPointIndex < path.Count-1 // safe check
        // && (startP.i != path[curPointIndex].i || startP.j != path[curPointIndex].j) // find path only if pet reaches a path point
        && !physicsCheck.isOnGround)
            return;

        // if(path != null && curPointIndex < path.Count-2) 
        //     return;    

        if(!player.GetComponent<PhysicsCheck>().isOnGround)
            return;

        curPointIndex = 0;
        path = NavManager.Instance.PathFinding(startP, endP);
        
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.white;
        if(path != null)
        {
            // Debug.Log("Draw path");
            for(int i = 0; i < path.Count; ++i)
            {
                Vector2 pos = NavManager.Instance.GetWorldPosition(path[i].i, path[i].j); 
                Gizmos.DrawSphere(pos, 0.2f);
            }
        }
        
    }

    #endregion

    // bind to OnSwitchWorld in World Control (inspector)
    public async void OnPetSwitchWorld()
    {
        // Debug.Log("On pet switch");
        // Play animation
        Func<Task> delayDeactivate = async () =>
        {
            await Task.Delay(200);
            gameObject.SetActive(WorldControl.Instance.isRealWorld);
        }; 

        if (!WorldControl.Instance.isRealWorld) 
            await delayDeactivate();
        else 
            gameObject.SetActive(WorldControl.Instance.isRealWorld);
        ResetPosition();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("HugTrigger"))
        {
            EventManager.BroadcastHugCutscenePetReachPlayer();
        }
    }
    
}
