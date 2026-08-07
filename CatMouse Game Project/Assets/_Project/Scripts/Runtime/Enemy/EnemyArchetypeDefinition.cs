using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [CreateAssetMenu(
        fileName = "EnemyArchetypeDefinition",
        menuName = "CatMouse/Run/Enemy Archetype Definition")]
    public sealed class EnemyArchetypeDefinition : ScriptableObject
    {
        [Header("Presentation")]
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _color = Color.white;
        [SerializeField, Min(0.1f)] private float _visualScale = 1f;

        [Header("Stats")]
        [SerializeField, Min(1)] private int _maximumHealth = 1;
        [SerializeField, Min(1)] private int _contactDamage = 5;
        [SerializeField, Min(0.1f)] private float _speedMultiplier = 1f;

        [Header("Attack")]
        [SerializeField] private EnemyAttackType _attackType;
        [SerializeField, Min(0.1f)] private float _rangedAttackRange = 5f;
        [SerializeField, Min(0.1f)] private float _rangedAttackCooldown = 2f;
        [SerializeField, Min(0.1f)] private float _rangedProjectileSpeed = 5f;
        [SerializeField, Min(1)] private int _rangedProjectileDamage = 6;

        [Header("Equipment")]
        [SerializeField] private EnemyEquipmentDefinition _equipment;

        public Sprite Sprite => _sprite;
        public Color Color => _color;
        public float VisualScale => _visualScale;
        public int MaximumHealth => _maximumHealth;
        public int ContactDamage => _contactDamage;
        public float SpeedMultiplier => _speedMultiplier;
        public EnemyAttackType AttackType => _attackType;
        public float RangedAttackRange => _rangedAttackRange;
        public float RangedAttackCooldown => _rangedAttackCooldown;
        public float RangedProjectileSpeed => _rangedProjectileSpeed;
        public int RangedProjectileDamage => _rangedProjectileDamage;
        public EnemyEquipmentDefinition Equipment => _equipment;

        private void OnValidate()
        {
            _visualScale = Mathf.Max(0.1f, _visualScale);
            _maximumHealth = Mathf.Max(1, _maximumHealth);
            _contactDamage = Mathf.Max(1, _contactDamage);
            _speedMultiplier = Mathf.Max(0.1f, _speedMultiplier);
            _rangedAttackRange = Mathf.Max(0.1f, _rangedAttackRange);
            _rangedAttackCooldown = Mathf.Max(0.1f, _rangedAttackCooldown);
            _rangedProjectileSpeed = Mathf.Max(0.1f, _rangedProjectileSpeed);
            _rangedProjectileDamage = Mathf.Max(1, _rangedProjectileDamage);
        }
    }

    public enum EnemyAttackType
    {
        Contact = 0,
        Ranged = 1,
    }
}
