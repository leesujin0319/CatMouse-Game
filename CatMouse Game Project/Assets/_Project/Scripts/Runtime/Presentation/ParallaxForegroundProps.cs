using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ParallaxForegroundProps : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _scrollDriver;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private SpriteRenderer[] _props;
        [SerializeField] private Sprite[] _variants;

        [Header("Parallax")]
        [SerializeField, Range(0f, 1f)] private float _scrollFactor = 0.85f;
        [SerializeField, Min(0.1f)] private float _minimumGap = 5f;
        [SerializeField, Min(0.1f)] private float _maximumGap = 8f;

        [Header("Appearance")]
        [SerializeField] private Vector2 _verticalRange = new(-4.1f, -3.25f);
        [SerializeField] private Vector2 _scaleRange = new(0.5f, 0.72f);
        [SerializeField] private int _sortingOrder = -25;
        [SerializeField] private int _seed = 20260716;

        private System.Random _random;
        private float _lastScrollX;

        private void Awake()
        {
            EnsureRandom();
            _lastScrollX = _scrollDriver != null ? _scrollDriver.position.x : 0f;
        }

        private void LateUpdate()
        {
            ScrollWithDriver();
            RecycleProps();
        }

        public void Configure(
            Transform scrollDriver,
            Camera worldCamera,
            SpriteRenderer[] props,
            Sprite[] variants,
            float scrollFactor)
        {
            _scrollDriver = scrollDriver;
            _worldCamera = worldCamera;
            _props = props;
            _variants = variants;
            _scrollFactor = Mathf.Clamp01(scrollFactor);
            _lastScrollX = _scrollDriver != null ? _scrollDriver.position.x : 0f;
            _random = new System.Random(_seed);
        }

        public void LayoutPropsAroundCamera()
        {
            if (!HasValidConfiguration())
            {
                return;
            }

            EnsureRandom();

            var cursorX = _worldCamera.transform.position.x
                - GetHalfViewWidth()
                - (_maximumGap * 2f);

            for (var index = 0; index < _props.Length; index++)
            {
                var prop = _props[index];
                if (prop == null)
                {
                    continue;
                }

                var position = prop.transform.position;
                position.x = cursorX;
                prop.transform.position = position;
                ApplyRandomAppearance(prop);
                cursorX = prop.bounds.max.x + GetRandomRange(_minimumGap, _maximumGap);
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

            for (var index = 0; index < _props.Length; index++)
            {
                var prop = _props[index];
                if (prop != null)
                {
                    prop.transform.position += Vector3.left * (scrollDelta * _scrollFactor);
                }
            }

            _lastScrollX = scrollX;
        }

        private void RecycleProps()
        {
            if (!HasValidConfiguration())
            {
                return;
            }

            var recycleThreshold = _worldCamera.transform.position.x - GetHalfViewWidth() - _maximumGap;

            for (var index = 0; index < _props.Length; index++)
            {
                var prop = _props[index];
                if (prop == null || prop.bounds.max.x >= recycleThreshold)
                {
                    continue;
                }

                var position = prop.transform.position;
                position.x = GetRightmostPropEdge() + GetRandomRange(_minimumGap, _maximumGap);
                prop.transform.position = position;
                ApplyRandomAppearance(prop);
            }
        }

        private void ApplyRandomAppearance(SpriteRenderer prop)
        {
            prop.sprite = _variants[_random.Next(_variants.Length)];
            prop.sortingOrder = _sortingOrder;
            prop.flipX = _random.NextDouble() >= 0.5d;

            var scale = GetRandomRange(_scaleRange.x, _scaleRange.y);
            prop.transform.localScale = new Vector3(scale, scale, 1f);

            var position = prop.transform.position;
            position.y = GetRandomRange(_verticalRange.x, _verticalRange.y);
            prop.transform.position = position;
        }

        private float GetHalfViewWidth()
        {
            return _worldCamera.orthographicSize * _worldCamera.aspect;
        }

        private float GetRightmostPropEdge()
        {
            var rightmostEdge = float.MinValue;

            for (var index = 0; index < _props.Length; index++)
            {
                var prop = _props[index];
                if (prop != null)
                {
                    rightmostEdge = Mathf.Max(rightmostEdge, prop.bounds.max.x);
                }
            }

            return rightmostEdge;
        }

        private float GetRandomRange(float minimum, float maximum)
        {
            return minimum + ((float)_random.NextDouble() * (maximum - minimum));
        }

        private bool HasValidConfiguration()
        {
            return _scrollDriver != null
                && _worldCamera != null
                && _props != null
                && _props.Length > 0
                && _variants != null
                && _variants.Length > 0;
        }

        private void EnsureRandom()
        {
            _random ??= new System.Random(_seed);
        }
    }
}
