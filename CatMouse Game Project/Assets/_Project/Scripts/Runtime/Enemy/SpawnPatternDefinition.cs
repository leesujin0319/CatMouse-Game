using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [CreateAssetMenu(fileName = "SpawnPatternDefinition", menuName = "CatMouse/Run/Spawn Pattern Definition")]
    public sealed class SpawnPatternDefinition : ScriptableObject
    {
        [Header("Difficulty")]
        [SerializeField, Min(1)] private int _minimumDifficultyLevel = 1;

        [Header("Spawn Pace")]
        [SerializeField, Min(1f)] private float _cooldownDistanceMeters = 35f;
        [SerializeField, Min(1)] private int _maximumConcurrentEnemies = 4;

        [Header("Encounter Area")]
        [SerializeField, Min(1)] private int _minimumEnemyCount = 1;
        [SerializeField, Min(1)] private int _maximumEnemyCount = 1;
        [SerializeField] private EnemyArchetypeDefinition[] _enemyArchetypes;
        [SerializeField, Min(0f)] private float _minimumEncounterWidth = 2.5f;
        [SerializeField, Min(0f)] private float _maximumEncounterWidth = 4.5f;
        [SerializeField, Min(0f)] private float _minimumEncounterHeight = 2f;
        [SerializeField, Min(0f)] private float _maximumEncounterHeight = 4f;

        [Header("Enemy Motion")]
        [SerializeField, Min(0f)] private float _enemySpeed = 2f;

        public int MinimumDifficultyLevel => _minimumDifficultyLevel;
        public float CooldownDistanceMeters => _cooldownDistanceMeters;
        public int MaximumConcurrentEnemies => _maximumConcurrentEnemies;
        public float EnemySpeed => _enemySpeed;

        public int GetEnemyCount()
        {
            return Random.Range(_minimumEnemyCount, _maximumEnemyCount + 1);
        }

        public Vector2 GetEncounterSize()
        {
            return new Vector2(
                Random.Range(_minimumEncounterWidth, _maximumEncounterWidth),
                Random.Range(_minimumEncounterHeight, _maximumEncounterHeight));
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
            _minimumDifficultyLevel = Mathf.Max(1, _minimumDifficultyLevel);
            _cooldownDistanceMeters = Mathf.Max(1f, _cooldownDistanceMeters);
            _maximumConcurrentEnemies = Mathf.Max(1, _maximumConcurrentEnemies);
            _minimumEnemyCount = Mathf.Max(1, _minimumEnemyCount);
            _maximumEnemyCount = Mathf.Max(_minimumEnemyCount, _maximumEnemyCount);
            _minimumEncounterWidth = Mathf.Max(0f, _minimumEncounterWidth);
            _maximumEncounterWidth = Mathf.Max(_minimumEncounterWidth, _maximumEncounterWidth);
            _minimumEncounterHeight = Mathf.Max(0f, _minimumEncounterHeight);
            _maximumEncounterHeight = Mathf.Max(_minimumEncounterHeight, _maximumEncounterHeight);
            _enemySpeed = Mathf.Max(0f, _enemySpeed);
        }
    }
}
