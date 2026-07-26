using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [CreateAssetMenu(
        fileName = "InfiniteSpawnScheduleDefinition",
        menuName = "CatMouse/Run/Infinite Spawn Schedule Definition")]
    public sealed class InfiniteSpawnScheduleDefinition : ScriptableObject
    {
        private const float MinimumCycleLengthMeters = 1f;
        private const float MinimumCooldownDistanceMeters = 1f;

        [Header("Cycle")]
        [SerializeField, Min(MinimumCycleLengthMeters)] private float _cycleLengthMeters = 300f;
        [SerializeField, Min(0)] private int _maximumDifficultyCycles = 6;

        [Header("Difficulty Per Cycle")]
        [SerializeField, Min(0f)] private float _cooldownReductionMeters = 2f;
        [SerializeField, Min(0f)] private float _enemySpeedIncrease = 0.15f;
        [SerializeField, Min(0)] private int _concurrentEnemyBonus = 1;

        [Header("Patterns")]
        [SerializeField] private SpawnPatternDefinition[] _patterns;

        public bool TryResolve(
            float totalDistanceMeters,
            out SpawnPatternDefinition pattern,
            out int difficultyCycle)
        {
            pattern = null;
            difficultyCycle = 0;

            if (_patterns == null || _patterns.Length == 0)
            {
                return false;
            }

            var clampedDistance = Mathf.Max(0f, totalDistanceMeters);
            var cycleIndex = Mathf.FloorToInt(clampedDistance / _cycleLengthMeters);
            var cycleDistance = clampedDistance % _cycleLengthMeters;
            var highestMinimumDistance = float.NegativeInfinity;

            for (var index = 0; index < _patterns.Length; index++)
            {
                var candidate = _patterns[index];
                if (candidate == null ||
                    candidate.MinimumDistanceMeters > cycleDistance ||
                    candidate.MinimumDistanceMeters < highestMinimumDistance)
                {
                    continue;
                }

                pattern = candidate;
                highestMinimumDistance = candidate.MinimumDistanceMeters;
            }

            if (pattern == null)
            {
                return false;
            }

            difficultyCycle = Mathf.Min(cycleIndex, _maximumDifficultyCycles);
            return true;
        }

        public float GetCooldownDistance(
            SpawnPatternDefinition pattern,
            int difficultyCycle)
        {
            if (pattern == null)
            {
                return MinimumCooldownDistanceMeters;
            }

            return Mathf.Max(
                MinimumCooldownDistanceMeters,
                pattern.CooldownDistanceMeters - (_cooldownReductionMeters * difficultyCycle));
        }

        public float GetEnemySpeed(
            SpawnPatternDefinition pattern,
            int difficultyCycle)
        {
            if (pattern == null)
            {
                return 0f;
            }

            return pattern.EnemySpeed + (_enemySpeedIncrease * difficultyCycle);
        }

        public int GetMaximumConcurrentEnemies(
            SpawnPatternDefinition pattern,
            int difficultyCycle)
        {
            if (pattern == null)
            {
                return 1;
            }

            return pattern.MaximumConcurrentEnemies + (_concurrentEnemyBonus * difficultyCycle);
        }

        private void OnValidate()
        {
            _cycleLengthMeters = Mathf.Max(MinimumCycleLengthMeters, _cycleLengthMeters);
            _maximumDifficultyCycles = Mathf.Max(0, _maximumDifficultyCycles);
            _cooldownReductionMeters = Mathf.Max(0f, _cooldownReductionMeters);
            _enemySpeedIncrease = Mathf.Max(0f, _enemySpeedIncrease);
            _concurrentEnemyBonus = Mathf.Max(0, _concurrentEnemyBonus);
        }
    }
}
