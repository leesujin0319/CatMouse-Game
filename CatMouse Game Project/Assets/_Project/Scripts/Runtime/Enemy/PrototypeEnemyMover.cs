using System;
using CatMouse.Game.Presentation;
using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [DisallowMultipleComponent]
    public sealed class PrototypeEnemyMover : MonoBehaviour
    {
        private const int SpriteWidth = 8;
        private const int SpriteHeight = 12;
        private const int DefaultMaximumHealth = 1;
        private const float PixelsPerUnit = 12f;
        private const string CharacterSortingLayer = "Characters";

        private static Sprite s_prototypeSprite;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _color = new Color(0.88f, 0.33f, 0.25f, 1f);
        [SerializeField, Min(1)] private int _maximumHealth = DefaultMaximumHealth;

        private float _speed;
        private int _currentHealth;
        private Vector3 _baseLocalScale;

        public bool IsAvailable => !gameObject.activeSelf;
        public EnemyArchetypeDefinition CurrentArchetype { get; private set; }
        public event Action<Vector3, Vector2> Defeated;

        public void Spawn(
            Vector3 position,
            float speed,
            EnemyArchetypeDefinition archetype)
        {
            transform.position = position;
            gameObject.SetActive(true);
            CurrentArchetype = archetype;

            var speedMultiplier = archetype != null ? archetype.SpeedMultiplier : 1f;
            var visualScale = archetype != null ? archetype.VisualScale : 1f;
            var archetypeSprite = archetype != null ? archetype.Sprite : null;
            _speed = Mathf.Max(0f, speed * speedMultiplier);
            _currentHealth = archetype != null ? archetype.MaximumHealth : _maximumHealth;
            _spriteRenderer.sprite = archetypeSprite != null
                ? archetypeSprite
                : GetPrototypeSprite();
            _spriteRenderer.color = archetypeSprite != null
                ? Color.white
                : archetype != null
                    ? archetype.Color
                    : _color;

            var sourceVisualHeight = GetSpriteVisualHeight(_spriteRenderer.sprite);
            var normalizedVisualScale = sourceVisualHeight > Mathf.Epsilon
                ? visualScale / sourceVisualHeight
                : visualScale;
            transform.localScale = _baseLocalScale * normalizedVisualScale;
            GetComponent<CharacterSpriteCollider2D>()?.Refresh();
        }

        public bool TryTakeDamage(int damage, Vector2 hitDirection)
        {
            if (IsAvailable || damage <= 0)
            {
                return false;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            if (_currentHealth > 0)
            {
                return false;
            }

            var defeatedPosition = transform.position;
            var normalizedHitDirection = hitDirection.sqrMagnitude > Mathf.Epsilon
                ? hitDirection.normalized
                : Vector2.right;
            gameObject.SetActive(false);
            Defeated?.Invoke(defeatedPosition, normalizedHitDirection);
            return true;
        }

        public void Tick(float deltaTime, float leftDespawnBoundary)
        {
            transform.position += Vector3.left * (_speed * deltaTime);

            if (transform.position.x < leftDespawnBoundary)
            {
                gameObject.SetActive(false);
            }
        }

        private void Awake()
        {
            _baseLocalScale = transform.localScale;
            EnsurePresentation();
        }

        private void OnValidate()
        {
            _maximumHealth = Mathf.Max(1, _maximumHealth);
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
            _spriteRenderer.sortingOrder = 1;
        }

        private static Sprite GetPrototypeSprite()
        {
            if (s_prototypeSprite != null)
            {
                return s_prototypeSprite;
            }

            var texture = new Texture2D(SpriteWidth, SpriteHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave,
            };

            for (var y = 0; y < SpriteHeight; y++)
            {
                for (var x = 0; x < SpriteWidth; x++)
                {
                    texture.SetPixel(x, y, IsSolidPixel(x, y) ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            s_prototypeSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, SpriteWidth, SpriteHeight),
                new Vector2(0.5f, 0f),
                PixelsPerUnit);
            s_prototypeSprite.hideFlags = HideFlags.HideAndDontSave;
            return s_prototypeSprite;
        }

        private static bool IsSolidPixel(int x, int y)
        {
            var isHead = y >= 7 && x >= 1 && x <= 6;
            var isBody = y >= 2 && y <= 6 && x >= 2 && x <= 5;
            var isFoot = y <= 1 && (x == 2 || x == 5);
            var isEar = y >= 10 && (x == 1 || x == 6);
            return isHead || isBody || isFoot || isEar;
        }

        private static float GetSpriteVisualHeight(Sprite sprite)
        {
            if (sprite == null)
            {
                return 0f;
            }

            var vertices = sprite.vertices;
            if (vertices == null || vertices.Length == 0)
            {
                return sprite.bounds.size.y;
            }

            var minimumY = vertices[0].y;
            var maximumY = vertices[0].y;

            for (var index = 1; index < vertices.Length; index++)
            {
                minimumY = Mathf.Min(minimumY, vertices[index].y);
                maximumY = Mathf.Max(maximumY, vertices[index].y);
            }

            return maximumY - minimumY;
        }
    }
}
