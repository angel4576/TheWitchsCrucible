using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : BossState
{
    public override void OnEnter(Boss boss)
    {
        currentBoss = boss;
    }

    public override void LogicUpdate()
    {
    }

    public override void PhysicsUpdate()
    {
        if (currentBoss.DetectPlayer())
        {
            currentBoss.FlipTo(currentBoss.player);
            currentBoss.transform.position = Vector2.MoveTowards(currentBoss.transform.position, 
                new Vector2(currentBoss.player.transform.position.x, currentBoss.player.transform.position.y), 
                currentBoss.moveSpeed * Time.deltaTime);
        }
    }

    public override void OnExit()
    {
        
    }
}
