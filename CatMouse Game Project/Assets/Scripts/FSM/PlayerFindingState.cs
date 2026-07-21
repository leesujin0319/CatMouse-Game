using UnityEngine;

public class PlayerFindingState : PlayerBaseState
{
    public PlayerFindingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {

    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.player.animationData.FindingParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.player.animationData.FindingParameterHash);
    }

    public override void Update()
    {

    }
}
