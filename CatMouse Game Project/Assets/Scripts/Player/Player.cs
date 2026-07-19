using UnityEngine;

public class Player : MonoBehaviour
{
    [field: Header("Animations")]
    [field: SerializeField] public PlayerAnimationData animationData { get; private set; }
    public Animator animator { get; private set; }
    public PlayerStateMachine stateMachine;

    [field: Header("PlayerStats")]
    public float AttackWeight = 1.0f;
    public float DefenseWeight = 1.0f;
    public float HealthWeight = 1.0f;
}
