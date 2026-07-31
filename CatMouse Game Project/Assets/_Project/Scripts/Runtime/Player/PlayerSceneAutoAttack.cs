using System.Collections.Generic;
using CatMouse.Game.Enemy;
using UnityEngine;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerSceneAutoAttack : MonoBehaviour
    {
        private const int ProjectilePoolCapacity = 8;
        private const float ProjectileLifetime = 2f;
        private const float TargetScanInterval = 0.1f;
        private const float DefaultHitRadius = 0.35f;

        [Header("References")]
        [SerializeField] private PlayerRunStats _runStats;
        [SerializeField] private PlayerRunHealth _runHealth;
        [SerializeField] private PrototypeEnemySpawner _enemySpawner;
        [SerializeField] private PlayerRunCombatEvolution _combatEvolution;
        [SerializeField] private Transform _projectileRoot;
        [SerializeField] private PlayerSceneAcornProjectile _projectileTemplate;

        [Header("Targeting")]
        [SerializeField] private LayerMask _targetLayers = ~0;
        [SerializeField] private Vector2 _attackOriginOffset = new(0.55f, 0.25f);
        [SerializeField, Min(1)] private int _projectilePoolCapacity = ProjectilePoolCapacity;
        [SerializeField, Min(0.1f)] private float _projectileLifetime = ProjectileLifetime;
        [SerializeField, Min(0.05f)] private float _targetScanInterval = TargetScanInterval;

        private readonly List<PlayerSceneAcornProjectile> _projectiles = new();
        private readonly Collider2D[] _targetColliders = new Collider2D[16];

        private float _attackCooldown;
        private float _targetScanCooldown;
        private PlayerSceneTestEnemy _currentTarget;

        private void Awake()
        {
            WarmPool();
        }

        private void OnEnable()
        {
            if (_runStats != null)
            {
                _runStats.StatsChanged += HandleStatsChanged;
            }
        }

        private void OnDisable()
        {
            if (_runStats != null)
            {
                _runStats.StatsChanged -= HandleStatsChanged;
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || !HasValidConfiguration())
            {
                return;
            }

            TickProjectiles();

            if (_enemySpawner != null)
            {
                _attackCooldown -= Time.deltaTime;
                if (_attackCooldown <= 0f)
                {
                    FireForward();
                }

                return;
            }

            UpdateTarget();

            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown > 0f || _currentTarget == null)
            {
                return;
            }

            TryFireAtCurrentTarget();
        }

        private void OnValidate()
        {
            _projectilePoolCapacity = Mathf.Max(1, _projectilePoolCapacity);
            _projectileLifetime = Mathf.Max(0.1f, _projectileLifetime);
            _targetScanInterval = Mathf.Max(0.05f, _targetScanInterval);
        }

        private void WarmPool()
        {
            if (_projectileTemplate == null || _projectileRoot == null)
            {
                return;
            }

            _projectileTemplate.gameObject.SetActive(false);
            _projectiles.Add(_projectileTemplate);

            for (int index = 1; index < _projectilePoolCapacity; index++)
            {
                PlayerSceneAcornProjectile projectile = Instantiate(_projectileTemplate, _projectileRoot);
                projectile.name = $"PlayerSceneAcorn_{index:00}";
                projectile.gameObject.SetActive(false);
                _projectiles.Add(projectile);
            }
        }

        private void TickProjectiles()
        {
            for (int index = 0; index < _projectiles.Count; index++)
            {
                PlayerSceneAcornProjectile projectile = _projectiles[index];
                if (projectile != null && !projectile.IsAvailable)
                {
                    projectile.Tick(Time.deltaTime);
                }
            }
        }

        private void UpdateTarget()
        {
            _targetScanCooldown -= Time.deltaTime;
            if (_targetScanCooldown > 0f && IsTargetInRange(_currentTarget))
            {
                return;
            }

            _targetScanCooldown = _targetScanInterval;
            _currentTarget = FindClosestTarget(_runStats.Current.AttackRange);
        }

        private void TryFireAtCurrentTarget()
        {
            PlayerSceneAcornProjectile projectile = GetAvailableProjectile();
            if (projectile == null)
            {
                return;
            }

            PlayerStatsSnapshot stats = _runStats.Current;
            projectile.Spawn(
                transform.position + (Vector3)_attackOriginOffset,
                _currentTarget,
                stats.ProjectileSpeed,
                stats.AttackDamage,
                _projectileLifetime,
                DefaultHitRadius,
                _runHealth);
            _attackCooldown = 1f / stats.AttacksPerSecond;
        }

        private void FireForward()
        {
            PlayerStatsSnapshot stats = _runStats.Current;
            int projectileCount = 1 + (_combatEvolution?.AdditionalProjectileCount ?? 0);
            float verticalSpacing = 0.18f;

            for (int index = 0; index < projectileCount; index++)
            {
                PlayerSceneAcornProjectile projectile = GetAvailableProjectile();
                if (projectile == null)
                {
                    break;
                }

                float verticalOffset = (index - ((projectileCount - 1) * 0.5f)) * verticalSpacing;
                Vector3 spawnPosition = transform.position
                    + (Vector3)_attackOriginOffset
                    + (Vector3.up * verticalOffset);
                PrototypeEnemyMover homingTarget = _combatEvolution != null
                    && _combatEvolution.UsesHomingProjectile
                    ? _enemySpawner.FindClosestActiveEnemy(spawnPosition, stats.AttackRange)
                    : null;

                projectile.Spawn(
                    spawnPosition,
                    _enemySpawner,
                    Vector2.right,
                    stats.ProjectileSpeed,
                    stats.AttackDamage,
                    _projectileLifetime,
                    DefaultHitRadius * (_combatEvolution?.ProjectileHitRadiusMultiplier ?? 1f),
                    homingTarget,
                    _combatEvolution?.ProjectileVisualScale ?? 1f,
                    _combatEvolution?.PoisonDamagePerTick ?? 0,
                    _combatEvolution?.PoisonDurationSeconds ?? 0f);
            }

            _attackCooldown = 1f / stats.AttacksPerSecond;
        }

        private PlayerSceneAcornProjectile GetAvailableProjectile()
        {
            for (int index = 0; index < _projectiles.Count; index++)
            {
                PlayerSceneAcornProjectile projectile = _projectiles[index];
                if (projectile != null && projectile.IsAvailable)
                {
                    return projectile;
                }
            }

            return null;
        }

        private PlayerSceneTestEnemy FindClosestTarget(float attackRange)
        {
            if (attackRange <= 0f)
            {
                return null;
            }

            int colliderCount = Physics2D.OverlapCircleNonAlloc(
                transform.position,
                attackRange,
                _targetColliders,
                _targetLayers);
            PlayerSceneTestEnemy closestTarget = null;
            float closestDistanceSqr = float.PositiveInfinity;

            for (int index = 0; index < colliderCount; index++)
            {
                Collider2D targetCollider = _targetColliders[index];
                PlayerSceneTestEnemy target = targetCollider != null
                    ? targetCollider.GetComponentInParent<PlayerSceneTestEnemy>()
                    : null;
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                float distanceSqr = (target.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestTarget = target;
                }
            }

            return closestTarget;
        }

        private bool IsTargetInRange(PlayerSceneTestEnemy target)
        {
            if (target == null || !target.IsAlive)
            {
                return false;
            }

            float attackRange = _runStats.Current.AttackRange;
            return (target.transform.position - transform.position).sqrMagnitude <= attackRange * attackRange;
        }

        private bool HasValidConfiguration()
        {
            return _runStats != null
                && _runStats.IsInitialized
                && _runHealth != null
                && _projectileRoot != null
                && _projectileTemplate != null
                && _projectiles.Count > 0
                && (_enemySpawner != null || _targetLayers.value != 0);
        }

        private void HandleStatsChanged(PlayerStatsSnapshot stats)
        {
            _attackCooldown = Mathf.Min(_attackCooldown, 1f / stats.AttacksPerSecond);
        }
    }
}
