using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BossState
{
    public override void OnEnter(Boss boss)
    {
        currentBoss = boss;

        currentBoss.moveSpeed = 0;
    }

    public override void LogicUpdate()
    {
        if (currentBoss.CheckPlayerInSmashRange())
        {
            Debug.Log("!!!!!Smash Player!!!!!");
            //currentBoss.SwitchState(BossStateType.SmashAttack);
        }
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit()
    {
        
    }
}
