using System;

public abstract class PlayerBaseState : IState
{

    protected PlayerController Player { get; }
    protected StateMachine StateMachine { get; }

    protected PlayerBaseState(
        PlayerController player,
        StateMachine stateMachine)
    {
        Player = player
            ?? throw new ArgumentNullException(nameof(player));

        StateMachine = stateMachine
            ?? throw new ArgumentNullException(nameof(stateMachine));
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
}