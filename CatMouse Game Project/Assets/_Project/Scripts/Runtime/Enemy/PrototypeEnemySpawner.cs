using System.Collections.Generic;
using CatMouse.Game.Player;
using CatMouse.Game.Run;
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
        private const int DefaultEnemyProjectilePoolCapacity = 8;
        private const float ContactDamageCooldown = 1f;
        private const int EncounterPlacementAttempts = 8;

        [Header("References")]
        [SerializeField] private TestRunnerController _runner;
        [SerializeField] private RunProgressController _runProgress;
        [SerializeField] private Transform _collector;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Transform _enemyRoot;
        [SerializeField] private PrototypeEnemyMover _enemyTemplate;
        [SerializeField] private Transform _enemyProjectileRoot;
        [SerializeField] private PrototypeEnemyProjectile _enemyProjectileTemplate;
        [SerializeField] private PrototypeCheeseDropPool _cheeseDropPool;
        [SerializeField] private RunCoinCollector _coinCollector;
        [SerializeField] private InfiniteSpawnScheduleDefinition _spawnSchedule;
        [SerializeField] private RunVerticalBounds _verticalBounds;

        [Header("Spawn Area")]
        [SerializeField, Min(0f)] private float _spawnRightPadding = DefaultSpawnRightPadding;
        [SerializeField, Min(0f)] private float _despawnLeftPadding = DefaultDespawnLeftPadding;
        [SerializeField] private float _minimumSpawnY = DefaultMinimumSpawnY;
        [SerializeField] private float _maximumSpawnY = DefaultMaximumSpawnY;
        [SerializeField, Min(1)] private int _poolCapacity = DefaultPoolCapacity;
        [SerializeField, Min(1)] private int _enemyProjectilePoolCapacity = DefaultEnemyProjectilePoolCapacity;

        private readonly List<PrototypeEnemyMover> _pool = new();
        private readonly List<PrototypeEnemyProjectile> _enemyProjectiles = new();
        private PlayerRunHealth _collectorHealth;
        private Collider2D _collectorCollider;
        private Transform _resolvedCollector;
        private float _nextWaveDistanceMeters;
        private float _nextContactDamageTime;

        private void Awake()
        {
            WarmPool();
            WarmEnemyProjectilePool();

            _resolvedCollector = _collector != null
                ? _collector
                : _runner != null
                    ? _runner.transform
                    : null;
            _collectorHealth = _resolvedCollector != null ? _resolvedCollector.GetComponent<PlayerRunHealth>() : null;
            _collectorCollider = _resolvedCollector != null ? _resolvedCollector.GetComponent<Collider2D>() : null;
            if (_cheeseDropPool != null && _resolvedCollector != null)
            {
                _cheeseDropPool.SetCollector(_resolvedCollector);
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
            _enemyProjectilePoolCapacity = Mathf.Max(1, _enemyProjectilePoolCapacity);

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
        public PrototypeEnemyMover FindClosestActiveEnemy(Vector3 origin, float maximumRange)
        {
            float maximumDistanceSqr = maximumRange > 0f
                ? maximumRange * maximumRange
                : float.PositiveInfinity;
            PrototypeEnemyMover closestEnemy = null;
            float closestDistanceSqr = maximumDistanceSqr;
            for (var index = 0; index < _pool.Count; index++)
            {
                PrototypeEnemyMover enemy = _pool[index];
                if (enemy == null || enemy.IsAvailable)
                {
                    continue;
                }
                float distanceSqr = (enemy.transform.position - origin).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }
        public int DamageEnemiesInRadius(Vector3 origin, float radius, int damage)
        {
            if (radius <= 0f || damage <= 0)
            {
                return 0;
            }
            int damagedEnemyCount = 0;
            float radiusSqr = radius * radius;
            for (var index = 0; index < _pool.Count; index++)
            {
                PrototypeEnemyMover enemy = _pool[index];
                if (enemy == null
                    || enemy.IsAvailable
                    || (enemy.transform.position - origin).sqrMagnitude > radiusSqr)
                {
                    continue;
                }
                enemy.TryTakeDamage(damage, Vector2.right);
                damagedEnemyCount++;
            }
            return damagedEnemyCount;
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

        private void WarmEnemyProjectilePool()
        {
            if (_enemyProjectileTemplate == null || _enemyProjectileRoot == null)
            {
                return;
            }

            _enemyProjectileTemplate.gameObject.SetActive(false);
            _enemyProjectiles.Add(_enemyProjectileTemplate);

            for (int index = 1; index < _enemyProjectilePoolCapacity; index++)
            {
                PrototypeEnemyProjectile projectile = Instantiate(_enemyProjectileTemplate, _enemyProjectileRoot);
                projectile.name = $"EnemyProjectile_{index:00}";
                projectile.gameObject.SetActive(false);
                _enemyProjectiles.Add(projectile);
            }
        }

        private void TickEnemyProjectiles()
        {
            for (int index = 0; index < _enemyProjectiles.Count; index++)
            {
                PrototypeEnemyProjectile projectile = _enemyProjectiles[index];
                if (projectile != null && !projectile.IsAvailable)
                {
                    projectile.Tick(Time.deltaTime);
                }
            }
        }

        private void HandleEnemyDefeated(Vector3 position, Vector2 hitDirection)
        {
            Vector2 rewardScatterDirection = hitDirection.sqrMagnitude > Mathf.Epsilon
                ? -hitDirection.normalized
                : Vector2.left;

            if (_cheeseDropPool != null)
            {
                _cheeseDropPool.Scatter(position, rewardScatterDirection);
            }

            _coinCollector?.TryDrop(position, rewardScatterDirection);
        }

        private void TickActiveEnemies()
        {
            var leftBoundary = _worldCamera.transform.position.x - GetHalfViewWidth() - _despawnLeftPadding;
            var worldScrollDelta = _runProgress != null
                ? _runProgress.CurrentWorldScrollDelta
                : 0f;

            for (var index = 0; index < _pool.Count; index++)
            {
                var enemy = _pool[index];
                if (enemy != null && !enemy.IsAvailable)
                {
                    enemy.Tick(Time.deltaTime, leftBoundary, worldScrollDelta);
                    if (!enemy.IsAvailable)
                    {
                        TryDealContactDamage(enemy);
                        TryFireRangedAttack(enemy);
                    }
                }
            }

            TickEnemyProjectiles();
        }

        private void TryDealContactDamage(PrototypeEnemyMover enemy)
        {
            if (_collectorHealth == null
                || _collectorCollider == null
                || Time.time < _nextContactDamageTime)
            {
                return;
            }

            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider == null || !enemyCollider.Distance(_collectorCollider).isOverlapped)
            {
                return;
            }

            if (_collectorHealth.TryTakeDamage(enemy.ContactDamage * enemy.NormalizedHealth))
            {
                _nextContactDamageTime = Time.time + ContactDamageCooldown;
                enemy.PlayAttackPresentation();
            }
        }

        private void TryFireRangedAttack(PrototypeEnemyMover enemy)
        {
            if (_resolvedCollector == null
                || _collectorHealth == null
                || _collectorCollider == null
                || !enemy.UsesRangedAttack)
            {
                return;
            }

            PrototypeEnemyProjectile projectile = GetAvailableEnemyProjectile();
            if (projectile == null || !enemy.TryStartRangedAttack(_resolvedCollector.position))
            {
                return;
            }

            Vector2 direction = _resolvedCollector.position - enemy.RangedAttackOrigin;
            projectile.Spawn(
                enemy.RangedAttackOrigin,
                direction,
                enemy.RangedProjectileSpeed,
                enemy.RangedProjectileDamage,
                _collectorHealth,
                _collectorCollider);
        }

        private void TrySpawnNextWave()
        {
            float currentDistance = _runProgress != null
                ? _runProgress.DistanceMeters
                : _runner.DistanceMeters;
            if (currentDistance < _nextWaveDistanceMeters)
            {
                return;
            }

            if (!_spawnSchedule.TryResolve(
                    currentDistance,
                    out var pattern,
                    out var difficultyLevel))
            {
                return;
            }

            var availableEnemyCount =
                _spawnSchedule.GetMaximumConcurrentEnemies(pattern, difficultyLevel) -
                CountActiveEnemies();
            if (availableEnemyCount <= 0)
            {
                return;
            }

            SpawnPattern(
                pattern,
                _spawnSchedule.GetEnemySpeed(pattern, difficultyLevel),
                availableEnemyCount,
                _spawnSchedule.GetEnemyHealthMultiplier(difficultyLevel));
            _nextWaveDistanceMeters =
                currentDistance +
                _spawnSchedule.GetCooldownDistance(pattern, difficultyLevel);
        }

        private void SpawnPattern(
            SpawnPatternDefinition pattern,
            float enemySpeed,
            int maximumSpawnCount,
            float healthMultiplier)
        {
            var spawnCount = Mathf.Min(pattern.GetEnemyCount(), maximumSpawnCount);
            var encounterSize = pattern.GetEncounterSize();
            GetSpawnVerticalRange(out var minimumY, out var maximumY);

            var encounterCenter = new Vector2(
                _worldCamera.transform.position.x + GetHalfViewWidth() + _spawnRightPadding + (encounterSize.x * 0.5f),
                GetEncounterCenterY(minimumY, maximumY, encounterSize.y));
            var memberOffsets = new Vector2[spawnCount];

            for (var index = 0; index < spawnCount; index++)
            {
                var enemy = GetAvailableEnemy();
                if (enemy == null)
                {
                    return;
                }

                memberOffsets[index] = GetEncounterMemberOffset(index, memberOffsets, encounterSize);
                var spawnPosition = new Vector3(
                    encounterCenter.x + memberOffsets[index].x,
                    Mathf.Clamp(encounterCenter.y + memberOffsets[index].y, minimumY, maximumY),
                    0f);
                enemy.Spawn(
                    spawnPosition,
                    enemySpeed,
                    pattern.GetEnemyArchetype(index),
                    healthMultiplier);
            }
        }

        private void GetSpawnVerticalRange(out float minimumY, out float maximumY)
        {
            minimumY = _minimumSpawnY;
            maximumY = _maximumSpawnY;

            if (_verticalBounds != null
                && _verticalBounds.TryGetMovementRange(out var floorMinimumY, out var floorMaximumY))
            {
                minimumY = floorMinimumY;
                maximumY = floorMaximumY;
            }
        }

        private static float GetEncounterCenterY(float minimumY, float maximumY, float encounterHeight)
        {
            var halfEncounterHeight = encounterHeight * 0.5f;
            var innerMinimumY = minimumY + halfEncounterHeight;
            var innerMaximumY = maximumY - halfEncounterHeight;

            return innerMinimumY <= innerMaximumY
                ? Random.Range(innerMinimumY, innerMaximumY)
                : (minimumY + maximumY) * 0.5f;
        }

        private static Vector2 GetEncounterMemberOffset(
            int memberIndex,
            Vector2[] existingOffsets,
            Vector2 encounterSize)
        {
            if (existingOffsets.Length <= 1)
            {
                return Vector2.zero;
            }

            var minimumDistance = Mathf.Min(encounterSize.x, encounterSize.y) * 0.5f;
            var minimumDistanceSqr = minimumDistance * minimumDistance;

            for (var attempt = 0; attempt < EncounterPlacementAttempts; attempt++)
            {
                var candidate = new Vector2(
                    Random.Range(-encounterSize.x, encounterSize.x) * 0.5f,
                    Random.Range(-encounterSize.y, encounterSize.y) * 0.5f);
                var overlapsExistingOffset = false;

                for (var index = 0; index < memberIndex; index++)
                {
                    if ((candidate - existingOffsets[index]).sqrMagnitude < minimumDistanceSqr)
                    {
                        overlapsExistingOffset = true;
                        break;
                    }
                }

                if (!overlapsExistingOffset)
                {
                    return candidate;
                }
            }

            var fallbackAngle = memberIndex * Mathf.PI * 2f / existingOffsets.Length;
            return new Vector2(
                Mathf.Cos(fallbackAngle) * encounterSize.x * 0.5f,
                Mathf.Sin(fallbackAngle) * encounterSize.y * 0.5f);
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

        private PrototypeEnemyProjectile GetAvailableEnemyProjectile()
        {
            for (int index = 0; index < _enemyProjectiles.Count; index++)
            {
                PrototypeEnemyProjectile projectile = _enemyProjectiles[index];
                if (projectile != null && projectile.IsAvailable)
                {
                    return projectile;
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
            return (_runProgress != null || _runner != null)
                && _worldCamera != null
                && _enemyRoot != null
                && _enemyTemplate != null
                && _spawnSchedule != null
                && _pool.Count > 0;
        }
    }
}
