using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
public sealed class PlayerController : MonoBehaviour
{
    private enum PlayerMoveState
    {
        None,
        MoveUp,
        MoveDown
    }

    [Header("현재 이동 상태")]

    [SerializeField]
    private PlayerMoveState currentState = PlayerMoveState.None;

    private PlayerInput playerInput;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        if (playerInput.IsPressed)
        {
            ChangeState(PlayerMoveState.MoveUp);
        }
        else
        {
            ChangeState(PlayerMoveState.MoveDown);
        }
    }

    private void Update()
    {
        UpdateState();
    }

    private void FixedUpdate()
    {
        playerMovement.Move();
    }

    private void UpdateState()
    {
        if (playerInput.IsPressed)
        {
            ChangeState(PlayerMoveState.MoveUp);
        }
        else
        {
            ChangeState(PlayerMoveState.MoveDown);
        }
    }

    private void ChangeState(PlayerMoveState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        ExitState(currentState);

        currentState = nextState;

        EnterState(currentState);
    }

    private void EnterState(PlayerMoveState state)
    {
        switch (state)
        {
            case PlayerMoveState.MoveUp:
                playerMovement.SetVerticalDirection(1f);
                Debug.Log("MoveUp 상태 진입");
                break;

            case PlayerMoveState.MoveDown:
                playerMovement.SetVerticalDirection(-1f);
                Debug.Log("MoveDown 상태 진입");
                break;

            case PlayerMoveState.None:
                playerMovement.Stop();
                break;
        }
    }

    private void ExitState(PlayerMoveState state)
    {
        switch (state)
        {
            case PlayerMoveState.MoveUp:
            case PlayerMoveState.MoveDown:
                playerMovement.Stop();
                break;
        }
    }
}