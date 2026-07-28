using CatMouse.Game.Player;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [field: Header("Animations")]
    [field: SerializeField] public PlayerAnimationData animationData { get; private set; }
    public Animator animator { get; private set; }
    public PlayerStateMachine stateMachine;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;
    [SerializeField] private PlayerRunStats runStats;

    [Header("Run Effect")]
    [SerializeField] private float defaultRunDuration = 2f;

    [field: Header("PlayerStats")]
    public float AttackWeight = 1.0f;
    public float DefenseWeight = 1.0f;
    public float HealthWeight = 1.0f;

    public Rigidbody2D Rigidbody2D { get; private set; }
    public float VerticalInput { get; private set; }
    public bool HasMovementInput => Mathf.Abs(VerticalInput) > 0.01f;
    public bool IsRunEffectActive => runEffectRemainingTime > 0f;
    public float MoveSpeed => GetAdjustedVerticalSpeed(moveSpeed);
    public float RunSpeed => GetAdjustedVerticalSpeed(runSpeed);

    private float fixedXPosition;
    private float runEffectRemainingTime;
    private PlayerInputAction playerInputAction;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator ??= GetComponentInChildren<Animator>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        animationData ??= new PlayerAnimationData();
        animationData.Initialize();
        stateMachine = new PlayerStateMachine(this);
        fixedXPosition = Rigidbody2D.position.x;
        playerInputAction = new PlayerInputAction();
    }

    private void Start()
    {
        stateMachine.ChangeState(stateMachine.playerMoveState);
    }

    private void OnEnable()
    {
        playerInputAction?.PlayerMouse.Enable();
    }

    private void OnDisable()
    {
        playerInputAction?.PlayerMouse.Disable();
    }

    private void OnDestroy()
    {
        playerInputAction?.Dispose();
    }

    private void Update()
    {
        ReadInput();
        UpdateRunEffectTimer();
        stateMachine.HandleInput();
        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.PhysicsUpdate();
    }

    public void Move(float speed)
    {
        Vector2 currentPosition = Rigidbody2D.position;
        Vector2 nextPosition =
            currentPosition
            + Vector2.up
            * VerticalInput
            * speed
            * Time.fixedDeltaTime;

        nextPosition.x = fixedXPosition;
        nextPosition.y = Mathf.Clamp(nextPosition.y, minY, maxY);

        Rigidbody2D.MovePosition(nextPosition);
    }

    public void StopMovement()
    {
        Vector2 currentPosition = Rigidbody2D.position;
        currentPosition.x = fixedXPosition;
        currentPosition.y = Mathf.Clamp(currentPosition.y, minY, maxY);
        Rigidbody2D.MovePosition(currentPosition);
    }

    public void ActivateRunEffect(float duration = -1f)
    {
        float appliedDuration = duration > 0f ? duration : defaultRunDuration;
        runEffectRemainingTime = Mathf.Max(runEffectRemainingTime, appliedDuration);
    }

    private void ReadInput()
    {
        bool isMovePressed = playerInputAction != null && playerInputAction.PlayerMouse.Move.IsPressed();
        VerticalInput = isMovePressed ? 1f : -1f;
    }

    private void UpdateRunEffectTimer()
    {
        if (runEffectRemainingTime <= 0f)
        {
            return;
        }

        runEffectRemainingTime -= Time.deltaTime;

        if (runEffectRemainingTime < 0f)
        {
            runEffectRemainingTime = 0f;
        }
    }

    private void OnValidate()
    {
        if (minY > maxY)
        {
            float temporaryValue = minY;
            minY = maxY;
            maxY = temporaryValue;
        }
    }

    private float GetAdjustedVerticalSpeed(float fallbackSpeed)
    {
        if (runStats == null || !runStats.IsInitialized)
        {
            return fallbackSpeed;
        }

        float baseSpeed = Mathf.Max(0.01f, moveSpeed);
        return runStats.Current.VerticalSpeed * (fallbackSpeed / baseSpeed);
    }
}
