using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class EndlessPantryBackground : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _scrollDriver;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private SpriteRenderer[] _tiles;

        [Header("Tiling")]
        [SerializeField, Min(0f)] private float _horizontalOverlap = 0.03f;
        [SerializeField, Range(0f, 1f)] private float _scrollFactor = 1f;
        [SerializeField] private bool _isCameraRelative;

        private float _tileWidth;
        private float _tileStride;
        private float _lastScrollX;

        private void Awake()
        {
            RefreshDimensions();
            _lastScrollX = _scrollDriver != null ? _scrollDriver.position.x : 0f;
            RecycleTiles();
        }

        private void LateUpdate()
        {
            ScrollWithDriver();
            RecycleTiles();
        }

        private void OnValidate()
        {
            RefreshDimensions();
        }

        public void Configure(
            Transform scrollDriver,
            Camera worldCamera,
            SpriteRenderer[] tiles,
            float scrollFactor)
        {
            Configure(scrollDriver, worldCamera, tiles, scrollFactor, false);
        }

        public void Configure(
            Transform scrollDriver,
            Camera worldCamera,
            SpriteRenderer[] tiles,
            float scrollFactor,
            bool isCameraRelative)
        {
            _scrollDriver = scrollDriver;
            _worldCamera = worldCamera;
            _tiles = tiles;
            _scrollFactor = Mathf.Clamp01(scrollFactor);
            _isCameraRelative = isCameraRelative;
            _lastScrollX = _scrollDriver != null ? _scrollDriver.position.x : 0f;

            RefreshDimensions();
        }

        public void LayoutTilesAroundCamera()
        {
            RefreshDimensions();

            if (!HasValidConfiguration())
            {
                return;
            }

            var centerIndex = (_tiles.Length - 1) * 0.5f;
            var cameraX = _worldCamera.transform.position.x;

            for (var index = 0; index < _tiles.Length; index++)
            {
                var tile = _tiles[index];
                if (tile == null)
                {
                    continue;
                }

                var tilePosition = tile.transform.position;
                tilePosition.x = cameraX + ((index - centerIndex) * _tileStride);
                tile.transform.position = tilePosition;
            }

            _lastScrollX = _scrollDriver.position.x;
        }

        private void ScrollWithDriver()
        {
            if (!HasValidConfiguration())
            {
                return;
            }

            var scrollX = _scrollDriver.position.x;
            var scrollDelta = scrollX - _lastScrollX;

            if (Mathf.Approximately(scrollDelta, 0f))
            {
                return;
            }

            for (var index = 0; index < _tiles.Length; index++)
            {
                var tile = _tiles[index];
                if (tile != null)
                {
                var scrollDistance = _isCameraRelative
                    ? scrollDelta * (1f - _scrollFactor)
                    : -scrollDelta * _scrollFactor;

                tile.transform.position += Vector3.right * scrollDistance;
            }
            }

            _lastScrollX = scrollX;
        }

        private void RecycleTiles()
        {
            if (!HasValidConfiguration())
            {
                return;
            }

            var recycleThreshold = _worldCamera.transform.position.x - GetHalfViewWidth() - _tileWidth;

            for (var index = 0; index < _tiles.Length; index++)
            {
                var tile = _tiles[index];
                if (tile == null || tile.bounds.max.x >= recycleThreshold)
                {
                    continue;
                }

                var tilePosition = tile.transform.position;
                tilePosition.x = GetRightmostTileX() + _tileStride;
                tile.transform.position = tilePosition;
            }
        }

        private float GetHalfViewWidth()
        {
            return _worldCamera.orthographicSize * _worldCamera.aspect;
        }

        private float GetRightmostTileX()
        {
            var rightmostX = float.MinValue;

            for (var index = 0; index < _tiles.Length; index++)
            {
                var tile = _tiles[index];
                if (tile != null)
                {
                    rightmostX = Mathf.Max(rightmostX, tile.transform.position.x);
                }
            }

            return rightmostX;
        }

        private bool HasValidConfiguration()
        {
            return _scrollDriver != null
                && _worldCamera != null
                && _tiles != null
                && _tiles.Length >= 3
                && _tileWidth > 0f
                && _tileStride > 0f;
        }

        private void RefreshDimensions()
        {
            if (_tiles == null || _tiles.Length == 0 || _tiles[0] == null)
            {
                _tileWidth = 0f;
                _tileStride = 0f;
                return;
            }

            _tileWidth = _tiles[0].bounds.size.x;
            _tileStride = Mathf.Max(0.01f, _tileWidth - _horizontalOverlap);
        }
    }
}
