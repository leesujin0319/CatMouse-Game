using System;
using CatMouse.Game.Player;
using UnityEngine;

namespace CatMouse.Game.Run
{
    [DisallowMultipleComponent]
    public sealed class RunProgressController : MonoBehaviour
    {
        private const float DefaultForwardSpeed = 3.5f;
        private const float DefaultMetersPerWorldUnit = 10f;

        [Header("References")]
        [SerializeField] private PlayerRunStats _runStats;
        [SerializeField] private Transform _scrollDriver;

        [Header("Progress")]
        [SerializeField, Min(0f)] private float _fallbackForwardSpeed = DefaultForwardSpeed;
        [SerializeField, Min(0f)] private float _metersPerWorldUnit = DefaultMetersPerWorldUnit;

        public event Action<float> DistanceChanged;

        public float DistanceMeters { get; private set; }

        private void Start()
        {
            DistanceChanged?.Invoke(DistanceMeters);
        }

        private void Update()
        {
            if (!Application.isPlaying || _scrollDriver == null)
            {
                return;
            }

            float distanceWorldUnits = GetForwardSpeed() * Time.deltaTime;
            _scrollDriver.position += Vector3.right * distanceWorldUnits;
            DistanceMeters += distanceWorldUnits * _metersPerWorldUnit;
            DistanceChanged?.Invoke(DistanceMeters);
        }

        private void OnValidate()
        {
            _fallbackForwardSpeed = Mathf.Max(0f, _fallbackForwardSpeed);
            _metersPerWorldUnit = Mathf.Max(0f, _metersPerWorldUnit);
        }

        private float GetForwardSpeed()
        {
            return _runStats != null && _runStats.IsInitialized
                ? _runStats.Current.ForwardSpeed
                : _fallbackForwardSpeed;
        }
    }
}
