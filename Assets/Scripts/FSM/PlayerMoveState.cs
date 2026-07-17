using UnityEngine;

public sealed class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(
        PlayerController player,
        StateMachine stateMachine)
        : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        UnityEngine.Debug.Log("MoveState Enter");
    }

    public override void HandleInput()
    {
    }

    public override void Update()
    {
        // if (Player.IsRunBoostActive)
        // {
        //    StateMachine.ChangeState(Player.RunState);
        // }
    }

    public override void PhysicsUpdate()
    {
        // float direction = Player.playerInput.IsMovePressed
        //     ? 1f
        //     : -1f;

        // Player.playerMovement.MoveVertical(direction);
    }

    public override void Exit()
    {
        UnityEngine.Debug.Log("MoveState Exit");
    }
}