using CatMouse.Game.Enemy;
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
        [SerializeField] private CircleCollider2D _hitCollider;
        [SerializeField] private Rigidbody2D _rigidbody;

        private PlayerSceneTestEnemy _target;
        private PlayerRunHealth _runHealth;
        private PrototypeEnemySpawner _enemySpawner;
        private Vector2 _direction;
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
            ConfigureHitCollider();
            gameObject.SetActive(true);
        }

        public void Spawn(
            Vector3 position,
            PrototypeEnemySpawner enemySpawner,
            Vector2 direction,
            float speed,
            int damage,
            float lifetime,
            float hitRadius)
        {
            transform.position = position;
            _target = null;
            _runHealth = null;
            _enemySpawner = enemySpawner;
            _direction = direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector2.right;
            _speed = Mathf.Max(0f, speed);
            _damage = Mathf.Max(1, damage);
            _remainingLifetime = Mathf.Max(0.1f, lifetime);
            _hitRadius = Mathf.Max(0f, hitRadius);
            transform.right = _direction;
            ConfigureHitCollider();
            gameObject.SetActive(true);
        }

        public void Tick(float deltaTime)
        {
            if (IsAvailable)
            {
                return;
            }

            _remainingLifetime -= deltaTime;
            if (_remainingLifetime <= 0f)
            {
                Deactivate();
                return;
            }

            if (_enemySpawner != null)
            {
                transform.position += (Vector3)(_direction * (_speed * deltaTime));
                return;
            }

            if (_target == null || !_target.IsAlive)
            {
                Deactivate();
                return;
            }

            Vector3 toTarget = _target.transform.position - transform.position;
            float distance = toTarget.magnitude;
            float movement = _speed * deltaTime;
            if (distance <= _hitRadius + movement)
            {
                if (_target.TryTakeDamage(_damage, out bool wasDefeated) && wasDefeated)
                {
                    _runHealth?.Restore(_target.HealthRestoreOnDefeat);
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsAvailable || _enemySpawner == null)
            {
                return;
            }

            PrototypeEnemyMover enemy = other.GetComponentInParent<PrototypeEnemyMover>();
            if (enemy == null || enemy.IsAvailable)
            {
                return;
            }

            enemy.TryTakeDamage(_damage, _direction);
            Deactivate();
        }

        private void Deactivate()
        {
            _target = null;
            _runHealth = null;
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

            _spriteRenderer.sprite = GetAcornSprite();
            _spriteRenderer.sortingLayerName = CharacterSortingLayer;
            _spriteRenderer.sortingOrder = 3;

            _hitCollider ??= GetComponent<CircleCollider2D>();
            if (_hitCollider == null)
            {
                _hitCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            _hitCollider.isTrigger = true;

            _rigidbody ??= GetComponent<Rigidbody2D>();
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody2D>();
            }

            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.gravityScale = 0f;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void ConfigureHitCollider()
        {
            if (_hitCollider != null)
            {
                _hitCollider.radius = Mathf.Max(0.01f, _hitRadius);
            }
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
