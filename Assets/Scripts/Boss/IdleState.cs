using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BossState
{
    public override void OnEnter(Boss boss)
    {
        Debug.Log("Enter Idle State");
        
        currentBoss = boss;

        currentBoss.moveSpeed = 0;
    }

    public override void LogicUpdate()
    {
        
        
        if (currentBoss.DetectPlayer())
        {
            ChooseAttack();
            // currentBoss.lastAttackTime = Time.time;
        }
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit()
    {
        
    }

    private void ChooseAttack()
    {
        /*
            Randomly goes into:
            1. arm attack
            2. smash attack
            3. idle
        */
        float randomValue = Random.Range(0f, 1f);
        if (randomValue < 0.4f)
        {
            currentBoss.SwitchState(BossStateType.SmashAttack);
        }
        else if (randomValue < 0.8f)
        {
            currentBoss.SwitchState(BossStateType.ArmSwingAttack);
        }
        else // 20 %
        {
            // Idle
        }
    }
}
