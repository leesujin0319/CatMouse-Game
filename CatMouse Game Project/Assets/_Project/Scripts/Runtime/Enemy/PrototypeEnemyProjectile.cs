using CatMouse.Game.Player;
using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [DisallowMultipleComponent]
    public sealed class PrototypeEnemyProjectile : MonoBehaviour
    {
        private const int SpriteSize = 10;
        private const float PixelsPerUnit = 12f;
        private const float ColliderRadius = 0.22f;
        private const float DefaultLifetime = 3f;
        private const string CharacterSortingLayer = "Characters";

        private static Sprite s_projectileSprite;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private CircleCollider2D _hitCollider;

        private Vector2 _velocity;
        private PlayerRunHealth _targetHealth;
        private Collider2D _targetCollider;
        private float _damage;
        private float _remainingLifetime;

        public bool IsAvailable => !gameObject.activeSelf;

        public void Spawn(
            Vector3 position,
            Vector2 direction,
            float speed,
            float damage,
            PlayerRunHealth targetHealth,
            Collider2D targetCollider)
        {
            transform.position = position;
            transform.rotation = Quaternion.identity;
            _velocity = direction.sqrMagnitude > Mathf.Epsilon
                ? direction.normalized * Mathf.Max(0f, speed)
                : Vector2.left * Mathf.Max(0f, speed);
            _damage = Mathf.Max(0f, damage);
            _targetHealth = targetHealth;
            _targetCollider = targetCollider;
            _remainingLifetime = DefaultLifetime;
            gameObject.SetActive(true);
        }

        public void Tick(float deltaTime)
        {
            if (IsAvailable)
            {
                return;
            }

            transform.position += (Vector3)(_velocity * deltaTime);
            transform.Rotate(0f, 0f, -540f * deltaTime);
            _remainingLifetime -= deltaTime;
            Physics2D.SyncTransforms();

            if (HasHitTarget())
            {
                _targetHealth?.TryTakeDamage(_damage);
                gameObject.SetActive(false);
                return;
            }

            if (_remainingLifetime <= 0f)
            {
                gameObject.SetActive(false);
            }
        }

        private void Awake()
        {
            EnsurePresentation();
        }

        private bool HasHitTarget()
        {
            return _hitCollider != null
                && _targetCollider != null
                && _hitCollider.Distance(_targetCollider).isOverlapped;
        }

        private void EnsurePresentation()
        {
            _spriteRenderer ??= GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _spriteRenderer.sprite = GetProjectileSprite();
            _spriteRenderer.sortingLayerName = CharacterSortingLayer;
            _spriteRenderer.sortingOrder = 4;

            _hitCollider ??= GetComponent<CircleCollider2D>();
            if (_hitCollider == null)
            {
                _hitCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            _hitCollider.isTrigger = true;
            _hitCollider.radius = ColliderRadius;

            Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody2D>();
            }

            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.gravityScale = 0f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private static Sprite GetProjectileSprite()
        {
            if (s_projectileSprite != null)
            {
                return s_projectileSprite;
            }

            Texture2D texture = new(SpriteSize, SpriteSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave,
            };

            int center = (SpriteSize - 1) / 2;
            for (int y = 0; y < SpriteSize; y++)
            {
                for (int x = 0; x < SpriteSize; x++)
                {
                    int distance = Mathf.Abs(x - center) + Mathf.Abs(y - center);
                    texture.SetPixel(
                        x,
                        y,
                        distance > 4
                            ? Color.clear
                            : distance > 2
                                ? new Color(0.58f, 0.18f, 0.13f, 1f)
                                : new Color(1f, 0.62f, 0.2f, 1f));
                }
            }

            texture.Apply();
            s_projectileSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, SpriteSize, SpriteSize),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit);
            s_projectileSprite.hideFlags = HideFlags.HideAndDontSave;
            return s_projectileSprite;
        }
    }
}
