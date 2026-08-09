using UnityEngine;

namespace CatMouse.Game.Run
{
    [DisallowMultipleComponent]
    public sealed class RunCoinPickup : MonoBehaviour
    {
        private const int SpriteSize = 12;
        private const float PixelsPerUnit = 16f;
        private const float VisualScale = 0.6f;
        private const float ColliderRadius = 0.24f;
        private const string CharacterSortingLayer = "Characters";

        private static Sprite s_coinSprite;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _presentationSprite;

        private Vector2 _velocity;
        private float _deceleration;
        private float _remainingLifetime;

        public bool IsAvailable => !gameObject.activeSelf;

        public bool TryCollect(Vector3 collectorPosition, float pickupRadius)
        {
            if (IsAvailable)
            {
                return false;
            }

            float radius = Mathf.Max(0f, pickupRadius);
            if ((transform.position - collectorPosition).sqrMagnitude > radius * radius)
            {
                return false;
            }

            gameObject.SetActive(false);
            return true;
        }

        public void Spawn(Vector3 position, Vector2 velocity, float deceleration, float lifetime)
        {
            transform.position = position;
            transform.rotation = Quaternion.identity;
            _velocity = velocity;
            _deceleration = Mathf.Max(0f, deceleration);
            _remainingLifetime = Mathf.Max(0f, lifetime);
            gameObject.SetActive(true);
        }

        public void Tick(
            float deltaTime,
            float minimumY,
            float maximumY,
            float leftDespawnBoundary,
            float worldScrollDelta)
        {
            transform.position += (Vector3)(_velocity * deltaTime);
            transform.position += Vector3.left * worldScrollDelta;
            _velocity = Vector2.MoveTowards(_velocity, Vector2.zero, _deceleration * deltaTime);
            transform.Rotate(0f, 0f, 240f * deltaTime);
            _remainingLifetime -= deltaTime;

            Vector3 position = transform.position;
            position.y = Mathf.Clamp(position.y, minimumY, maximumY);
            transform.position = position;

            if (_remainingLifetime <= 0f || position.x < leftDespawnBoundary)
            {
                gameObject.SetActive(false);
            }
        }

        private void Awake()
        {
            EnsurePresentation();
        }

        private void EnsurePresentation()
        {
            _spriteRenderer ??= GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
            transform.localScale = Vector3.one * VisualScale;
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _spriteRenderer.sprite = _presentationSprite != null
                ? _presentationSprite
                : GetCoinSprite();
            _spriteRenderer.sortingLayerName = CharacterSortingLayer;
            _spriteRenderer.sortingOrder = 3;

            CircleCollider2D coinCollider = GetComponent<CircleCollider2D>();
            if (coinCollider == null)
            {
                coinCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            coinCollider.isTrigger = true;
            coinCollider.radius = ColliderRadius;

            Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody2D>();
            }

            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.gravityScale = 0f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private static Sprite GetCoinSprite()
        {
            if (s_coinSprite != null)
            {
                return s_coinSprite;
            }

            Texture2D texture = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave,
            };

            for (int y = 0; y < SpriteSize; y++)
            {
                for (int x = 0; x < SpriteSize; x++)
                {
                    texture.SetPixel(x, y, GetCoinPixel(x, y));
                }
            }

            texture.Apply();
            s_coinSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, SpriteSize, SpriteSize),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit);
            s_coinSprite.hideFlags = HideFlags.HideAndDontSave;
            return s_coinSprite;
        }

        private static Color GetCoinPixel(int x, int y)
        {
            int center = (SpriteSize - 1) / 2;
            int distanceX = x - center;
            int distanceY = y - center;
            int distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            if (distanceSquared > 30)
            {
                return Color.clear;
            }

            if (distanceSquared > 20)
            {
                return new Color(0.78f, 0.37f, 0.04f, 1f);
            }

            if (x <= 4 && y >= 7)
            {
                return new Color(1f, 0.97f, 0.62f, 1f);
            }

            return new Color(1f, 0.77f, 0.08f, 1f);
        }
    }
}
