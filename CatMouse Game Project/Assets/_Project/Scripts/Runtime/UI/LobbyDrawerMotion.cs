using UnityEngine;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyDrawerMotion : MonoBehaviour
    {
        private const float Duration = 0.18f;
        private static readonly Vector2 HiddenOffset = new(56f, 0f);

        private RectTransform _rectTransform;
        private Vector2 _shownPosition;
        private float _elapsed;
        private bool _hasShownOnce;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _shownPosition = _rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            if (!_hasShownOnce)
            {
                _hasShownOnce = true;
                return;
            }

            _elapsed = 0f;
            _rectTransform.anchoredPosition = _shownPosition + HiddenOffset;
        }

        private void Update()
        {
            if (_elapsed >= Duration)
            {
                return;
            }

            _elapsed = Mathf.Min(_elapsed + Time.unscaledDeltaTime, Duration);
            float progress = Mathf.SmoothStep(0f, 1f, _elapsed / Duration);
            _rectTransform.anchoredPosition = Vector2.Lerp(_shownPosition + HiddenOffset, _shownPosition, progress);
        }
    }
}
