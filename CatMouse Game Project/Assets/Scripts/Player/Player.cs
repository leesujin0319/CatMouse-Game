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
    public float MoveSpeed => moveSpeed;
    public float RunSpeed => runSpeed;

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
        // 수정: 아이템으로 부여된 Run 효과 시간도 상태머신 갱신 전에 함께 줄여줍니다.
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
        // 수정: 아이템이 이 메서드를 호출하면 지정 시간 동안 RunState 진입 조건이 활성화됩니다.
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
        // 수정: Run 효과 시간이 끝나면 자동으로 MoveState가 다시 기본 상태를 맡도록 남은 시간을 감소시킵니다.
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
}
