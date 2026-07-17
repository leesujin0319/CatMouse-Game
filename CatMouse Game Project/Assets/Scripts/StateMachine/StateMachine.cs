public abstract class StateMachine
{
    protected IState currentState;

    public void ChangeState(IState newState)
    {
        if(ReferenceEquals(currentState, newState)) return;
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void HandleInput()
    {
        currentState?.HandleInput();
    }

    public void Update()
    {
        currentState?.Update();
    }

    public void PhysicsUpdate()
    {
        currentState?.PhysicsUpdate();
    }
}