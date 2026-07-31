using UnityEngine;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerHitFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerRunHealth _runHealth;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField, Min(0.01f)] private float _duration = 0.16f;
        [SerializeField, Min(1f)] private float _scaleMultiplier = 1.1f;
        [SerializeField] private Color _flashColor = new Color(1f, 0.35f, 0.35f, 1f);

        private Color _baseColor;
        private float _remaining;
        private Vector3 _baseLocalScale;

        private void Awake()
        {
            CaptureBasePresentation();
        }

        private void OnEnable()
        {
            if (_runHealth != null)
            {
                _runHealth.Damaged += Play;
            }
        }

        private void OnDisable()
        {
            if (_runHealth != null)
            {
                _runHealth.Damaged -= Play;
            }

            RestoreBasePresentation();
        }

        private void Update()
        {
            if (_remaining <= 0f || _spriteRenderer == null)
            {
                return;
            }

            _remaining = Mathf.Max(0f, _remaining - Time.unscaledDeltaTime);
            var normalizedRemaining = _remaining / _duration;
            _spriteRenderer.color = Color.Lerp(_baseColor, _flashColor, normalizedRemaining);
            _spriteRenderer.transform.localScale = _baseLocalScale * Mathf.Lerp(
                1f,
                _scaleMultiplier,
                normalizedRemaining);

            if (_remaining <= 0f)
            {
                RestoreBasePresentation();
            }
        }

        private void Play(float _)
        {
            CaptureBasePresentation();
            _remaining = _duration;
        }

        private void CaptureBasePresentation()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            _baseColor = _spriteRenderer.color;
            _baseLocalScale = _spriteRenderer.transform.localScale;
        }

        private void RestoreBasePresentation()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            _spriteRenderer.color = _baseColor;
            _spriteRenderer.transform.localScale = _baseLocalScale;
        }

        private void OnValidate()
        {
            _duration = Mathf.Max(0.01f, _duration);
            _scaleMultiplier = Mathf.Max(1f, _scaleMultiplier);
        }
    }
}
