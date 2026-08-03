using System.Collections.Generic;
using UnityEngine;

namespace CatMouse.Game.Run
{
    [DisallowMultipleComponent]
    public sealed class RunCoinCollector : MonoBehaviour
    {
        private const int DefaultPoolCapacity = 8;
        private const float DefaultFirstSpawnDistanceMeters = 20f;
        private const float DefaultSpawnIntervalMeters = 25f;
        private const float DefaultSpawnRightPadding = 1.25f;
        private const float DefaultDespawnLeftPadding = 1.25f;
        private const float DefaultLifetime = 12f;
        private const float DefaultPickupRadius = 0.6f;

        [Header("References")]
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private RunVerticalBounds _verticalBounds;
        [SerializeField] private RunProgressController _runProgress;
        [SerializeField] private Transform _collector;
        [SerializeField] private Transform _coinRoot;
        [SerializeField] private RunCoinPickup _coinTemplate;

        [Header("Spawn")]
        [SerializeField, Min(1)] private int _poolCapacity = DefaultPoolCapacity;
        [SerializeField, Min(0f)] private float _firstSpawnDistanceMeters = DefaultFirstSpawnDistanceMeters;
        [SerializeField, Min(0.01f)] private float _spawnIntervalMeters = DefaultSpawnIntervalMeters;
        [SerializeField, Min(0f)] private float _spawnRightPadding = DefaultSpawnRightPadding;
        [SerializeField, Min(0f)] private float _despawnLeftPadding = DefaultDespawnLeftPadding;
        [SerializeField, Min(0f)] private float _lifetime = DefaultLifetime;
        [SerializeField, Min(0f)] private float _pickupRadius = DefaultPickupRadius;

        private readonly List<RunCoinPickup> _pool = new();
        private float _nextSpawnDistanceMeters;

        public int CollectedCoinCount { get; private set; }
        public event System.Action<int> CoinCountChanged;

        private void Awake()
        {
            WarmPool();
            _nextSpawnDistanceMeters = _firstSpawnDistanceMeters;
        }

        private void Update()
        {
            if (!Application.isPlaying || !HasValidConfiguration())
            {
                return;
            }

            TickActiveCoins();
            TrySpawnCoin();
        }

        private void OnValidate()
        {
            _poolCapacity = Mathf.Max(1, _poolCapacity);
            _firstSpawnDistanceMeters = Mathf.Max(0f, _firstSpawnDistanceMeters);
            _spawnIntervalMeters = Mathf.Max(0.01f, _spawnIntervalMeters);
            _spawnRightPadding = Mathf.Max(0f, _spawnRightPadding);
            _despawnLeftPadding = Mathf.Max(0f, _despawnLeftPadding);
            _lifetime = Mathf.Max(0f, _lifetime);
            _pickupRadius = Mathf.Max(0f, _pickupRadius);
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
            float leftBoundary = _worldCamera.transform.position.x
                - (_worldCamera.orthographicSize * _worldCamera.aspect)
                - _despawnLeftPadding;
            float worldScrollSpeed = _runProgress != null
                ? _runProgress.CurrentForwardSpeed
                : 0f;

            for (int index = 0; index < _pool.Count; index++)
            {
                RunCoinPickup coin = _pool[index];
                if (coin == null || coin.IsAvailable)
                {
                    continue;
                }

                coin.Tick(Time.deltaTime, leftBoundary, worldScrollSpeed);
                if (coin.TryCollect(_collector.position, _pickupRadius))
                {
                    CollectedCoinCount++;
                    CoinCountChanged?.Invoke(CollectedCoinCount);
                }
            }
        }

        private void TrySpawnCoin()
        {
            if (_runProgress.DistanceMeters < _nextSpawnDistanceMeters)
            {
                return;
            }

            RunCoinPickup coin = GetAvailableCoin();
            if (coin == null || !_verticalBounds.TryGetMovementRange(out float minimumY, out float maximumY))
            {
                return;
            }

            float halfViewWidth = _worldCamera.orthographicSize * _worldCamera.aspect;
            Vector3 spawnPosition = new Vector3(
                _worldCamera.transform.position.x + halfViewWidth + _spawnRightPadding,
                Random.Range(minimumY, maximumY),
                0f);
            coin.Spawn(spawnPosition, _lifetime);
            _nextSpawnDistanceMeters += _spawnIntervalMeters;
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
