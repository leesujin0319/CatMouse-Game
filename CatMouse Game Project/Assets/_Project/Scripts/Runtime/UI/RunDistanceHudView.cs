using CatMouse.Game.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class RunDistanceHudView : MonoBehaviour
    {
        [SerializeField] private RunProgressController _runProgress;
        [SerializeField] private Text _distanceLabel;

        private void OnEnable()
        {
            if (_runProgress != null)
            {
                _runProgress.DistanceChanged += SetDistance;
                SetDistance(_runProgress.DistanceMeters);
            }
        }

        private void OnDisable()
        {
            if (_runProgress != null)
            {
                _runProgress.DistanceChanged -= SetDistance;
            }
        }

        public void SetDistance(float distanceMeters)
        {
            if (_distanceLabel == null)
            {
                return;
            }

            int displayedMeters = Mathf.FloorToInt(Mathf.Max(0f, distanceMeters));
            _distanceLabel.text = $"{displayedMeters:0000} m";
        }
    }
}
