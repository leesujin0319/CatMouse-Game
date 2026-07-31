using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [DisallowMultipleComponent]
    public sealed class PrototypeCheeseDrop : MonoBehaviour
    {
        private const int SpriteSize = 8;
        private const float PixelsPerUnit = 16f;
        private const float ColliderRadius = 0.22f;
        private const string CharacterSortingLayer = "Characters";

        private static Sprite s_prototypeSprite;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _color = new Color(1f, 0.73f, 0.08f, 1f);

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

            var radius = Mathf.Max(0f, pickupRadius);
            if ((transform.position - collectorPosition).sqrMagnitude > radius * radius)
            {
                return false;
            }

            gameObject.SetActive(false);
            return true;
        }

        public void Spawn(
            Vector3 position,
            Vector2 velocity,
            float deceleration,
            float lifetime)
        {
            transform.position = position;
            _velocity = velocity;
            _deceleration = Mathf.Max(0f, deceleration);
            _remainingLifetime = Mathf.Max(0f, lifetime);
            gameObject.SetActive(true);
        }

        public void AttractTo(Vector3 collectorPosition, float magnetRadius, float magnetSpeed, float deltaTime)
        {
            var radius = Mathf.Max(0f, magnetRadius);
            if (radius <= 0f
                || (transform.position - collectorPosition).sqrMagnitude > radius * radius)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                collectorPosition,
                Mathf.Max(0f, magnetSpeed) * deltaTime);
        }

        public void Tick(
            float deltaTime,
            float minimumY,
            float maximumY,
            float leftDespawnBoundary,
            float worldScrollSpeed)
        {
            transform.position += (Vector3)((_velocity + (Vector2.left * worldScrollSpeed)) * deltaTime);
            _velocity = Vector2.MoveTowards(_velocity, Vector2.zero, _deceleration * deltaTime);
            _remainingLifetime -= deltaTime;

            var position = transform.position;
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
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _spriteRenderer.sprite = GetPrototypeSprite();
            _spriteRenderer.color = _color;
            _spriteRenderer.sortingLayerName = CharacterSortingLayer;
            _spriteRenderer.sortingOrder = 2;

            var cheeseCollider = GetComponent<CircleCollider2D>();
            if (cheeseCollider == null)
            {
                cheeseCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            cheeseCollider.isTrigger = true;
            cheeseCollider.radius = ColliderRadius;

            var rigidbody = GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody2D>();
            }

            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.gravityScale = 0f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private static Sprite GetPrototypeSprite()
        {
            if (s_prototypeSprite != null)
            {
                return s_prototypeSprite;
            }

            var texture = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave,
            };

            for (var y = 0; y < SpriteSize; y++)
            {
                for (var x = 0; x < SpriteSize; x++)
                {
                    texture.SetPixel(x, y, IsSolidPixel(x, y) ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            s_prototypeSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, SpriteSize, SpriteSize),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit);
            s_prototypeSprite.hideFlags = HideFlags.HideAndDontSave;
            return s_prototypeSprite;
        }

        private static bool IsSolidPixel(int x, int y)
        {
            var isBody = y >= 1 && y <= 6 && x >= 1 && x <= 6;
            var isUpperCorner = y == 7 && x >= 2 && x <= 5;
            var isHole = (x == 3 && y == 4) || (x == 5 && y == 2);
            return (isBody || isUpperCorner) && !isHole;
        }
    }
}
