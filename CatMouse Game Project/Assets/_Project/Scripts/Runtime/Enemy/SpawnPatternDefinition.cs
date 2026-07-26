using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [CreateAssetMenu(fileName = "SpawnPatternDefinition", menuName = "CatMouse/Run/Spawn Pattern Definition")]
    public sealed class SpawnPatternDefinition : ScriptableObject
    {
        [Header("Distance")]
        [SerializeField, Min(0f)] private float _minimumDistanceMeters;
        [SerializeField, Min(1f)] private float _cooldownDistanceMeters = 35f;
        [SerializeField, Min(1)] private int _maximumConcurrentEnemies = 4;

        [Header("Formation")]
        [SerializeField, Min(1)] private int _enemyCount = 1;
        [SerializeField] private EnemyArchetypeDefinition[] _enemyArchetypes;
        [SerializeField] private float[] _verticalOffsets = { 0f };
        [SerializeField, Min(0f)] private float _horizontalSpacing = 0.75f;

        [Header("Enemy Motion")]
        [SerializeField, Min(0f)] private float _enemySpeed = 2f;

        public float MinimumDistanceMeters => _minimumDistanceMeters;
        public float CooldownDistanceMeters => _cooldownDistanceMeters;
        public int MaximumConcurrentEnemies => _maximumConcurrentEnemies;
        public int EnemyCount => _enemyCount;
        public float HorizontalSpacing => _horizontalSpacing;
        public float EnemySpeed => _enemySpeed;

        public float GetVerticalOffset(int index)
        {
            if (_verticalOffsets == null || _verticalOffsets.Length == 0)
            {
                return 0f;
            }

            return _verticalOffsets[index % _verticalOffsets.Length];
        }

        public EnemyArchetypeDefinition GetEnemyArchetype(int index)
        {
            if (_enemyArchetypes == null || _enemyArchetypes.Length == 0)
            {
                return null;
            }

            return _enemyArchetypes[index % _enemyArchetypes.Length];
        }

        private void OnValidate()
        {
            _minimumDistanceMeters = Mathf.Max(0f, _minimumDistanceMeters);
            _cooldownDistanceMeters = Mathf.Max(1f, _cooldownDistanceMeters);
            _maximumConcurrentEnemies = Mathf.Max(1, _maximumConcurrentEnemies);
            _enemyCount = Mathf.Max(1, _enemyCount);
            _horizontalSpacing = Mathf.Max(0f, _horizontalSpacing);
            _enemySpeed = Mathf.Max(0f, _enemySpeed);
        }
    }
}
