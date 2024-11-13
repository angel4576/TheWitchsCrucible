using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashAttackState : BossState
{
    public override void OnEnter(Boss boss)
    {
        currentBoss = boss;
        
        currentBoss.animator.SetTrigger("leftSmashAttack");
    }

    public override void LogicUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        
    }

    public override void OnExit()
    {
        
    }
    
}
