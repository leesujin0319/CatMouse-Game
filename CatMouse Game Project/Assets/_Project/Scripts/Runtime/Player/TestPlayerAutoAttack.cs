using System.Collections.Generic;
using CatMouse.Game.Enemy;
using UnityEngine;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class TestPlayerAutoAttack : MonoBehaviour
    {
        private const int DefaultPoolCapacity = 12;
        private const int DefaultDamage = 1;
        private const float DefaultAttackInterval = 0.55f;
        private const float DefaultAttackRange = 8f;
        private const float DefaultProjectileSpeed = 15f;
        private const float DefaultProjectileLifetime = 2f;

        [Header("References")]
        [SerializeField] private PrototypeEnemySpawner _enemySpawner;
        [SerializeField] private Transform _projectileRoot;
        [SerializeField] private PrototypeAcornProjectile _projectileTemplate;

        [Header("Attack")]
        [SerializeField] private Vector2 _attackOriginOffset = new(0.55f, 0.5f);
        [SerializeField, Min(0.05f)] private float _attackInterval = DefaultAttackInterval;
        [SerializeField, Min(0f)] private float _attackRange = DefaultAttackRange;
        [SerializeField, Min(0f)] private float _projectileSpeed = DefaultProjectileSpeed;
        [SerializeField, Min(0.1f)] private float _projectileLifetime = DefaultProjectileLifetime;
        [SerializeField, Min(1)] private int _damage = DefaultDamage;
        [SerializeField, Min(1)] private int _poolCapacity = DefaultPoolCapacity;

        private readonly List<PrototypeAcornProjectile> _pool = new();
        private float _attackCooldown;

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

            TickActiveProjectiles();
            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown > 0f)
            {
                return;
            }

            TryFireStraightProjectile();
        }

        private void OnValidate()
        {
            _attackInterval = Mathf.Max(0.05f, _attackInterval);
            _attackRange = Mathf.Max(0f, _attackRange);
            _projectileSpeed = Mathf.Max(0f, _projectileSpeed);
            _projectileLifetime = Mathf.Max(0.1f, _projectileLifetime);
            _damage = Mathf.Max(1, _damage);
            _poolCapacity = Mathf.Max(1, _poolCapacity);
        }

        private void WarmPool()
        {
            if (_projectileTemplate == null || _projectileRoot == null)
            {
                return;
            }

            _projectileTemplate.gameObject.SetActive(false);
            _pool.Add(_projectileTemplate);

            for (var index = 1; index < _poolCapacity; index++)
            {
                var projectile = Instantiate(_projectileTemplate, _projectileRoot);
                projectile.name = $"PrototypeAcorn_{index:00}";
                projectile.gameObject.SetActive(false);
                _pool.Add(projectile);
            }
        }

        private void TickActiveProjectiles()
        {
            for (var index = 0; index < _pool.Count; index++)
            {
                var projectile = _pool[index];
                if (projectile != null && !projectile.IsAvailable)
                {
                    projectile.Tick(Time.deltaTime);
                }
            }
        }

        private void TryFireStraightProjectile()
        {
            if (_attackRange <= 0f)
            {
                return;
            }

            var origin = transform.position + (Vector3)_attackOriginOffset;
            var projectile = GetAvailableProjectile();
            if (projectile == null)
            {
                return;
            }

            var lifetime = _projectileSpeed > Mathf.Epsilon
                ? Mathf.Min(_projectileLifetime, _attackRange / _projectileSpeed)
                : _projectileLifetime;
            projectile.Spawn(
                origin,
                _enemySpawner,
                _projectileSpeed,
                _damage,
                lifetime);
            _attackCooldown = _attackInterval;
        }

        private PrototypeAcornProjectile GetAvailableProjectile()
        {
            for (var index = 0; index < _pool.Count; index++)
            {
                var projectile = _pool[index];
                if (projectile != null && projectile.IsAvailable)
                {
                    return projectile;
                }
            }

            return null;
        }

        private bool HasValidConfiguration()
        {
            return _enemySpawner != null
                && _projectileRoot != null
                && _projectileTemplate != null
                && _pool.Count > 0;
        }
    }
}
