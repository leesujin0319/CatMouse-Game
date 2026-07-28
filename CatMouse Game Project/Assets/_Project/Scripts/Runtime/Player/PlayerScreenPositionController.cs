using CatMouse.Game.Run;
using UnityEngine;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerScreenPositionController : MonoBehaviour
    {
        [SerializeField] private global::Player _player;
        [SerializeField] private PlayerRunStats _runStats;
        [SerializeField] private Camera _worldCamera;

        [Header("Screen Position")]
        [SerializeField, Range(0f, 1f)] private float _leftViewportX = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _centerViewportX = 0.5f;
        [SerializeField, Min(0.01f)] private float _baseForwardSpeed = 3.5f;
        [SerializeField, Min(0.01f)] private float _forwardSpeedForCenter = 7f;

        private RunItemDefinition _temporaryItem;
        private float _temporaryDuration;
        private float _temporaryRemaining;
        private float _temporaryStartViewportX;

        private void OnEnable()
        {
            if (_runStats != null)
            {
                _runStats.StatsChanged += HandleStatsChanged;
            }
        }

        private void Start()
        {
            ApplyStatBasedPosition();
        }

        private void Update()
        {
            if (!Application.isPlaying || _temporaryItem == null)
            {
                return;
            }

            _temporaryRemaining -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(_temporaryRemaining / _temporaryDuration);
            ApplyViewportPosition(Mathf.Lerp(_temporaryStartViewportX, _centerViewportX, progress));

            if (_temporaryRemaining <= 0f)
            {
                EndTemporarySpeedEffect();
            }
        }

        private void OnDisable()
        {
            if (_runStats != null)
            {
                _runStats.StatsChanged -= HandleStatsChanged;
            }

            EndTemporarySpeedEffect();
        }

        private void OnValidate()
        {
            _leftViewportX = Mathf.Clamp01(_leftViewportX);
            _centerViewportX = Mathf.Clamp(_centerViewportX, _leftViewportX, 1f);
            _baseForwardSpeed = Mathf.Max(0.01f, _baseForwardSpeed);
            _forwardSpeedForCenter = Mathf.Max(_baseForwardSpeed, _forwardSpeedForCenter);
        }

        public bool TryStartTemporarySpeedEffect(RunItemDefinition itemDefinition)
        {
            if (_player == null
                || _runStats == null
                || !_runStats.IsInitialized
                || itemDefinition == null
                || !itemDefinition.IsTemporary)
            {
                return false;
            }

            EndTemporarySpeedEffect();
            _temporaryStartViewportX = GetStatBasedViewportX();
            _temporaryItem = itemDefinition;
            _temporaryDuration = itemDefinition.TemporaryDuration;
            _temporaryRemaining = _temporaryDuration;
            if (!_runStats.TryApplyTemporary(itemDefinition))
            {
                _temporaryItem = null;
                _temporaryDuration = 0f;
                _temporaryRemaining = 0f;
                return false;
            }

            return true;
        }

        private void EndTemporarySpeedEffect()
        {
            if (_temporaryItem == null)
            {
                return;
            }

            RunItemDefinition itemDefinition = _temporaryItem;
            _temporaryItem = null;
            _temporaryDuration = 0f;
            _temporaryRemaining = 0f;
            _runStats?.RemoveTemporary(itemDefinition);
            ApplyStatBasedPosition();
        }

        private void HandleStatsChanged(PlayerStatsSnapshot _)
        {
            if (_temporaryItem == null)
            {
                ApplyStatBasedPosition();
            }
        }

        private void ApplyStatBasedPosition()
        {
            ApplyViewportPosition(GetStatBasedViewportX());
        }

        private float GetStatBasedViewportX()
        {
            float forwardSpeed = _runStats != null && _runStats.IsInitialized
                ? _runStats.Current.ForwardSpeed
                : _baseForwardSpeed;
            float normalizedSpeed = Mathf.InverseLerp(_baseForwardSpeed, _forwardSpeedForCenter, forwardSpeed);
            return Mathf.Lerp(_leftViewportX, _centerViewportX, normalizedSpeed);
        }

        private void ApplyViewportPosition(float viewportX)
        {
            if (_player == null || _worldCamera == null)
            {
                return;
            }

            float cameraDistance = Mathf.Abs(_worldCamera.transform.position.z - _player.transform.position.z);
            float worldX = _worldCamera.ViewportToWorldPoint(new Vector3(viewportX, 0.5f, cameraDistance)).x;
            _player.SetFixedHorizontalPosition(worldX);
        }
    }
}
