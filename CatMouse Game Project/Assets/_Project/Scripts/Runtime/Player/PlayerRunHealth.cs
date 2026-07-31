using System;
using UnityEngine;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRunHealth : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float _maximumHealth = 100f;
        [SerializeField, Min(0f)] private float _drainPerSecond = 1f;
        [SerializeField, Min(0f)] private float _damageInvulnerabilityDuration = 0.45f;

        private float _currentHealth;
        private float _damageInvulnerabilityRemaining;
        private bool _isDepleted;

        public event Action<float> Damaged;
        public event Action<float, float> HealthChanged;
        public event Action Depleted;

        public float CurrentHealth => _currentHealth;
        public float MaximumHealth => _maximumHealth;
        public float NormalizedHealth => _maximumHealth > 0f ? _currentHealth / _maximumHealth : 0f;
        public bool IsDamageInvulnerable => _damageInvulnerabilityRemaining > 0f;

        private void Awake()
        {
            _currentHealth = _maximumHealth;
            NotifyHealthChanged();
        }

        private void Update()
        {
            _damageInvulnerabilityRemaining = Mathf.Max(
                0f,
                _damageInvulnerabilityRemaining - Time.deltaTime);

            if (!Application.isPlaying || _isDepleted || _drainPerSecond <= 0f)
            {
                return;
            }

            Consume(_drainPerSecond * Time.deltaTime);
        }

        private void OnValidate()
        {
            _maximumHealth = Mathf.Max(1f, _maximumHealth);
            _drainPerSecond = Mathf.Max(0f, _drainPerSecond);
            _damageInvulnerabilityDuration = Mathf.Max(0f, _damageInvulnerabilityDuration);
            _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maximumHealth);
        }

        public void Restore(float amount)
        {
            if (amount <= 0f || _isDepleted)
            {
                return;
            }

            SetCurrentHealth(_currentHealth + amount);
        }

        public void TakeDamage(float amount)
        {
            TryTakeDamage(amount);
        }

        public bool TryTakeDamage(float amount)
        {
            if (amount <= 0f || _isDepleted || IsDamageInvulnerable)
            {
                return false;
            }

            _damageInvulnerabilityRemaining = _damageInvulnerabilityDuration;
            Consume(amount);
            Damaged?.Invoke(amount);
            return true;
        }

        private void Consume(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetCurrentHealth(_currentHealth - amount);
            if (_currentHealth > 0f || _isDepleted)
            {
                return;
            }

            _isDepleted = true;
            Depleted?.Invoke();
        }

        private void SetCurrentHealth(float value)
        {
            float clampedValue = Mathf.Clamp(value, 0f, _maximumHealth);
            if (Mathf.Approximately(_currentHealth, clampedValue))
            {
                return;
            }

            _currentHealth = clampedValue;
            NotifyHealthChanged();
        }

        private void NotifyHealthChanged()
        {
            HealthChanged?.Invoke(_currentHealth, _maximumHealth);
        }
    }
}
