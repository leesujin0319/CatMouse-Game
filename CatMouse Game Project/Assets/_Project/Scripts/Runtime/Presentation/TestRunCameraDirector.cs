using CatMouse.Game.Player;
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
        private const float DefaultMinimumCameraY = -1.6f;
        private const float DefaultMaximumCameraY = 0.35f;

        [Header("References")]
        [SerializeField] private TestRunnerController _runner;
        [SerializeField] private Transform _cameraTarget;

        [Header("Framing")]
        [SerializeField] private float _forwardOffset = DefaultForwardOffset;
        [SerializeField, Min(0f)] private float _verticalFollowSpeed = DefaultVerticalFollowSpeed;
        [SerializeField, Min(0f)] private float _upperFollowOffset = DefaultUpperFollowOffset;
        [SerializeField, Min(0f)] private float _lowerFollowOffset = DefaultLowerFollowOffset;
        [SerializeField] private float _minimumCameraY = DefaultMinimumCameraY;
        [SerializeField] private float _maximumCameraY = DefaultMaximumCameraY;

        public void Initialize(TestRunnerController runner, Transform cameraTarget)
        {
            _runner = runner;
            _cameraTarget = cameraTarget;
            SnapToRunner();
        }

        private void Awake()
        {
            SnapToRunner();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying || _runner == null || _cameraTarget == null)
            {
                return;
            }

            var runnerPosition = _runner.transform.position;
            var targetY = ResolveTargetY(runnerPosition.y, _cameraTarget.position.y);
            var targetPosition = new Vector3(runnerPosition.x + _forwardOffset, targetY, _cameraTarget.position.z);
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
            if (_runner == null || _cameraTarget == null)
            {
                return;
            }

            var runnerPosition = _runner.transform.position;
            var targetY = Mathf.Clamp(_cameraTarget.position.y, _minimumCameraY, _maximumCameraY);
            _cameraTarget.position = new Vector3(runnerPosition.x + _forwardOffset, targetY, _cameraTarget.position.z);
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

            return Mathf.Clamp(targetY, _minimumCameraY, _maximumCameraY);
        }
    }
}
