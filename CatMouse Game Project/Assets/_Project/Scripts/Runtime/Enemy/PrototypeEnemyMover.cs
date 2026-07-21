using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [DisallowMultipleComponent]
    public sealed class PrototypeEnemyMover : MonoBehaviour
    {
        private const int SpriteWidth = 8;
        private const int SpriteHeight = 12;
        private const float PixelsPerUnit = 12f;

        private static Sprite s_prototypeSprite;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _color = new Color(0.88f, 0.33f, 0.25f, 1f);

        private float _speed;

        public bool IsAvailable => !gameObject.activeSelf;

        public void Spawn(Vector3 position, float speed)
        {
            transform.position = position;
            _speed = Mathf.Max(0f, speed);
            gameObject.SetActive(true);
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
    }
}
