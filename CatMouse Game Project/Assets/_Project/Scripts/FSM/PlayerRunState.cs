using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.player.animationData.RunParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.player.animationData.RunParameterHash);
    }

    public override void Update()
    {
        StopAnimation(stateMachine.player.animationData.MoveParameterHash);
    }

    public override void HandleInput()
    {
        if (!stateMachine.player.IsRunEffectActive)
        {
            stateMachine.ChangeState(stateMachine.playerMoveState);
        }
    }

    public override void PhysicsUpdate()
    {
        if (stateMachine.player.HasMovementInput)
        {
            stateMachine.player.Move(stateMachine.player.RunSpeed);
            return;
        }

        stateMachine.player.StopMovement();
    }
}
