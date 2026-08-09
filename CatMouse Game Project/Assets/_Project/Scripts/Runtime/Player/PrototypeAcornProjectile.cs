using CatMouse.Game.Enemy;
using CatMouse.Game.Presentation;
using UnityEngine;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PrototypeAcornProjectile : MonoBehaviour
    {
        private const int SpriteWidth = 6;
        private const int SpriteHeight = 5;
        private const float PixelsPerUnit = 12f;
        private const float HitRadius = 0.35f;
        private const string CharacterSortingLayer = "Characters";

        private static readonly Color32 BodyColor = new(150, 88, 39, 255);
        private static readonly Color32 CapColor = new(91, 55, 31, 255);
        private static Sprite s_prototypeSprite;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _presentationSprite;
        [SerializeField] private RunVisualEffectPool _visualEffectPool;

        private PrototypeEnemySpawner _enemySpawner;
        private float _speed;
        private float _remainingLifetime;
        private int _damage;

        public bool IsAvailable => !gameObject.activeSelf;

        public void Spawn(
            Vector3 position,
            PrototypeEnemySpawner enemySpawner,
            float speed,
            int damage,
            float lifetime)
        {
            transform.position = position;
            _enemySpawner = enemySpawner;
            _speed = Mathf.Max(0f, speed);
            _damage = Mathf.Max(1, damage);
            _remainingLifetime = Mathf.Max(0f, lifetime);
            gameObject.SetActive(true);
        }

        public void Tick(float deltaTime)
        {
            if (IsAvailable)
            {
                return;
            }

            _remainingLifetime -= deltaTime;
            if (_remainingLifetime <= 0f || _enemySpawner == null)
            {
                Deactivate();
                return;
            }

            var nextPosition = transform.position + (Vector3.right * (_speed * deltaTime));
            if (_enemySpawner.TryDamageFirstEnemyInSegment(
                    transform.position,
                    nextPosition,
                    HitRadius,
                    _damage,
                    Vector2.right))
            {
                _visualEffectPool?.PlayImpact(nextPosition);
                Deactivate();
                return;
            }

            transform.position = nextPosition;
        }

        private void Awake()
        {
            EnsurePresentation();
        }

        private void Deactivate()
        {
            _enemySpawner = null;
            gameObject.SetActive(false);
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

            _spriteRenderer.sprite = _presentationSprite != null
                ? _presentationSprite
                : GetPrototypeSprite();
            _spriteRenderer.sortingLayerName = CharacterSortingLayer;
            _spriteRenderer.sortingOrder = 3;
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
                    texture.SetPixel(x, y, GetPixelColor(x, y));
                }
            }

            texture.Apply();
            s_prototypeSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, SpriteWidth, SpriteHeight),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit);
            s_prototypeSprite.hideFlags = HideFlags.HideAndDontSave;
            return s_prototypeSprite;
        }

        private static Color GetPixelColor(int x, int y)
        {
            var isCap = y >= 3 && x >= 1 && x <= 4;
            if (isCap)
            {
                return CapColor;
            }

            var isBody = y <= 2
                && ((y == 2 && x >= 1 && x <= 4)
                    || (y == 1 && x >= 1 && x <= 4)
                    || (y == 0 && x >= 2 && x <= 3));
            return isBody ? BodyColor : Color.clear;
        }
    }
}
