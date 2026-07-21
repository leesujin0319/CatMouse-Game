using UnityEngine;
using UnityEngine.UI;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class TestRunDistanceHudView : MonoBehaviour
    {
        [SerializeField] private Text _distanceLabel;

        public void Initialize(Text distanceLabel)
        {
            _distanceLabel = distanceLabel;
            SetDistance(0f);
        }

        public void SetDistance(float distanceMeters)
        {
            if (_distanceLabel == null)
            {
                return;
            }

            var displayedMeters = Mathf.FloorToInt(Mathf.Max(0f, distanceMeters));
            _distanceLabel.text = $"{displayedMeters:0000} m";
        }
    }
}
