using System;

public abstract class PlayerBaseState : IState
{

    protected PlayerStateMachine stateMachine;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {

    }

    public virtual void Exit()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void PhysicsUpdate()
    {
        
    }

    public virtual void HandleInput()
    {

    }

     protected void StartAnimation(int animatorHash)
    {
        stateMachine.player.animator.SetBool(animatorHash, true);
    }

    protected void StopAnimation(int animatorHash)
    {
        stateMachine.player.animator.SetBool(animatorHash, false);
    }
}