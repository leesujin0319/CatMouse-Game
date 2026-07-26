using UnityEngine;

namespace CatMouse.Game.Run
{
    [DisallowMultipleComponent]
    public sealed class RunVerticalBounds : MonoBehaviour
    {
        private const float DefaultTopInset = 1f;
        private const float DefaultCameraTopReveal = 2f;

        [SerializeField] private SpriteRenderer _floorRenderer;
        [SerializeField, Min(0f)] private float _bottomInset;
        [SerializeField, Min(0f)] private float _topInset = DefaultTopInset;

        [Header("Camera Reveal")]
        [SerializeField, Min(0f)] private float _cameraBottomReveal;
        [SerializeField, Min(0f)] private float _cameraTopReveal = DefaultCameraTopReveal;

        public bool TryGetMovementRange(out float minimumY, out float maximumY)
        {
            if (_floorRenderer == null)
            {
                minimumY = 0f;
                maximumY = 0f;
                return false;
            }

            var bounds = _floorRenderer.bounds;
            minimumY = bounds.min.y + _bottomInset;
            maximumY = bounds.max.y - _topInset;

            if (minimumY <= maximumY)
            {
                return true;
            }

            var midpoint = (minimumY + maximumY) * 0.5f;
            minimumY = midpoint;
            maximumY = midpoint;
            return true;
        }

        public bool TryGetCameraRange(Camera worldCamera, out float minimumY, out float maximumY)
        {
            if (worldCamera == null || !worldCamera.orthographic)
            {
                minimumY = 0f;
                maximumY = 0f;
                return false;
            }

            if (_floorRenderer == null)
            {
                minimumY = 0f;
                maximumY = 0f;
                return false;
            }

            var bounds = _floorRenderer.bounds;
            minimumY = bounds.min.y - _cameraBottomReveal + worldCamera.orthographicSize;
            maximumY = bounds.max.y + _cameraTopReveal - worldCamera.orthographicSize;

            if (minimumY <= maximumY)
            {
                return true;
            }

            var midpoint = (minimumY + maximumY) * 0.5f;
            minimumY = midpoint;
            maximumY = midpoint;
            return true;
        }

        private void OnValidate()
        {
            _bottomInset = Mathf.Max(0f, _bottomInset);
            _topInset = Mathf.Max(0f, _topInset);
            _cameraBottomReveal = Mathf.Max(0f, _cameraBottomReveal);
            _cameraTopReveal = Mathf.Max(0f, _cameraTopReveal);
        }
    }
}
