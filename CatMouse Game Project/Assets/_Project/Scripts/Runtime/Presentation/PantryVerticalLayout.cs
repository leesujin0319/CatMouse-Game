using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class PantryVerticalLayout : MonoBehaviour
    {
        private const float MinimumDimension = 0.01f;
        private const float HorizontalTileOverlap = 0.08f;
        private const float DefaultTileWidth = 16f;
        private const float DefaultFloorCenterY = 0f;
        private const float DefaultFloorHeight = 7f;
        private const float DefaultFarBackgroundHeight = 3f;
        private const float DefaultFarBackgroundOverlap = 0.08f;
        private const float DefaultForegroundHeight = 0.5f;
        private const float DefaultForegroundOverlap = 0.5f;

        [Header("References")]
        [SerializeField] private Transform _farBackgroundLayer;
        [SerializeField] private SpriteRenderer _farBackgroundReference;
        [SerializeField] private Transform _floorLayer;
        [SerializeField] private SpriteRenderer _floorReference;
        [SerializeField] private Transform _foregroundLayer;
        [SerializeField] private SpriteRenderer _foregroundReference;

        [Header("Horizontal Tiling")]
        [SerializeField, Min(MinimumDimension)] private float _tileWidth = DefaultTileWidth;

        [Header("Floor")]
        [SerializeField] private float _floorCenterY = DefaultFloorCenterY;
        [SerializeField, Min(MinimumDimension)] private float _floorHeight = DefaultFloorHeight;

        [Header("Far Background")]
        [SerializeField, Min(MinimumDimension)] private float _farBackgroundHeight = DefaultFarBackgroundHeight;
        [SerializeField, Min(0f)] private float _farBackgroundOverlap = DefaultFarBackgroundOverlap;

        [Header("Foreground")]
        [SerializeField, Min(MinimumDimension)] private float _foregroundHeight = DefaultForegroundHeight;
        [SerializeField, Min(0f)] private float _foregroundOverlap = DefaultForegroundOverlap;

        public void ApplyLayout()
        {
            SetSize(_floorLayer, _floorReference, _tileWidth, _floorHeight);
            SetLocalY(_floorLayer, _floorCenterY);

            var floorHalfHeight = _floorHeight * 0.5f;
            var floorTop = _floorCenterY + floorHalfHeight;
            var floorBottom = _floorCenterY - floorHalfHeight;

            SetSize(
                _farBackgroundLayer,
                _farBackgroundReference,
                _tileWidth,
                _farBackgroundHeight);
            SetLocalY(
                _farBackgroundLayer,
                floorTop - _farBackgroundOverlap + (_farBackgroundHeight * 0.5f));

            SetSize(
                _foregroundLayer,
                _foregroundReference,
                _tileWidth,
                _foregroundHeight);
            SetLocalY(
                _foregroundLayer,
                floorBottom + _foregroundOverlap - (_foregroundHeight * 0.5f));
        }

        private void Awake()
        {
            ApplyLayout();
        }

        private void OnValidate()
        {
            _tileWidth = Mathf.Max(MinimumDimension, _tileWidth);
            _floorHeight = Mathf.Max(MinimumDimension, _floorHeight);
            _farBackgroundHeight = Mathf.Max(MinimumDimension, _farBackgroundHeight);
            _farBackgroundOverlap = Mathf.Max(0f, _farBackgroundOverlap);
            _foregroundHeight = Mathf.Max(MinimumDimension, _foregroundHeight);
            _foregroundOverlap = Mathf.Max(0f, _foregroundOverlap);

            ApplyLayout();
        }

        private static void SetSize(
            Transform layer,
            SpriteRenderer referenceRenderer,
            float targetWidth,
            float targetHeight)
        {
            if (layer == null || referenceRenderer == null || referenceRenderer.sprite == null)
            {
                return;
            }

            var sourceSize = referenceRenderer.sprite.bounds.size;

            if (sourceSize.x <= 0f || sourceSize.y <= 0f)
            {
                return;
            }

            var scale = layer.localScale;
            scale.x = targetWidth / sourceSize.x;
            scale.y = targetHeight / sourceSize.y;
            layer.localScale = scale;

            LayoutTiles(layer, targetWidth, scale.x);
        }

        private static void LayoutTiles(
            Transform layer,
            float tileWidth,
            float horizontalScale)
        {
            if (layer == null || layer.childCount == 0 || horizontalScale <= 0f)
            {
                return;
            }

            var tileStride = tileWidth - HorizontalTileOverlap;
            var localStride = tileStride / horizontalScale;
            var centerIndex = (layer.childCount - 1) * 0.5f;

            for (var index = 0; index < layer.childCount; index++)
            {
                var tile = layer.GetChild(index);
                var position = tile.localPosition;
                position.x = (index - centerIndex) * localStride;
                tile.localPosition = position;
            }
        }

        private static void SetLocalY(Transform layer, float localY)
        {
            if (layer == null)
            {
                return;
            }

            var position = layer.localPosition;
            position.y = localY;
            layer.localPosition = position;
        }
    }
}
