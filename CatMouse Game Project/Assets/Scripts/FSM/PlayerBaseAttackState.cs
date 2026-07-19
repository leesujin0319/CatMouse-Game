using UnityEngine;

public class PlayerBaseAttackState : PlayerBaseState
{
    public PlayerBaseAttackState(PlayerStateMachine stateMachine) : base(stateMachine)
    {

    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.player.animationData.BaseAttackParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.player.animationData.BaseAttackParameterHash);
    }

    public override void Update()
    {

    }
}
