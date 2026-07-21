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
        private const int DefaultPoolCapacity = 6;

        [Header("References")]
        [SerializeField] private TestRunnerController _runner;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Transform _enemyRoot;
        [SerializeField] private PrototypeEnemyMover _enemyTemplate;
        [SerializeField] private SpawnPatternDefinition[] _patterns;

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

        private void WarmPool()
        {
            if (_enemyTemplate == null || _enemyRoot == null)
            {
                return;
            }

            _enemyTemplate.gameObject.SetActive(false);
            _pool.Add(_enemyTemplate);

            for (var index = 1; index < _poolCapacity; index++)
            {
                var enemy = Instantiate(_enemyTemplate, _enemyRoot);
                enemy.name = $"PrototypeEnemy_{index:00}";
                enemy.gameObject.SetActive(false);
                _pool.Add(enemy);
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

            var pattern = SelectPattern(currentDistance);
            if (pattern == null || CountActiveEnemies() >= pattern.MaximumConcurrentEnemies)
            {
                return;
            }

            SpawnPattern(pattern);
            _nextWaveDistanceMeters = currentDistance + pattern.CooldownDistanceMeters;
        }

        private SpawnPatternDefinition SelectPattern(float currentDistance)
        {
            if (_patterns == null)
            {
                return null;
            }

            SpawnPatternDefinition selectedPattern = null;

            for (var index = 0; index < _patterns.Length; index++)
            {
                var pattern = _patterns[index];
                if (pattern != null && pattern.MinimumDistanceMeters <= currentDistance)
                {
                    selectedPattern = pattern;
                }
            }

            return selectedPattern;
        }

        private void SpawnPattern(SpawnPatternDefinition pattern)
        {
            var firstSpawnX = _worldCamera.transform.position.x + GetHalfViewWidth() + _spawnRightPadding;

            for (var index = 0; index < pattern.EnemyCount; index++)
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
                enemy.Spawn(spawnPosition, pattern.EnemySpeed);
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
                && _pool.Count > 0;
        }
    }
}
