using System.Collections.Generic;
using CatMouse.Game.Presentation;
using UnityEngine;

namespace CatMouse.Game.Run
{
    [DisallowMultipleComponent]
    public sealed class RunCoinCollector : MonoBehaviour
    {
        private const int DefaultPoolCapacity = 8;
        private const float DefaultDespawnLeftPadding = 1.25f;
        private const float DefaultLifetime = 12f;
        private const float DefaultPickupRadius = 0.6f;
        private const float DefaultDropChance = 0.35f;
        private const int DefaultMinimumDropCount = 1;
        private const int DefaultMaximumDropCount = 2;
        private const float DefaultMinimumScatterSpeed = 0.9f;
        private const float DefaultMaximumScatterSpeed = 1.7f;
        private const float DefaultScatterDeceleration = 3f;
        private const float DefaultScatterArcDegrees = 70f;
        private const float ScatterAngleJitter = 0.1f;

        [Header("References")]
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private RunVerticalBounds _verticalBounds;
        [SerializeField] private RunProgressController _runProgress;
        [SerializeField] private Transform _collector;
        [SerializeField] private RunVisualEffectPool _visualEffectPool;
        [SerializeField] private Transform _coinRoot;
        [SerializeField] private RunCoinPickup _coinTemplate;

        [Header("Pool")]
        [SerializeField, Min(1)] private int _poolCapacity = DefaultPoolCapacity;
        [SerializeField, Min(0f)] private float _despawnLeftPadding = DefaultDespawnLeftPadding;
        [SerializeField, Min(0f)] private float _lifetime = DefaultLifetime;
        [SerializeField, Min(0f)] private float _pickupRadius = DefaultPickupRadius;

        [Header("Enemy Defeat Reward")]
        [SerializeField, Range(0f, 1f)] private float _dropChance = DefaultDropChance;
        [SerializeField, Min(1)] private int _minimumDropCount = DefaultMinimumDropCount;
        [SerializeField, Min(1)] private int _maximumDropCount = DefaultMaximumDropCount;
        [SerializeField, Min(0f)] private float _minimumScatterSpeed = DefaultMinimumScatterSpeed;
        [SerializeField, Min(0f)] private float _maximumScatterSpeed = DefaultMaximumScatterSpeed;
        [SerializeField, Min(0f)] private float _scatterDeceleration = DefaultScatterDeceleration;
        [SerializeField, Range(0f, 180f)] private float _scatterArcDegrees = DefaultScatterArcDegrees;

        private readonly List<RunCoinPickup> _pool = new();

        public int CollectedCoinCount { get; private set; }
        public event System.Action<int> CoinCountChanged;

        private void Awake()
        {
            WarmPool();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying || !HasValidConfiguration())
            {
                return;
            }

            TickActiveCoins();
        }

        private void OnValidate()
        {
            _poolCapacity = Mathf.Max(1, _poolCapacity);
            _despawnLeftPadding = Mathf.Max(0f, _despawnLeftPadding);
            _lifetime = Mathf.Max(0f, _lifetime);
            _pickupRadius = Mathf.Max(0f, _pickupRadius);
            _dropChance = Mathf.Clamp01(_dropChance);
            _minimumDropCount = Mathf.Max(1, _minimumDropCount);
            _maximumDropCount = Mathf.Max(_minimumDropCount, _maximumDropCount);
            _minimumScatterSpeed = Mathf.Max(0f, _minimumScatterSpeed);
            _maximumScatterSpeed = Mathf.Max(_minimumScatterSpeed, _maximumScatterSpeed);
            _scatterDeceleration = Mathf.Max(0f, _scatterDeceleration);
            _scatterArcDegrees = Mathf.Clamp(_scatterArcDegrees, 0f, 180f);
        }

        private void WarmPool()
        {
            if (_coinTemplate == null || _coinRoot == null)
            {
                return;
            }

            _coinTemplate.gameObject.SetActive(false);
            _pool.Add(_coinTemplate);

            for (int index = 1; index < _poolCapacity; index++)
            {
                RunCoinPickup coin = Instantiate(_coinTemplate, _coinRoot);
                coin.name = $"RunCoin_{index:00}";
                coin.gameObject.SetActive(false);
                _pool.Add(coin);
            }
        }

        private void TickActiveCoins()
        {
            if (!_verticalBounds.TryGetMovementRange(out float minimumY, out float maximumY))
            {
                return;
            }

            float leftBoundary = _worldCamera.transform.position.x
                - (_worldCamera.orthographicSize * _worldCamera.aspect)
                - _despawnLeftPadding;
            float worldScrollDelta = _runProgress.CurrentWorldScrollDelta;
            for (int index = 0; index < _pool.Count; index++)
            {
                RunCoinPickup coin = _pool[index];
                if (coin == null || coin.IsAvailable)
                {
                    continue;
                }

                coin.Tick(Time.deltaTime, minimumY, maximumY, leftBoundary, worldScrollDelta);
                if (coin.TryCollect(_collector.position, _pickupRadius))
                {
                    _visualEffectPool?.PlayPickup(coin.transform.position);
                    CollectedCoinCount++;
                    CoinCountChanged?.Invoke(CollectedCoinCount);
                }
            }
        }

        public bool TryDrop(Vector3 position, Vector2 scatterDirection)
        {
            if (!HasValidConfiguration() || Random.value > _dropChance)
            {
                return false;
            }

            var direction = scatterDirection.sqrMagnitude > Mathf.Epsilon
                ? scatterDirection.normalized
                : Vector2.left;
            var baseAngle = Mathf.Atan2(direction.y, direction.x);
            var halfArcRadians = _scatterArcDegrees * 0.5f * Mathf.Deg2Rad;
            var dropCount = Random.Range(_minimumDropCount, _maximumDropCount + 1);
            var hasDroppedCoin = false;

            for (int index = 0; index < dropCount; index++)
            {
                RunCoinPickup coin = GetAvailableCoin();
                if (coin == null)
                {
                    break;
                }

                var arcPosition = dropCount > 1
                    ? index / (dropCount - 1f)
                    : 0.5f;
                var angle = baseAngle
                    + Mathf.Lerp(-halfArcRadians, halfArcRadians, arcPosition)
                    + Random.Range(-ScatterAngleJitter, ScatterAngleJitter);
                var velocityDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var speed = Random.Range(_minimumScatterSpeed, _maximumScatterSpeed);
                coin.Spawn(position, velocityDirection * speed, _scatterDeceleration, _lifetime);
                hasDroppedCoin = true;
            }

            return hasDroppedCoin;
        }

        private RunCoinPickup GetAvailableCoin()
        {
            for (int index = 0; index < _pool.Count; index++)
            {
                RunCoinPickup coin = _pool[index];
                if (coin != null && coin.IsAvailable)
                {
                    return coin;
                }
            }

            return null;
        }

        private bool HasValidConfiguration()
        {
            return _worldCamera != null
                && _verticalBounds != null
                && _runProgress != null
                && _collector != null
                && _coinRoot != null
                && _coinTemplate != null
                && _pool.Count > 0;
        }
    }
}
