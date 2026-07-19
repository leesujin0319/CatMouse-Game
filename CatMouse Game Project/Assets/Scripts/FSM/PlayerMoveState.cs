using UnityEngine;

public sealed class PlayerMoveState : PlayerBaseState
{
  
    public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.player.animationData.MoveParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.player.animationData.MoveParameterHash);
    }

}