using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashAttackState : BossState
{
    private float attackTimer;
    
    public override void OnEnter(Boss boss)
    {
        Debug.Log("Enter Smash Attack State");
        currentBoss = boss;
        attackTimer = 0;
        
        // Cooldown
        if (Time.time < currentBoss.lastAttackTime + currentBoss.attackCoolDown)
        {
            currentBoss.SwitchState(BossStateType.Idle);
            return;
        }
        
        float chance = Random.Range(0f, 1f);
        if (chance < .5f)
        {
            // Debug.Log("Left");
            currentBoss.animator.SetTrigger("leftSmashAttack");
        }
        else
        {
            // Debug.Log("Right");
            currentBoss.animator.SetTrigger("rightSmashAttack");
        }
    }

    public override void LogicUpdate()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= currentBoss.smashAttackDuration)
        {
            currentBoss.lastAttackTime = Time.time;
            currentBoss.SwitchState(BossStateType.Idle);
        }
    }

    public override void PhysicsUpdate()
    {
        
    }

    public override void OnExit()
    {
        
    }
    
}
