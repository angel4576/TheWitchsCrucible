using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class NavManager : MonoBehaviour
{
    public static NavManager Instance { get; private set; }
    
    private NavPoint[,] grid;

    [Header("Gizmos Draw")]
    public bool EnableOnDrawGizmos;
    private bool IsInEditor = true;

    [Header("Nev Mesh Setting")]
    public Transform gridOrigin;
    public int gridH;
    public int gridW;
    public float cellSize = 1.0f;

    [Header("Raycast Check")]
    public float platformChecklength;
    public float TrajectoryCheckLength;
    public LayerMask layer;

    // [Header("Jump Setting")]
    private int jumpHeightDivision = 3;
    private int jumpSpeedDivision = 3;
    private float AiJumpHeight; // max jump height
    private float AiJumpSpeed; // horizontal speed

    [Header("Jump Points")]
    public List<GameObject> points;
    
    [Header("AI Setting")]
    public GameObject Pet;
    private float moveSpeed;
    [HideInInspector]public float gravity;

    [Header("Events")] 
    // public UnityEvent OnPlatformReached;
    
    // AStar
    List<NavPoint> openList;
    List<NavPoint> closeList;
    

    private void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        IsInEditor = false;

        grid = new NavPoint[gridH, gridW];
        GenerateNavMesh();
        
        openList = new List<NavPoint>();
        closeList = new List<NavPoint>();

        Rigidbody2D rb = Pet.GetComponent<Rigidbody2D>();
        gravity = Physics2D.gravity.y * rb.gravityScale;

    }

    private void Start() 
    {
        EnableOnDrawGizmos = false;

        
    }

    public Vector2 GetWorldPosition(int i, int j) // grid position to world position
    {
        Vector2 origin = new Vector2(gridOrigin.position.x, gridOrigin.position.y);
        return new Vector2(origin.x + (float)j * cellSize + cellSize * 0.5f, origin.y + (float)i * cellSize + cellSize * 0.5f);
    }

    // get nearest navPoint to trajectory point
    public NavPoint FindNearestNavPoint(Vector2 worldPos)
    {
        int j = math.max(0, (int)Math.Floor((worldPos.x - gridOrigin.position.x) / cellSize));
        int i = math.max(0, (int)Math.Floor((worldPos.y - gridOrigin.position.y) / cellSize));

        return grid[i, j];
    }

    public JumpTrajectory GetJumpTrajectory(int i, int j, int trajectoryIndex)
    {
        return grid[i, j].trajectories[trajectoryIndex];
    }

    #region Nav Mesh Generation
    public void GenerateNavMesh()
    {
        for(int i = 0; i < gridH; ++i)
        {
            for(int j = 0; j < gridW; ++j)
            {
                if(grid[i, j] != null)
                {
                    grid[i, j].trajectories.Clear(); // 清除之前的跳跃轨迹
                    grid[i, j].links.Clear(); // 清除旧的路径
                }
            }
        }
        
        for(int i = 0; i < gridH; ++i)
        {
            for(int j = 0; j < gridW; ++j)
            {
                NavPoint navPoint = new NavPoint(i, j); // grid position
                
                Vector2 pos = GetWorldPosition(i, j);

                bool isDownHit = Physics2D.Raycast(pos, Vector2.down, platformChecklength, layer);
                bool isUpHit = Physics2D.Raycast(pos, Vector2.up, platformChecklength, layer);

                // is platform if collide with downside & not collide with upside
                if(isDownHit && !isUpHit)
                {
                    // Initialize current pt to be solo or right edge (Left edge to be solo, others are right edge)
                    bool leftCheck = j > 0 && grid[i, j-1].type == NavPointType.None; // left point is not platform
                    bool rightCheck = j < gridW - 1; // point on last col of grid is not right edge pt
                    if (leftCheck && rightCheck)
                    {
                        navPoint.type = NavPointType.Solo;
                    }
                    else if (leftCheck)
                    {
                        navPoint.type = NavPointType.LeftEdge; 
                    }
                    else if (rightCheck)
                    {
                        navPoint.type = NavPointType.RightEdge; 
                    }
                    else
                    {
                        navPoint.type = NavPointType.Platform;
                    }

                    // Update left pt to be left edge or platform
                    if(j > 0 && grid[i, j-1].type != NavPointType.None) // left pt is on platform
                    {
                        if (grid[i, j-1].type == NavPointType.RightEdge)
                        {
                            grid[i, j-1].type = NavPointType.Platform;
                        }
                        else // solo to be left edge
                        {
                            grid[i, j-1].type = NavPointType.LeftEdge;
                        }

                        // add move link
                        grid[i, j-1].AddLink(i, j, -1);
                        navPoint.AddLink(i, j-1, -1);

                    }
                }

                grid[i, j] = navPoint;

            }
        }
    
        AddFallLink();
        // AddJumpLinks();
    }

    public void ReAddJumpLinks()
    {
        // for (int i = 0; i < gridH; i++)
        // {
        //     for (int j = 0; j < gridW; j++)
        //     {
        //         grid[i, j].trajectories.Clear(); // 确保轨迹不重复
        //     }
        // }
        
        foreach (var point in points)
        { 
            JumpStartPoint jumpPoint = point.GetComponent<JumpStartPoint>();
            if (jumpPoint != null)
            {
                // Debug.Log($"Start point speed: {jumpPoint.moveSpeed}");
                // Debug.Log($"end point i, j: {jumpPoint.getEndPoint().i}, j: {jumpPoint.getEndPoint().j}");
                jumpPoint.getStartPoint().ClearJumpLinks();
                jumpPoint.getStartPoint().AddJumpLink(jumpPoint.getEndPoint().i, jumpPoint.getEndPoint().j
                    , jumpPoint.moveSpeed, jumpPoint.jumpSpeed);
            }
        }
    }

    public void RefreshJumpPoints()
    {
        foreach (var point in points)
        { 
            JumpStartPoint jumpPoint = point.GetComponent<JumpStartPoint>();
            if (jumpPoint != null)
            {
                jumpPoint.UpdateJumpPoints();
            }
        }
    }
    
    void AddFallLink()
    {
        for(int i = 0; i < gridH; ++i)
        {
            for(int j = 0; j < gridW; ++j)
            {   
                // Fall from left edge
                if(grid[i, j].type == NavPointType.LeftEdge || grid[i, j].type == NavPointType.Solo)
                {
                    if(j == 0) continue;
                    for(int k = i-1; k >= 0; --k)
                    {
                        if(grid[k, j-1].type != NavPointType.None)
                        {
                            grid[i, j].AddLink(k, j-1, -2);
                            break;
                        }
                    }
                }

                // Fall from right edge
                if(grid[i, j].type == NavPointType.RightEdge || grid[i, j].type == NavPointType.Solo)
                {
                    if(j == gridW-1) continue;
                    for(int k = i-1; k >= 0; --k)
                    {
                        if(grid[k, j+1].type != NavPointType.None)
                        {
                            grid[i, j].AddLink(k, j+1, -2);
                            break;
                        }
                    }
                }
            }
        }
    }

    void AddJumpLinks()
    {
        // Generate possible jumps
        List<List<JumpTrajectory>> jumpTrajectoryLists = new List<List<JumpTrajectory>>(); // list of trajectory lists to validate
        for(int i = 0; i < gridH; ++i)
        {
            for(int j = 0; j < gridW; ++j)
            {
                if(grid[i, j].type != NavPointType.None) // start point
                {
                    // Check target nav point within jump range
                    // int startRow = math.max(i - jumpRange, 0); int endRow = math.min(i + jumpRange, gridH - 1);
                    // int startCol = math.max(j - jumpRange, 0); int endCol = math.min(j + jumpRange, gridW - 1);
                    // for(int row = startRow; row <= endRow; ++row)
                    // {   
                    //     for(int col = startCol; col <= endCol; ++col)
                    //     {
                    //         if(grid[row, col].type != NavPointType.None) // target navPoint
                    //         {

                    //         }
                    //     }
                    // }

                    List<JumpTrajectory> navPtTrajectories = new List<JumpTrajectory>(); // trajectories to validate
                    // generate possible jump trajectories based on height/jump divisions for each nav point
                    for(int hDiv = 1; hDiv <= jumpHeightDivision; ++hDiv)
                    {
                        float jumpHeight = AiJumpHeight * (i / jumpHeightDivision);
                        for(int sDiv = 1; sDiv <= jumpSpeedDivision; ++sDiv)
                        {
                            float jumpSpeed = AiJumpSpeed * (i / jumpSpeedDivision);

                            // jump to left/right
                            Vector2 startPos = GetWorldPosition(i, j);
                            JumpTrajectory rightTraj = new JumpTrajectory(grid[i, j], startPos, jumpHeight, jumpSpeed);
                            JumpTrajectory leftTraj = new JumpTrajectory(grid[i, j], startPos, jumpHeight, -jumpSpeed);
                            
                            rightTraj.GenerateTrajectoryPoints();
                            leftTraj.GenerateTrajectoryPoints();

                            navPtTrajectories.Add(rightTraj);
                            navPtTrajectories.Add(leftTraj);
                        }
                    }

                    jumpTrajectoryLists.Add(navPtTrajectories);
                }
            }

        }

        // add jump links
        ValidateTrajectory(jumpTrajectoryLists);

    }

    void ValidateTrajectory(List<List<JumpTrajectory>> trajectoriesToValidate)
    {
        foreach (List<JumpTrajectory> trajectories in trajectoriesToValidate)
        {
            foreach (JumpTrajectory trajectory in trajectories) // for each trajectory
            {
                NavPoint startPoint = trajectory.startNavPoint;
                NavPoint prevPoint = startPoint;
                
                Vector2 prevPos = GetWorldPosition(prevPoint.i, prevPoint.j);
                foreach (Vector2 trajectoryP in trajectory.TrajectoryPoints) // for each pt in a trajectory
                {
                
                    // find related nav point
                    int col = (int)Math.Floor((trajectoryP.x - gridOrigin.position.x) / cellSize);
                    int row = (int)Math.Floor((trajectoryP.y - gridOrigin.position.y) / cellSize);

                    if(row < 0 || row > gridH - 1 || col < 0 || col > gridW - 1)
                    {
                        continue;
                    }

                    NavPoint targetPoint = grid[row, col]; // FindNearestNavPoint(trajectoryP); 
                    // cNavPoint relatedP = FindNearestNavPoint(trajectoryP); // find navPoint related to the trajectory point pos
                    
                    if(targetPoint.i != startPoint.i || targetPoint.j != startPoint.j) // related navPoint cannot be start navPoint
                    {
                        Vector2 targetPos = GetWorldPosition(targetPoint.i, targetPoint.j); 
                        // Vector2 prevPos = GetWorldPosition(prevPoint.i, prevPoint.j); 
                        // bool isBlock = trajectory.hJumpSpeed > 0 ? Physics2D.Raycast(trajectoryP, Vector2.right, TrajectoryCheckLength) : 
                        //                                         Physics2D.Raycast(trajectoryP, Vector2.right, TrajectoryCheckLength);
                    
                        bool canLand = Physics2D.Raycast(trajectoryP, Vector2.down, platformChecklength); // trajectory point is on platform
                        if(targetPoint.type != NavPointType.None 
                        && startPoint.type == NavPointType.RightEdge
                        // && targetPos.y < prevPos.y
                        && canLand)
                        {
                            // startPoint.trajectory = trajectory;
                            startPoint.AddLink(targetPoint.i, targetPoint.j, 3); // add jump link
                        }
                    }

                    prevPos = trajectoryP;
                    
                }
            }
        }
    }

    #endregion
    
    #region AStar Path Finding
    public List<Link> PathFinding(NavPoint start, NavPoint dest)
    {
        openList.Clear();
        closeList.Clear();

        // put start in open list
        start.f = 0; start.g = 0; start.h = 0;
        start.father = null;
        openList.Add(start);

        while(openList.Count > 0)
        {
            openList.Sort(SortOpenList);
            
            NavPoint u = openList[0]; // get current lowest f node 
            closeList.Add(u); 
            openList.RemoveAt(0); 

            // Debug.Log(u.i);
            if(u == dest) // found destination
            {
                //Debug.Log("Found path");
                List<Link> path = new List<Link>();
                path.Add(new Link(dest.i, dest.j, -1)); 
                
                NavPoint v = dest;
                // backtrack path from dest
                while(v != start)
                {
                    path.Add(v.father);
                    v = grid[v.father.i, v.father.j];
                }

                path.Reverse(); 
                return path;

            }

            FindAdjacentNode(u, dest);
        
        }

        // Debug.Log("No path");
        return null;
    }

    private void FindAdjacentNode(NavPoint u, NavPoint dest)
    {
        // Debug.Log(dest.i);
        // Debug.Log(dest.j);
        for(int i = 0; i < u.links.Count; ++i) // update adjacent pts
        {
            int adjI = u.links[i].i;
            int adjJ = u.links[i].j;
            NavPoint adjP = grid[adjI, adjJ];

            if(closeList.Contains(adjP) || openList.Contains(adjP))
                continue;

            
            adjP.father = new Link(u.i, u.j, u.links[i].type); // father to adjacent
            
            // Update f = g + h
            float hDistToEnd = cellSize * math.abs(dest.j - u.j);
            float vDistToEnd = cellSize * (dest.i - u.i);
            

            // calculate h
            adjP.h = hDistToEnd / moveSpeed; // horizontal time 
            adjP.h += math.sqrt(2.0f * math.abs(vDistToEnd) / -gravity); // simplify by only considering fall
            
            // calculate vertical time
            // Should differentiate fall and jump 
            /*if(vDistToEnd < 0) // dest is under u
            {
                // fall
                adjP.h += math.sqrt(2.0f * math.abs(vDistToEnd) / -Physics2D.gravity.y);
            }
            else if(vDistToEnd > 0)
            {
                adjP.h += 2.0f * (u.trajectories[typeNum].vJumpSpeed / -Physics2D.gravity.y);
            }*/ 

            int typeNum = u.links[i].type;
            // calculate g
            adjP.g = u.g; // use time as cost
            if(typeNum == -1) // move 
            {
                adjP.g += cellSize * math.abs(adjP.j - u.j) / moveSpeed; 
            }
            else if(typeNum == -2) // fall
            {
                adjP.g += math.sqrt(2.0f * cellSize * math.abs(u.i - adjP.i) / -gravity); // sqrt(2h/g)
            }
            else // jump
            {
                // adjP.g += 2.0f * (u.trajectories[typeNum].vJumpSpeed / -Physics2D.gravity.y);
                adjP.g += cellSize * math.abs(adjP.j - u.j) / u.trajectories[typeNum].hJumpSpeed; // jump horizontal time
            }

            adjP.f = adjP.g + adjP.h;
            openList.Add(adjP); // add all adjacent points to open list

        }
    }

    private int SortOpenList(NavPoint x, NavPoint y)
    {
        return x.f.CompareTo(y.f); // ascending order
    }

    #endregion

    // Debug draw
    private void OnDrawGizmos() 
    {
        // Initialize grid when is not in game (for debug)
        if(IsInEditor)
        {
            grid = new NavPoint[gridH, gridW];
            GenerateNavMesh();
        }

        // Draw grid
        if(EnableOnDrawGizmos)
        {
            for(int i = 0; i < gridH; i++)
            {
                for(int j = 0; j < gridW; j++)
                {
                    Vector2 pos = GetWorldPosition(i, j);
                    // Draw points
                    if(grid[i, j].type != NavPointType.None)
                    {
                        if(grid[i, j].type == NavPointType.Platform)
                            Gizmos.color = Color.green;
                        else if(grid[i, j].type == NavPointType.LeftEdge)
                            Gizmos.color = Color.red;
                        else if(grid[i, j].type == NavPointType.RightEdge)
                            Gizmos.color = Color.blue;
                        else if(grid[i, j].type == NavPointType.Solo)
                            Gizmos.color = Color.yellow;

                        Gizmos.DrawSphere(pos, 0.2f);
                    }
                    else // not platform
                    {
                        // Gizmos.color = Color.yellow;
                        // Gizmos.DrawSphere(pos, 0.2f);
                    }

                    // Draw links
                    foreach(Link link in grid[i, j].links)
                    {
                        Vector2 linkTargetPos = GetWorldPosition(link.i, link.j);
                        if(link.type == -1) // move link
                            Gizmos.color = Color.green;
                        else if(link.type == -2) // fall link
                            Gizmos.color = Color.blue;
                        else // jump link
                            Gizmos.color = Color.red; 
                            
                        Gizmos.DrawLine(pos, linkTargetPos);
                    }
                    
                    
                }
            }
        }
        
        

        
    }
    

}
