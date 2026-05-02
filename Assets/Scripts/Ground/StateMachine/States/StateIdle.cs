using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class BossStateIdle : IState
{
    BossAI owner;
    float maxIdleTime = 3f;
    float currentIdleTime = 0f;

    public BossStateIdle(BossAI owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        // reminder: change animation to idle animation
        StartTimer();
        // Animator anim = owner.gameObject.GetComponent<Animator>();
        // if (anim == null)
        // {
        //     anim = owner.gameObject.AddComponent<Animator>();
            
        // }
        // anim.runtimeAnimatorController = owner.vanishAnimator.runtimeAnimatorController;
    }

    public void Exit()
    {
        
    }

    public void Execute()
    {
        currentIdleTime -= Time.deltaTime;
        if (currentIdleTime < 0.0f)
        {
            Debug.Log("idle time over, switching states");

            IState newState = owner.DecideAttack();
            //owner.stateMachine.ChangeState(newState);
            owner.stateMachine.ChangeState(new BossStateIdle(owner));
        }
    }

    public void StartTimer()
    {
        currentIdleTime = maxIdleTime;
    }

}
