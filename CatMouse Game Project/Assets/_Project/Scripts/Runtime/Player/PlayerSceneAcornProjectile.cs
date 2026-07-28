using UnityEngine;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerSceneAcornProjectile : MonoBehaviour
    {
        private const int SpriteWidth = 6;
        private const int SpriteHeight = 5;
        private const float PixelsPerUnit = 12f;
        private const string CharacterSortingLayer = "Characters";

        private static readonly Color32 BodyColor = new(150, 88, 39, 255);
        private static readonly Color32 CapColor = new(91, 55, 31, 255);
        private static Sprite s_acornSprite;

        [SerializeField] private SpriteRenderer _spriteRenderer;

        private PlayerSceneTestEnemy _target;
        private PlayerRunHealth _runHealth;
        private float _speed;
        private float _remainingLifetime;
        private float _hitRadius;
        private int _damage;

        public bool IsAvailable => !gameObject.activeSelf;

        public void Spawn(
            Vector3 position,
            PlayerSceneTestEnemy target,
            float speed,
            int damage,
            float lifetime,
            float hitRadius,
            PlayerRunHealth runHealth)
        {
            transform.position = position;
            _target = target;
            _speed = Mathf.Max(0f, speed);
            _damage = Mathf.Max(1, damage);
            _remainingLifetime = Mathf.Max(0.1f, lifetime);
            _hitRadius = Mathf.Max(0f, hitRadius);
            _runHealth = runHealth;
            gameObject.SetActive(true);
        }

        public void Tick(float deltaTime)
        {
            if (IsAvailable)
            {
                return;
            }

            _remainingLifetime -= deltaTime;
            if (_remainingLifetime <= 0f || _target == null || !_target.IsAlive)
            {
                Deactivate();
                return;
            }

            Vector3 toTarget = _target.transform.position - transform.position;
            float distance = toTarget.magnitude;
            float movement = _speed * deltaTime;
            if (distance <= _hitRadius + movement)
            {
                if (_target.TryTakeDamage(_damage, out float healthRestoreAmount))
                {
                    _runHealth?.Restore(healthRestoreAmount);
                }

                Deactivate();
                return;
            }

            Vector3 direction = toTarget / distance;
            transform.position += direction * movement;
            transform.right = direction;
        }

        private void Awake()
        {
            EnsurePresentation();
        }

        private void Deactivate()
        {
            _target = null;
            _runHealth = null;
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

            _spriteRenderer.sprite = GetAcornSprite();
            _spriteRenderer.sortingLayerName = CharacterSortingLayer;
            _spriteRenderer.sortingOrder = 3;
        }

        private static Sprite GetAcornSprite()
        {
            if (s_acornSprite != null)
            {
                return s_acornSprite;
            }

            Texture2D texture = new Texture2D(SpriteWidth, SpriteHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave,
            };

            for (int y = 0; y < SpriteHeight; y++)
            {
                for (int x = 0; x < SpriteWidth; x++)
                {
                    texture.SetPixel(x, y, GetPixelColor(x, y));
                }
            }

            texture.Apply();
            s_acornSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, SpriteWidth, SpriteHeight),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit);
            s_acornSprite.hideFlags = HideFlags.HideAndDontSave;
            return s_acornSprite;
        }

        private static Color GetPixelColor(int x, int y)
        {
            bool isCap = y >= 3 && x >= 1 && x <= 4;
            if (isCap)
            {
                return CapColor;
            }

            bool isBody = y <= 2
                && ((y == 2 && x >= 1 && x <= 4)
                    || (y == 1 && x >= 1 && x <= 4)
                    || (y == 0 && x >= 2 && x <= 3));
            return isBody ? BodyColor : Color.clear;
        }
    }
}
