using System;
using CatMouse.Game.Run;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class TestRunnerController : MonoBehaviour
    {
        private const float MinimumVisualHeight = 0.01f;
        private const float DefaultForwardSpeed = 3.5f;
        private const float DefaultVerticalSpeed = 5f;
        private const float DefaultMinimumY = -3.5f;
        private const float DefaultMaximumY = 2.5f;
        private const float DefaultMetersPerWorldUnit = 10f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _visualRenderer;
        [SerializeField, Min(MinimumVisualHeight)] private float _visualHeight = 1f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _forwardSpeed = DefaultForwardSpeed;
        [SerializeField, Min(0f)] private float _verticalSpeed = DefaultVerticalSpeed;
        [SerializeField] private float _minimumY = DefaultMinimumY;
        [SerializeField] private float _maximumY = DefaultMaximumY;
        [SerializeField] private RunVerticalBounds _verticalBounds;

        [Header("Distance")]
        [SerializeField, Min(0f)] private float _metersPerWorldUnit = DefaultMetersPerWorldUnit;

        private InputAction _holdToAscendAction;
        private float _startX;

        public event Action<float> DistanceChanged;

        public float DistanceMeters { get; private set; }

        public void Initialize(
            float forwardSpeed,
            float verticalSpeed,
            float minimumY,
            float maximumY,
            float metersPerWorldUnit,
            RunVerticalBounds verticalBounds)
        {
            _forwardSpeed = Mathf.Max(0f, forwardSpeed);
            _verticalSpeed = Mathf.Max(0f, verticalSpeed);
            _minimumY = Mathf.Min(minimumY, maximumY);
            _maximumY = Mathf.Max(minimumY, maximumY);
            _metersPerWorldUnit = Mathf.Max(0f, metersPerWorldUnit);
            _verticalBounds = verticalBounds;
        }

        private void Awake()
        {
            ApplyVisualHeight();
            _holdToAscendAction = new InputAction("HoldToAscend", InputActionType.Button);
            _holdToAscendAction.AddBinding("<Pointer>/press");
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                _holdToAscendAction.Enable();
            }
        }

        private void Start()
        {
            _startX = transform.position.x;
            PublishDistance();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var position = transform.position;
            var verticalDirection = _holdToAscendAction.IsPressed() ? 1f : -1f;
            var minimumY = _minimumY;
            var maximumY = _maximumY;

            if (_verticalBounds != null &&
                _verticalBounds.TryGetMovementRange(out var floorMinimumY, out var floorMaximumY))
            {
                minimumY = floorMinimumY;
                maximumY = floorMaximumY;
            }

            position.x += _forwardSpeed * Time.deltaTime;
            position.y = Mathf.Clamp(
                position.y + (verticalDirection * _verticalSpeed * Time.deltaTime),
                minimumY,
                maximumY);

            transform.position = position;
            PublishDistance();
        }

        private void OnDisable()
        {
            _holdToAscendAction.Disable();
        }

        private void OnDestroy()
        {
            _holdToAscendAction.Dispose();
        }

        private void OnValidate()
        {
            _visualHeight = Mathf.Max(MinimumVisualHeight, _visualHeight);
            _forwardSpeed = Mathf.Max(0f, _forwardSpeed);
            _verticalSpeed = Mathf.Max(0f, _verticalSpeed);
            _metersPerWorldUnit = Mathf.Max(0f, _metersPerWorldUnit);

            if (_minimumY > _maximumY)
            {
                _maximumY = _minimumY;
            }

            ApplyVisualHeight();
        }

        private void ApplyVisualHeight()
        {
            if (_visualRenderer == null || _visualRenderer.sprite == null)
            {
                return;
            }

            var sourceHeight = GetSpriteVisualHeight(_visualRenderer.sprite);

            if (sourceHeight <= 0f)
            {
                return;
            }

            var visualTransform = _visualRenderer.transform;
            var scale = visualTransform.localScale;
            var uniformScale = _visualHeight / sourceHeight;
            scale.x = uniformScale;
            scale.y = uniformScale;
            visualTransform.localScale = scale;
        }

        private static float GetSpriteVisualHeight(Sprite sprite)
        {
            var vertices = sprite.vertices;
            if (vertices == null || vertices.Length == 0)
            {
                return sprite.bounds.size.y;
            }

            var minimumY = vertices[0].y;
            var maximumY = vertices[0].y;

            for (var index = 1; index < vertices.Length; index++)
            {
                minimumY = Mathf.Min(minimumY, vertices[index].y);
                maximumY = Mathf.Max(maximumY, vertices[index].y);
            }

            return maximumY - minimumY;
        }

        private void PublishDistance()
        {
            DistanceMeters = Mathf.Max(0f, transform.position.x - _startX) * _metersPerWorldUnit;
            DistanceChanged?.Invoke(DistanceMeters);
        }
    }
}
