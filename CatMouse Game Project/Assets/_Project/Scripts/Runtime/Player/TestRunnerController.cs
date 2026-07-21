using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class TestRunnerController : MonoBehaviour
    {
        private const float DefaultForwardSpeed = 3.5f;
        private const float DefaultVerticalSpeed = 5f;
        private const float DefaultMetersPerWorldUnit = 10f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _forwardSpeed = DefaultForwardSpeed;
        [SerializeField, Min(0f)] private float _verticalSpeed = DefaultVerticalSpeed;
        [SerializeField] private float _minimumY = -3.4f;
        [SerializeField] private float _maximumY = 1.75f;

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
            float metersPerWorldUnit)
        {
            _forwardSpeed = Mathf.Max(0f, forwardSpeed);
            _verticalSpeed = Mathf.Max(0f, verticalSpeed);
            _minimumY = Mathf.Min(minimumY, maximumY);
            _maximumY = Mathf.Max(minimumY, maximumY);
            _metersPerWorldUnit = Mathf.Max(0f, metersPerWorldUnit);
        }

        private void Awake()
        {
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

            position.x += _forwardSpeed * Time.deltaTime;
            position.y = Mathf.Clamp(
                position.y + (verticalDirection * _verticalSpeed * Time.deltaTime),
                _minimumY,
                _maximumY);

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
            _forwardSpeed = Mathf.Max(0f, _forwardSpeed);
            _verticalSpeed = Mathf.Max(0f, _verticalSpeed);
            _metersPerWorldUnit = Mathf.Max(0f, _metersPerWorldUnit);

            if (_minimumY > _maximumY)
            {
                _maximumY = _minimumY;
            }
        }

        private void PublishDistance()
        {
            DistanceMeters = Mathf.Max(0f, transform.position.x - _startX) * _metersPerWorldUnit;
            DistanceChanged?.Invoke(DistanceMeters);
        }
    }
}
