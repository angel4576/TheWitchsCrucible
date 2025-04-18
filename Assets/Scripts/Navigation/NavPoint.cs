using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NavPointType
{
    None,
    Platform,
    LeftEdge,  // left navpoint of platform
    RightEdge,
    Solo, // only 1 navpoint platform
}

public class JumpTrajectory
{
    Vector2 startPosition;
    public NavPoint startNavPoint;
    public float jumpMaxHeight;
    public float hJumpSpeed; // horizontal jump speed
    public float vJumpSpeed;
    private float jumpDuration;
    public List<Vector2> TrajectoryPoints;

    public JumpTrajectory(NavPoint startPoint, Vector2 startPos, float jumpHeight, float jumpSpeed)
    {
        jumpMaxHeight = jumpHeight;
        startPosition = startPos;

        hJumpSpeed = jumpSpeed;
        vJumpSpeed = (float)Math.Sqrt(2.0f * 9.8f * jumpMaxHeight);
        jumpDuration = 2 * (vJumpSpeed / 9.8f);

        startNavPoint = startPoint;
        TrajectoryPoints = new List<Vector2>();
    }
    
    public JumpTrajectory(float moveSpeed, float jumpSpeed)
    {
        // startPosition = startPos;
        hJumpSpeed = moveSpeed;
        vJumpSpeed = jumpSpeed;
    }

    private Vector2 GetJumpPosition(float t)
    {
        float x = startPosition.x + hJumpSpeed * t;
        float y = startPosition.y + vJumpSpeed * t - 0.5f * 9.8f * t * t;
        return new Vector2(x, y);
    }
    public void GenerateTrajectoryPoints()
    {
        for(float t = 0; t <= jumpDuration; t+=0.1f)
        {
            TrajectoryPoints.Add(GetJumpPosition(t));
        }
    }

}

// link parameters
public class Link 
{
    public int i, j; 
    public int type; // link type
    // -1: move
    // -2: fall
    // type num of jump link means the ith trajectory

    public Link(int row, int col, int typeNum)
    {
        i = row;
        j = col;
        type = typeNum;
    }
}

public class NavPoint
{
    public int i, j; // row, column
    public NavPointType type;
    public List<JumpTrajectory> trajectories = new List<JumpTrajectory>();
    public List<Link> links = new List<Link>();

    // AStar
    public float f, g, h;
    public Link father; // Todo: father我觉得应该用navPoint来表示

    public NavPoint(int row, int col)
    {
        i = row;
        j = col;

        type = NavPointType.None;
    }

    public void AddLink(int row, int col, int typeNum) // add a link between 2 pts
    {
        links.Add(new Link(row, col, typeNum));
    }

    public void AddJumpLink(int row, int col, float moveSpeed, float jumpSpeed)
    {
        trajectories.Add(new JumpTrajectory(moveSpeed, jumpSpeed));
        links.Add(new Link(row, col, trajectories.Count-1)); // index 
        /*Debug.Log($"JumpLink Added: Start({this.i}, {this.j}) -> End({i}, {j}), MoveSpeed: {moveSpeed}, JumpSpeed: {jumpSpeed}");
        Debug.Log($"Trajectory Count: {trajectories.Count}");
        Debug.Log($"Link Count: {links.Count}");*/

    }

    public void ClearJumpLinks()
    {
        trajectories.Clear();
        // foreach (Link l in links)
        // {
        //     if (l.type != -1 && l.type != -2)
        //         links.Remove(l);
        //
        // }
    }
    

}

