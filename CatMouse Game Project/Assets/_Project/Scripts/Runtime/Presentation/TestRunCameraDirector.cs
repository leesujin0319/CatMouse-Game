using CatMouse.Game.Player;
using CatMouse.Game.Run;
using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class TestRunCameraDirector : MonoBehaviour
    {
        private const float DefaultForwardOffset = 3f;
        private const float DefaultVerticalFollowSpeed = 8f;
        private const float DefaultUpperFollowOffset = 1.4f;
        private const float DefaultLowerFollowOffset = 1.8f;
        private const float DefaultMinimumCameraY = 0.5f;
        private const float DefaultMaximumCameraY = 1.5f;

        [Header("References")]
        [SerializeField] private TestRunnerController _runner;
        [SerializeField] private Transform _followTarget;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private RunVerticalBounds _verticalBounds;
        [SerializeField] private Camera _worldCamera;

        [Header("Framing")]
        [SerializeField] private float _forwardOffset = DefaultForwardOffset;
        [SerializeField] private bool _followHorizontalMovement = true;
        [SerializeField, Min(0f)] private float _verticalFollowSpeed = DefaultVerticalFollowSpeed;
        [SerializeField, Min(0f)] private float _upperFollowOffset = DefaultUpperFollowOffset;
        [SerializeField, Min(0f)] private float _lowerFollowOffset = DefaultLowerFollowOffset;
        [SerializeField] private float _minimumCameraY = DefaultMinimumCameraY;
        [SerializeField] private float _maximumCameraY = DefaultMaximumCameraY;

        public void Initialize(
            TestRunnerController runner,
            Transform cameraTarget,
            RunVerticalBounds verticalBounds,
            Camera worldCamera)
        {
            _runner = runner;
            _followTarget = runner != null ? runner.transform : null;
            _cameraTarget = cameraTarget;
            _verticalBounds = verticalBounds;
            _worldCamera = worldCamera;
            SnapToRunner();
        }

        private void Awake()
        {
            SnapToRunner();
        }

        private void LateUpdate()
        {
            Transform followTarget = GetFollowTarget();
            if (!Application.isPlaying || followTarget == null || _cameraTarget == null)
            {
                return;
            }

            var runnerPosition = followTarget.position;
            var targetY = ResolveTargetY(runnerPosition.y, _cameraTarget.position.y);
            var targetX = _followHorizontalMovement
                ? runnerPosition.x + _forwardOffset
                : _cameraTarget.position.x;
            var targetPosition = new Vector3(targetX, targetY, _cameraTarget.position.z);
            var followFactor = 1f - Mathf.Exp(-_verticalFollowSpeed * Time.deltaTime);

            _cameraTarget.position = new Vector3(
                targetPosition.x,
                Mathf.Lerp(_cameraTarget.position.y, targetPosition.y, followFactor),
                targetPosition.z);
        }

        private void OnValidate()
        {
            _verticalFollowSpeed = Mathf.Max(0f, _verticalFollowSpeed);
            _upperFollowOffset = Mathf.Max(0f, _upperFollowOffset);
            _lowerFollowOffset = Mathf.Max(0f, _lowerFollowOffset);

            if (_minimumCameraY > _maximumCameraY)
            {
                _maximumCameraY = _minimumCameraY;
            }
        }

        private void SnapToRunner()
        {
            Transform followTarget = GetFollowTarget();
            if (followTarget == null || _cameraTarget == null)
            {
                return;
            }

            var runnerPosition = followTarget.position;
            var targetY = ClampCameraY(_cameraTarget.position.y);
            var targetX = _followHorizontalMovement
                ? runnerPosition.x + _forwardOffset
                : _cameraTarget.position.x;
            _cameraTarget.position = new Vector3(targetX, targetY, _cameraTarget.position.z);
        }

        private Transform GetFollowTarget()
        {
            return _followTarget != null
                ? _followTarget
                : _runner != null
                    ? _runner.transform
                    : null;
        }

        private float ResolveTargetY(float runnerY, float currentCameraY)
        {
            var targetY = currentCameraY;
            var upperThreshold = currentCameraY + _upperFollowOffset;
            var lowerThreshold = currentCameraY - _lowerFollowOffset;

            if (runnerY > upperThreshold)
            {
                targetY = runnerY - _upperFollowOffset;
            }
            else if (runnerY < lowerThreshold)
            {
                targetY = runnerY + _lowerFollowOffset;
            }

            return ClampCameraY(targetY);
        }

        private float ClampCameraY(float targetY)
        {
            var minimumY = _minimumCameraY;
            var maximumY = _maximumCameraY;

            if (_verticalBounds != null &&
                _verticalBounds.TryGetCameraRange(_worldCamera, out var floorMinimumY, out var floorMaximumY))
            {
                minimumY = floorMinimumY;
                maximumY = floorMaximumY;
            }

            return Mathf.Clamp(targetY, minimumY, maximumY);
        }
    }
}
