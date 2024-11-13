using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmSwingAttackState : BossState
{
    private float attackTimer;
    // private float coolDownTimer;
    public override void OnEnter(Boss boss)
    {
        Debug.Log("Enter Swing Attack State");
        currentBoss = boss;
        attackTimer = 0;
        
        // Cooldown
        if (Time.time < currentBoss.lastAttackTime + currentBoss.attackCoolDown)
        {
            currentBoss.SwitchState(BossStateType.Idle);
            return;
        }
        
        float chance = Random.Range(0f, 1f);
        // 50 percent chance
        if (chance < .5f)
        {
            currentBoss.animator.SetTrigger("leftArmSwingAttack");
        }
        else
        {
            currentBoss.animator.SetTrigger("rightArmSwingAttack");
        }
        
    }

    public override void LogicUpdate()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= currentBoss.swingAttackDuration) // attack end
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
