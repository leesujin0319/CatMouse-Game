using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public sealed class CharacterSpriteCollider2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private CapsuleCollider2D _collider;
        [SerializeField, Range(0.1f, 1f)] private float _widthRatio = 0.55f;
        [SerializeField, Range(0.1f, 1f)] private float _heightRatio = 0.72f;

        public void Configure(SpriteRenderer spriteRenderer, bool isTrigger)
        {
            _spriteRenderer = spriteRenderer;
            _collider ??= GetComponent<CapsuleCollider2D>();
            _collider.isTrigger = isTrigger;
            Refresh();
        }

        public void Refresh()
        {
            _spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
            _collider ??= GetComponent<CapsuleCollider2D>();
            if (_spriteRenderer == null || _collider == null || _spriteRenderer.sprite == null)
            {
                return;
            }

            var bounds = _spriteRenderer.sprite.bounds;
            var minimum = transform.InverseTransformPoint(
                _spriteRenderer.transform.TransformPoint(bounds.min));
            var maximum = transform.InverseTransformPoint(
                _spriteRenderer.transform.TransformPoint(bounds.max));
            var size = new Vector2(
                Mathf.Abs(maximum.x - minimum.x) * _widthRatio,
                Mathf.Abs(maximum.y - minimum.y) * _heightRatio);

            _collider.direction = CapsuleDirection2D.Vertical;
            _collider.size = size;
            _collider.offset = new Vector2(
                (minimum.x + maximum.x) * 0.5f,
                minimum.y + (size.y * 0.5f));
        }

        private void Awake()
        {
            Refresh();
        }
    }
}
