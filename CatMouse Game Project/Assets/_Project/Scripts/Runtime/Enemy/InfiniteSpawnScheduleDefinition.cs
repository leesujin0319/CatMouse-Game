using UnityEngine;
using CatMouse.Game.Run;

namespace CatMouse.Game.Enemy
{
    [CreateAssetMenu(
        fileName = "InfiniteSpawnScheduleDefinition",
        menuName = "CatMouse/Run/Infinite Spawn Schedule Definition")]
    public sealed class InfiniteSpawnScheduleDefinition : ScriptableObject
    {
        private const int MinimumDifficultyLevel = 1;
        private const float MinimumMetersPerDifficultyLevel = 1f;
        private const float MinimumCooldownDistanceMeters = 1f;

        [Header("Difficulty Levels")]
        [SerializeField, Min(MinimumMetersPerDifficultyLevel)] private float _metersPerDifficultyLevel = 100f;

        [Header("Difficulty Increase Per Level")]
        [SerializeField, Min(MinimumCooldownDistanceMeters)] private float _minimumCooldownDistanceMeters = 24f;
        [SerializeField, Min(0f)] private float _cooldownReductionMeters = 2f;
        [SerializeField, Min(0f)] private float _maximumEnemySpeedBonus = 0.6f;
        [SerializeField, Min(0f)] private float _enemySpeedIncrease = 0.15f;
        [SerializeField, Min(0)] private int _maximumConcurrentEnemyBonus = 5;
        [SerializeField, Min(0)] private int _concurrentEnemyBonus = 1;

        [Header("Enemy Health Per Level")]
        [SerializeField] private ExpectedPlayerPowerCurveDefinition _expectedPlayerPowerCurve;

        [Header("Player Health Loss Per Level")]
        [SerializeField, Min(0.01f)] private float _baseHealthLossMultiplier = 0.3f;
        [SerializeField, Min(0f)] private float _healthLossMultiplierIncreasePerLevel = 0.06f;
        [SerializeField, Min(0.01f)] private float _maximumHealthLossMultiplier = 0.75f;

        [Header("Patterns")]
        [SerializeField] private SpawnPatternDefinition[] _patterns;

        public bool TryResolve(
            float totalDistanceMeters,
            out SpawnPatternDefinition pattern,
            out int difficultyLevel)
        {
            pattern = null;
            difficultyLevel = MinimumDifficultyLevel;

            if (_patterns == null || _patterns.Length == 0)
            {
                return false;
            }

            difficultyLevel = GetDifficultyLevel(totalDistanceMeters);
            var eligiblePatternCount = 0;

            for (var index = 0; index < _patterns.Length; index++)
            {
                var candidate = _patterns[index];
                if (candidate == null || candidate.MinimumDifficultyLevel > difficultyLevel)
                {
                    continue;
                }

                eligiblePatternCount++;
            }

            if (eligiblePatternCount == 0)
            {
                return false;
            }

            var selectedPatternIndex = Random.Range(0, eligiblePatternCount);
            for (var index = 0; index < _patterns.Length; index++)
            {
                var candidate = _patterns[index];
                if (candidate == null || candidate.MinimumDifficultyLevel > difficultyLevel)
                {
                    continue;
                }

                if (selectedPatternIndex-- == 0)
                {
                    pattern = candidate;
                    return true;
                }
            }

            return true;
        }

        public int GetDifficultyLevel(float totalDistanceMeters)
        {
            var clampedDistance = Mathf.Max(0f, totalDistanceMeters);
            var level = MinimumDifficultyLevel +
                Mathf.FloorToInt(clampedDistance / _metersPerDifficultyLevel);
            return level;
        }

        public float GetCooldownDistance(
            SpawnPatternDefinition pattern,
            int difficultyLevel)
        {
            if (pattern == null)
            {
                return MinimumCooldownDistanceMeters;
            }

            return Mathf.Max(
                _minimumCooldownDistanceMeters,
                pattern.CooldownDistanceMeters - (_cooldownReductionMeters * GetDifficultyStep(difficultyLevel)));
        }

        public float GetEnemySpeed(
            SpawnPatternDefinition pattern,
            int difficultyLevel)
        {
            if (pattern == null)
            {
                return 0f;
            }

            var speedBonus = Mathf.Min(
                _maximumEnemySpeedBonus,
                _enemySpeedIncrease * GetDifficultyStep(difficultyLevel));
            return pattern.EnemySpeed + speedBonus;
        }

        public int GetMaximumConcurrentEnemies(
            SpawnPatternDefinition pattern,
            int difficultyLevel)
        {
            if (pattern == null)
            {
                return 1;
            }

            var concurrentBonus = Mathf.Min(
                _maximumConcurrentEnemyBonus,
                _concurrentEnemyBonus * GetDifficultyStep(difficultyLevel));
            return pattern.MaximumConcurrentEnemies + concurrentBonus;
        }

        public float GetEnemyHealthMultiplier(int difficultyLevel)
        {
            return _expectedPlayerPowerCurve != null
                ? _expectedPlayerPowerCurve.GetExpectedDpsMultiplier(difficultyLevel)
                : 1f;
        }

        public float GetHealthLossMultiplier(int difficultyLevel)
        {
            return Mathf.Min(
                _maximumHealthLossMultiplier,
                _baseHealthLossMultiplier +
                (_healthLossMultiplierIncreasePerLevel * GetDifficultyStep(difficultyLevel)));
        }

        private static int GetDifficultyStep(int difficultyLevel)
        {
            return Mathf.Max(0, difficultyLevel - MinimumDifficultyLevel);
        }

        private void OnValidate()
        {
            _metersPerDifficultyLevel = Mathf.Max(MinimumMetersPerDifficultyLevel, _metersPerDifficultyLevel);
            _minimumCooldownDistanceMeters = Mathf.Max(
                MinimumCooldownDistanceMeters,
                _minimumCooldownDistanceMeters);
            _cooldownReductionMeters = Mathf.Max(0f, _cooldownReductionMeters);
            _maximumEnemySpeedBonus = Mathf.Max(0f, _maximumEnemySpeedBonus);
            _enemySpeedIncrease = Mathf.Max(0f, _enemySpeedIncrease);
            _maximumConcurrentEnemyBonus = Mathf.Max(0, _maximumConcurrentEnemyBonus);
            _concurrentEnemyBonus = Mathf.Max(0, _concurrentEnemyBonus);
            _baseHealthLossMultiplier = Mathf.Max(0.01f, _baseHealthLossMultiplier);
            _healthLossMultiplierIncreasePerLevel = Mathf.Max(0f, _healthLossMultiplierIncreasePerLevel);
            _maximumHealthLossMultiplier = Mathf.Max(
                _baseHealthLossMultiplier,
                _maximumHealthLossMultiplier);
        }
    }
}
