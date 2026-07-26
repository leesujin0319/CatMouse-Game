using System.Collections.Generic;
using CatMouse.Game.Player;
using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [DisallowMultipleComponent]
    public sealed class PrototypeEnemySpawner : MonoBehaviour
    {
        private const float DefaultSpawnRightPadding = 1.25f;
        private const float DefaultDespawnLeftPadding = 1.25f;
        private const float DefaultMinimumSpawnY = -3.1f;
        private const float DefaultMaximumSpawnY = 1.55f;
        private const float EnemyHitHeightOffset = 0.35f;
        private const int DefaultPoolCapacity = 12;

        [Header("References")]
        [SerializeField] private TestRunnerController _runner;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Transform _enemyRoot;
        [SerializeField] private PrototypeEnemyMover _enemyTemplate;
        [SerializeField] private PrototypeCheeseDropPool _cheeseDropPool;
        [SerializeField] private InfiniteSpawnScheduleDefinition _spawnSchedule;

        [Header("Spawn Area")]
        [SerializeField, Min(0f)] private float _spawnRightPadding = DefaultSpawnRightPadding;
        [SerializeField, Min(0f)] private float _despawnLeftPadding = DefaultDespawnLeftPadding;
        [SerializeField] private float _minimumSpawnY = DefaultMinimumSpawnY;
        [SerializeField] private float _maximumSpawnY = DefaultMaximumSpawnY;
        [SerializeField, Min(1)] private int _poolCapacity = DefaultPoolCapacity;

        private readonly List<PrototypeEnemyMover> _pool = new();
        private float _nextWaveDistanceMeters;

        private void Awake()
        {
            WarmPool();

            if (_cheeseDropPool != null && _runner != null)
            {
                _cheeseDropPool.SetCollector(_runner.transform);
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || !HasValidConfiguration())
            {
                return;
            }

            TickActiveEnemies();
            TrySpawnNextWave();
        }

        private void OnValidate()
        {
            _spawnRightPadding = Mathf.Max(0f, _spawnRightPadding);
            _despawnLeftPadding = Mathf.Max(0f, _despawnLeftPadding);
            _poolCapacity = Mathf.Max(1, _poolCapacity);

            if (_minimumSpawnY > _maximumSpawnY)
            {
                _maximumSpawnY = _minimumSpawnY;
            }
        }

        private void OnDestroy()
        {
            for (var index = 0; index < _pool.Count; index++)
            {
                var enemy = _pool[index];
                if (enemy != null)
                {
                    enemy.Defeated -= HandleEnemyDefeated;
                }
            }
        }

        public bool TryDamageFirstEnemyInSegment(
            Vector3 segmentStart,
            Vector3 segmentEnd,
            float hitRadius,
            int damage,
            Vector2 hitDirection)
        {
            var segment = (Vector2)(segmentEnd - segmentStart);
            var segmentSqrLength = segment.sqrMagnitude;
            if (segmentSqrLength <= Mathf.Epsilon)
            {
                return false;
            }

            PrototypeEnemyMover firstEnemy = null;
            var firstProgress = float.PositiveInfinity;
            var hitRadiusSqr = Mathf.Max(0f, hitRadius) * Mathf.Max(0f, hitRadius);

            for (var index = 0; index < _pool.Count; index++)
            {
                var enemy = _pool[index];
                if (enemy == null || enemy.IsAvailable)
                {
                    continue;
                }

                var enemyPosition = (Vector2)enemy.transform.position
                    + (Vector2.up * EnemyHitHeightOffset);
                var fromStart = enemyPosition - (Vector2)segmentStart;
                var progress = Mathf.Clamp01(Vector2.Dot(fromStart, segment) / segmentSqrLength);
                var closestPoint = (Vector2)segmentStart + (segment * progress);

                if ((enemyPosition - closestPoint).sqrMagnitude <= hitRadiusSqr
                    && progress < firstProgress)
                {
                    firstProgress = progress;
                    firstEnemy = enemy;
                }
            }

            if (firstEnemy == null)
            {
                return false;
            }

            firstEnemy.TryTakeDamage(damage, hitDirection);
            return true;
        }

        private void WarmPool()
        {
            if (_enemyTemplate == null || _enemyRoot == null)
            {
                return;
            }

            _enemyTemplate.gameObject.SetActive(false);
            RegisterEnemy(_enemyTemplate);

            for (var index = 1; index < _poolCapacity; index++)
            {
                var enemy = Instantiate(_enemyTemplate, _enemyRoot);
                enemy.name = $"PrototypeEnemy_{index:00}";
                enemy.gameObject.SetActive(false);
                RegisterEnemy(enemy);
            }
        }

        private void RegisterEnemy(PrototypeEnemyMover enemy)
        {
            enemy.Defeated += HandleEnemyDefeated;
            _pool.Add(enemy);
        }

        private void HandleEnemyDefeated(Vector3 position, Vector2 hitDirection)
        {
            if (_cheeseDropPool != null)
            {
                _cheeseDropPool.Scatter(position, -hitDirection);
            }
        }

        private void TickActiveEnemies()
        {
            var leftBoundary = _worldCamera.transform.position.x - GetHalfViewWidth() - _despawnLeftPadding;

            for (var index = 0; index < _pool.Count; index++)
            {
                var enemy = _pool[index];
                if (enemy != null && !enemy.IsAvailable)
                {
                    enemy.Tick(Time.deltaTime, leftBoundary);
                }
            }
        }

        private void TrySpawnNextWave()
        {
            var currentDistance = _runner.DistanceMeters;
            if (currentDistance < _nextWaveDistanceMeters)
            {
                return;
            }

            if (!_spawnSchedule.TryResolve(
                    currentDistance,
                    out var pattern,
                    out var difficultyCycle))
            {
                return;
            }

            var availableEnemyCount =
                _spawnSchedule.GetMaximumConcurrentEnemies(pattern, difficultyCycle) -
                CountActiveEnemies();
            if (availableEnemyCount <= 0)
            {
                return;
            }

            SpawnPattern(
                pattern,
                _spawnSchedule.GetEnemySpeed(pattern, difficultyCycle),
                availableEnemyCount);
            _nextWaveDistanceMeters =
                currentDistance +
                _spawnSchedule.GetCooldownDistance(pattern, difficultyCycle);
        }

        private void SpawnPattern(
            SpawnPatternDefinition pattern,
            float enemySpeed,
            int maximumSpawnCount)
        {
            var firstSpawnX = _worldCamera.transform.position.x + GetHalfViewWidth() + _spawnRightPadding;
            var spawnCount = Mathf.Min(pattern.EnemyCount, maximumSpawnCount);

            for (var index = 0; index < spawnCount; index++)
            {
                var enemy = GetAvailableEnemy();
                if (enemy == null)
                {
                    return;
                }

                var spawnPosition = new Vector3(
                    firstSpawnX + (index * pattern.HorizontalSpacing),
                    Mathf.Clamp(pattern.GetVerticalOffset(index), _minimumSpawnY, _maximumSpawnY),
                    0f);
                enemy.Spawn(
                    spawnPosition,
                    enemySpeed,
                    pattern.GetEnemyArchetype(index));
            }
        }

        private PrototypeEnemyMover GetAvailableEnemy()
        {
            for (var index = 0; index < _pool.Count; index++)
            {
                var enemy = _pool[index];
                if (enemy != null && enemy.IsAvailable)
                {
                    return enemy;
                }
            }

            return null;
        }

        private int CountActiveEnemies()
        {
            var activeEnemyCount = 0;

            for (var index = 0; index < _pool.Count; index++)
            {
                var enemy = _pool[index];
                if (enemy != null && !enemy.IsAvailable)
                {
                    activeEnemyCount++;
                }
            }

            return activeEnemyCount;
        }

        private float GetHalfViewWidth()
        {
            return _worldCamera.orthographicSize * _worldCamera.aspect;
        }

        private bool HasValidConfiguration()
        {
            return _runner != null
                && _worldCamera != null
                && _enemyRoot != null
                && _enemyTemplate != null
                && _spawnSchedule != null
                && _pool.Count > 0;
        }
    }
}
