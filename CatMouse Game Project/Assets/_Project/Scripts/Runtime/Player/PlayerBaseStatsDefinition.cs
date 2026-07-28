using UnityEngine;

namespace CatMouse.Game.Player
{
    [CreateAssetMenu(
        fileName = "PlayerBaseStatsDefinition",
        menuName = "CatMouse/Run/Player Base Stats Definition")]
    public sealed class PlayerBaseStatsDefinition : ScriptableObject
    {
        [Header("Combat")]
        [SerializeField, Min(1)] private int _attackDamage = 1;
        [SerializeField, Min(0.05f)] private float _attacksPerSecond = 1.82f;
        [SerializeField, Min(0f)] private float _attackRange = 8f;
        [SerializeField, Min(0f)] private float _projectileSpeed = 15f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _forwardSpeed = 3.5f;
        [SerializeField, Min(0f)] private float _verticalSpeed = 5f;

        public PlayerStatsSnapshot CreateSnapshot()
        {
            return new PlayerStatsSnapshot(
                _attackDamage,
                _attacksPerSecond,
                _attackRange,
                _projectileSpeed,
                _forwardSpeed,
                _verticalSpeed);
        }

        private void OnValidate()
        {
            _attackDamage = Mathf.Max(1, _attackDamage);
            _attacksPerSecond = Mathf.Max(0.05f, _attacksPerSecond);
            _attackRange = Mathf.Max(0f, _attackRange);
            _projectileSpeed = Mathf.Max(0f, _projectileSpeed);
            _forwardSpeed = Mathf.Max(0f, _forwardSpeed);
            _verticalSpeed = Mathf.Max(0f, _verticalSpeed);
        }
    }
}
