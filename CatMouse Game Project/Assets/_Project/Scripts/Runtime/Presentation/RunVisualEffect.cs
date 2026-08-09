using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class RunVisualEffect : MonoBehaviour
    {
        private const string CharacterSortingLayer = "Characters";

        [SerializeField] private SpriteRenderer _spriteRenderer;

        private float _duration;
        private float _remaining;
        private float _startScale;

        public bool IsAvailable => !gameObject.activeSelf;

        public void Spawn(Sprite sprite, Vector3 position, float scale, float duration, int sortingOrder)
        {
            EnsureRenderer();
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            _startScale = Mathf.Max(0.01f, scale);
            transform.localScale = Vector3.one * _startScale;
            _duration = Mathf.Max(0.01f, duration);
            _remaining = _duration;
            _spriteRenderer.sprite = sprite;
            _spriteRenderer.color = Color.white;
            _spriteRenderer.sortingOrder = sortingOrder;
            gameObject.SetActive(true);
        }

        private void Awake()
        {
            EnsureRenderer();
        }

        private void Update()
        {
            if (_remaining <= 0f)
            {
                return;
            }

            _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);
            float progress = 1f - (_remaining / _duration);
            transform.localScale = Vector3.one * Mathf.Lerp(_startScale, _startScale * 1.35f, progress);

            Color color = _spriteRenderer.color;
            color.a = 1f - progress;
            _spriteRenderer.color = color;

            if (_remaining <= 0f)
            {
                gameObject.SetActive(false);
            }
        }

        private void EnsureRenderer()
        {
            _spriteRenderer ??= GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _spriteRenderer.sortingLayerName = CharacterSortingLayer;
        }
    }
}
