using CatMouse.Game.Player;
using CatMouse.Game.UI;
using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class TestRunCompositionRoot : MonoBehaviour
    {
        private const float ForwardSpeed = 3.5f;
        private const float VerticalSpeed = 5f;
        private const float MinimumPlayerY = -3.4f;
        private const float MaximumPlayerY = 1.75f;
        private const float MetersPerWorldUnit = 10f;

        [SerializeField] private TestRunnerController _runner;
        [SerializeField] private TestRunDistanceHudView _distanceHud;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private Transform _scrollDriver;
        [SerializeField] private TestRunCameraDirector _cameraDirector;

        public void Initialize(
            TestRunnerController runner,
            TestRunDistanceHudView distanceHud,
            Transform cameraTarget,
            Transform scrollDriver)
        {
            Dispose();

            _runner = runner;
            _distanceHud = distanceHud;
            _cameraTarget = cameraTarget;
            _scrollDriver = scrollDriver;

            _runner.Initialize(
                ForwardSpeed,
                VerticalSpeed,
                MinimumPlayerY,
                MaximumPlayerY,
                MetersPerWorldUnit);

            _cameraTarget.SetParent(transform, true);
            _scrollDriver.SetParent(_runner.transform, false);
            _scrollDriver.localPosition = Vector3.zero;
            _cameraDirector.Initialize(_runner, _cameraTarget);

            _runner.DistanceChanged += HandleDistanceChanged;
            _distanceHud.SetDistance(_runner.DistanceMeters);
        }

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _runner.Initialize(
                ForwardSpeed,
                VerticalSpeed,
                MinimumPlayerY,
                MaximumPlayerY,
                MetersPerWorldUnit);

            _runner.DistanceChanged += HandleDistanceChanged;
            _distanceHud.SetDistance(_runner.DistanceMeters);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void HandleDistanceChanged(float distanceMeters)
        {
            _distanceHud.SetDistance(distanceMeters);
        }

        private void Dispose()
        {
            if (_runner != null)
            {
                _runner.DistanceChanged -= HandleDistanceChanged;
            }
        }
    }
}
