using System.Collections.Generic;
using CatMouse.Game.Player;
using CatMouse.Game.Run;
using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [DisallowMultipleComponent]
    public sealed class PrototypeCheeseDropPool : MonoBehaviour
    {
        private const int DefaultPoolCapacity = 24;
        private const int DefaultDropCount = 3;
        private const float DefaultMinimumScatterSpeed = 1.2f;
        private const float DefaultMaximumScatterSpeed = 2.8f;
        private const float DefaultScatterDeceleration = 4f;
        private const float DefaultScatterArcDegrees = 80f;
        private const float DefaultLifetime = 8f;
        private const float DefaultDespawnLeftPadding = 1.25f;
        private const float DefaultPickupRadius = 0.6f;
        private const float ScatterAngleJitter = 0.12f;

        [Header("References")]
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private RunVerticalBounds _verticalBounds;
        [SerializeField] private RunProgressController _runProgress;
        [SerializeField] private PlayerRunHealth _runHealth;
        [SerializeField] private RunExperienceController _experience;
        [SerializeField] private Transform _cheeseRoot;
        [SerializeField] private PrototypeCheeseDrop _cheeseTemplate;
        private Transform _collector;

        [Header("Pool")]
        [SerializeField, Min(1)] private int _poolCapacity = DefaultPoolCapacity;
        [SerializeField, Min(1)] private int _dropCount = DefaultDropCount;

        [Header("Scatter")]
        [SerializeField, Min(0f)] private float _minimumScatterSpeed = DefaultMinimumScatterSpeed;
        [SerializeField, Min(0f)] private float _maximumScatterSpeed = DefaultMaximumScatterSpeed;
        [SerializeField, Min(0f)] private float _scatterDeceleration = DefaultScatterDeceleration;
        [SerializeField, Range(0f, 180f)] private float _scatterArcDegrees = DefaultScatterArcDegrees;
        [SerializeField, Min(0f)] private float _lifetime = DefaultLifetime;
        [SerializeField, Min(0f)] private float _despawnLeftPadding = DefaultDespawnLeftPadding;
        [SerializeField, Min(0f)] private float _pickupRadius = DefaultPickupRadius;

        [Header("Rewards")]
        [SerializeField, Min(0f)] private float _healthRestorePerCheese = 2f;
        [SerializeField, Min(0f)] private float _experiencePerCheese = 1f;

        private readonly List<PrototypeCheeseDrop> _pool = new();

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

            TickActiveCheese();
        }

        public void Scatter(Vector3 position, Vector2 scatterDirection)
        {
            if (!HasValidConfiguration())
            {
                return;
            }

            var direction = scatterDirection.sqrMagnitude > Mathf.Epsilon
                ? scatterDirection.normalized
                : Vector2.left;
            var baseAngle = Mathf.Atan2(direction.y, direction.x);
            var halfArcRadians = _scatterArcDegrees * 0.5f * Mathf.Deg2Rad;

            for (var index = 0; index < _dropCount; index++)
            {
                var cheese = GetAvailableCheese();
                if (cheese == null)
                {
                    return;
                }

                var arcPosition = _dropCount > 1
                    ? index / (_dropCount - 1f)
                    : 0.5f;
                var angle = baseAngle
                    + Mathf.Lerp(-halfArcRadians, halfArcRadians, arcPosition)
                    + Random.Range(-ScatterAngleJitter, ScatterAngleJitter);
                var velocityDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var speed = Random.Range(_minimumScatterSpeed, _maximumScatterSpeed);
                cheese.Spawn(position, velocityDirection * speed, _scatterDeceleration, _lifetime);
            }
        }

        public void SetCollector(Transform collector)
        {
            _collector = collector;
            _runHealth = collector != null ? collector.GetComponent<PlayerRunHealth>() : null;
            _experience = collector != null ? collector.GetComponent<RunExperienceController>() : null;
        }

        public void SetRunProgress(RunProgressController runProgress)
        {
            _runProgress = runProgress;
        }

        private void OnValidate()
        {
            _poolCapacity = Mathf.Max(1, _poolCapacity);
            _dropCount = Mathf.Max(1, _dropCount);
            _minimumScatterSpeed = Mathf.Max(0f, _minimumScatterSpeed);
            _maximumScatterSpeed = Mathf.Max(_minimumScatterSpeed, _maximumScatterSpeed);
            _scatterDeceleration = Mathf.Max(0f, _scatterDeceleration);
            _scatterArcDegrees = Mathf.Clamp(_scatterArcDegrees, 0f, 180f);
            _lifetime = Mathf.Max(0f, _lifetime);
            _despawnLeftPadding = Mathf.Max(0f, _despawnLeftPadding);
            _pickupRadius = Mathf.Max(0f, _pickupRadius);
            _healthRestorePerCheese = Mathf.Max(0f, _healthRestorePerCheese);
            _experiencePerCheese = Mathf.Max(0f, _experiencePerCheese);
        }

        private void WarmPool()
        {
            if (_cheeseTemplate == null || _cheeseRoot == null)
            {
                return;
            }

            _cheeseTemplate.gameObject.SetActive(false);
            _pool.Add(_cheeseTemplate);

            for (var index = 1; index < _poolCapacity; index++)
            {
                var cheese = Instantiate(_cheeseTemplate, _cheeseRoot);
                cheese.name = $"PrototypeCheese_{index:00}";
                cheese.gameObject.SetActive(false);
                _pool.Add(cheese);
            }
        }

        private void TickActiveCheese()
        {
            if (!_verticalBounds.TryGetMovementRange(out var minimumY, out var maximumY))
            {
                return;
            }

            var leftBoundary = _worldCamera.transform.position.x
                - (_worldCamera.orthographicSize * _worldCamera.aspect)
                - _despawnLeftPadding;
            var worldScrollSpeed = _runProgress != null
                ? _runProgress.CurrentForwardSpeed
                : 0f;

            for (var index = 0; index < _pool.Count; index++)
            {
                var cheese = _pool[index];
                if (cheese != null && !cheese.IsAvailable)
                {
                    cheese.Tick(
                        Time.deltaTime,
                        minimumY,
                        maximumY,
                        leftBoundary,
                        worldScrollSpeed);

                    if (_collector != null)
                    {
                        if (cheese.TryCollect(_collector.position, _pickupRadius))
                        {
                            _runHealth?.Restore(_healthRestorePerCheese);
                            _experience?.GainExperience(_experiencePerCheese);
                        }
                    }
                }
            }
        }

        private PrototypeCheeseDrop GetAvailableCheese()
        {
            for (var index = 0; index < _pool.Count; index++)
            {
                var cheese = _pool[index];
                if (cheese != null && cheese.IsAvailable)
                {
                    return cheese;
                }
            }

            return null;
        }

        private bool HasValidConfiguration()
        {
            return _worldCamera != null
                && _verticalBounds != null
                && _cheeseRoot != null
                && _cheeseTemplate != null
                && _pool.Count > 0;
        }
    }
}
