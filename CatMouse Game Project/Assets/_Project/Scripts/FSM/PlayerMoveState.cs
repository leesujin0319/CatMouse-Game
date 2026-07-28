using UnityEngine;

public sealed class PlayerMoveState : PlayerBaseState
{
  
    public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        UpdateMoveAnimation();
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.player.animationData.MoveParameterHash);
    }

    public override void HandleInput()
    {

        if (stateMachine.player.IsRunEffectActive)
        {
            stateMachine.ChangeState(stateMachine.playerRunState);
        }
    }

    public override void Update()
    {
        UpdateMoveAnimation();
    }

    public override void PhysicsUpdate()
    {
        if (stateMachine.player.HasMovementInput)
        {
            stateMachine.player.Move(stateMachine.player.MoveSpeed);
            return;
        }

        stateMachine.player.StopMovement();
    }

    private void UpdateMoveAnimation()
    {
        if (stateMachine.player.HasMovementInput)
        {
            StartAnimation(stateMachine.player.animationData.MoveParameterHash);
            return;
        }

        StopAnimation(stateMachine.player.animationData.MoveParameterHash);
    }
}
