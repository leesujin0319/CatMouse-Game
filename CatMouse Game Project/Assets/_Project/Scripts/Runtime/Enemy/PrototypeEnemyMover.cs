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
        private const int DefaultContactDamage = 5;
        private const float HitFeedbackDuration = 0.12f;
        private const float PoisonTickInterval = 1f;
        private const float PixelsPerUnit = 12f;
        private const string CharacterSortingLayer = "Characters";

        private static readonly Color HitColor = new(1f, 0.45f, 0.45f, 1f);

        private static Sprite s_prototypeSprite;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private RunSpriteMotion _presentation;
        [SerializeField] private Color _color = new Color(0.88f, 0.33f, 0.25f, 1f);
        [SerializeField, Min(1)] private int _maximumHealth = DefaultMaximumHealth;
        [SerializeField, Min(1)] private int _contactDamage = DefaultContactDamage;

        private float _approachSpeed;
        private float _poisonRemainingTime;
        private float _poisonTickTimer;
        private int _poisonDamage;
        private Vector2 _poisonHitDirection;
        private float _hitFeedbackRemaining;
        private int _currentHealth;
        private int _currentContactDamage;
        private Color _baseColor;
        private Vector3 _baseLocalScale;
        private Vector3 _spawnLocalScale;
        private float _nextRangedAttackTime;

        public bool IsAvailable => !gameObject.activeSelf;
        public EnemyArchetypeDefinition CurrentArchetype { get; private set; }
        public EnemyEquipmentDefinition CurrentEquipment { get; private set; }
        public int CurrentHealth => _currentHealth;
        public int MaximumHealth { get; private set; }
        public float DifficultyHealthMultiplier { get; private set; } = 1f;
        public int ContactDamage => _currentContactDamage;
        public float NormalizedHealth => MaximumHealth > 0 ? _currentHealth / (float)MaximumHealth : 0f;
        public bool UsesRangedAttack => CurrentArchetype != null
            && CurrentArchetype.AttackType == EnemyAttackType.Ranged;
        public float RangedProjectileSpeed => CurrentArchetype != null
            ? CurrentArchetype.RangedProjectileSpeed
            : 0f;
        public int RangedProjectileDamage => CurrentArchetype != null
            ? CurrentArchetype.RangedProjectileDamage
            : 0;
        public Vector3 RangedAttackOrigin => _spriteRenderer != null
            ? _spriteRenderer.bounds.center
            : transform.position;
        public event Action<Vector3, Vector2> Defeated;

        public void Spawn(
            Vector3 position,
            float approachSpeed,
            EnemyArchetypeDefinition archetype,
            float healthMultiplier)
        {
            transform.position = position;
            gameObject.SetActive(true);
            CurrentArchetype = archetype;
            CurrentEquipment = archetype != null ? archetype.Equipment : null;

            var visualScale = archetype != null ? archetype.VisualScale : 1f;
            var archetypeSprite = archetype != null ? archetype.Sprite : null;
            var maximumHealth = archetype != null ? archetype.MaximumHealth : _maximumHealth;
            var contactDamage = archetype != null ? archetype.ContactDamage : _contactDamage;
            DifficultyHealthMultiplier = Mathf.Max(1f, healthMultiplier);
            MaximumHealth = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    (maximumHealth + (CurrentEquipment != null ? CurrentEquipment.HealthBonus : 0)) *
                    DifficultyHealthMultiplier));
            _currentHealth = MaximumHealth;
            _currentContactDamage = Mathf.Max(
                1,
                contactDamage + (CurrentEquipment != null ? CurrentEquipment.ContactDamageBonus : 0));
            var speedMultiplier = archetype != null ? archetype.SpeedMultiplier : 1f;
            var equipmentSpeedMultiplier = CurrentEquipment != null
                ? CurrentEquipment.SpeedMultiplier
                : 1f;
            _approachSpeed = Mathf.Max(
                0f,
                approachSpeed * ((speedMultiplier * equipmentSpeedMultiplier) - 1f));
            _hitFeedbackRemaining = 0f;
            _nextRangedAttackTime = 0f;
            ClearPoison();
            _spriteRenderer.sprite = archetypeSprite != null
                ? archetypeSprite
                : GetPrototypeSprite();
            _baseColor = archetypeSprite != null
                ? Color.white
                : archetype != null
                    ? archetype.Color
                    : _color;
            _spriteRenderer.color = _baseColor;

            var sourceVisualHeight = GetSpriteVisualHeight(_spriteRenderer.sprite);
            var normalizedVisualScale = sourceVisualHeight > Mathf.Epsilon
                ? visualScale / sourceVisualHeight
                : visualScale;
            _spawnLocalScale = _baseLocalScale * normalizedVisualScale;
            transform.localScale = _spawnLocalScale;
            GetComponent<CharacterSpriteCollider2D>()?.Refresh();
        }

        public bool TryStartRangedAttack(Vector3 targetPosition)
        {
            if (!UsesRangedAttack || Time.time < _nextRangedAttackTime)
            {
                return false;
            }

            float attackRange = CurrentArchetype.RangedAttackRange;
            if ((targetPosition - RangedAttackOrigin).sqrMagnitude > attackRange * attackRange)
            {
                return false;
            }

            _nextRangedAttackTime = Time.time + CurrentArchetype.RangedAttackCooldown;
            PlayAttackPresentation();
            return true;
        }

        public void PlayAttackPresentation()
        {
            _presentation?.PlayAttack();
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
                _hitFeedbackRemaining = HitFeedbackDuration;
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

        public void ApplyPoison(int damagePerTick, float duration, Vector2 hitDirection)
        {
            if (IsAvailable || damagePerTick <= 0 || duration <= 0f)
            {
                return;
            }

            _poisonDamage = Mathf.Max(_poisonDamage, damagePerTick);
            _poisonRemainingTime = Mathf.Max(_poisonRemainingTime, duration);
            _poisonTickTimer = PoisonTickInterval;
            _poisonHitDirection = hitDirection.sqrMagnitude > Mathf.Epsilon
                ? hitDirection.normalized
                : Vector2.right;
        }

        public void Tick(float deltaTime, float leftDespawnBoundary, float worldScrollDelta)
        {
            TickPoison(deltaTime);
            if (IsAvailable)
            {
                return;
            }

            UpdateHitFeedback(deltaTime);
            transform.position += Vector3.left * (worldScrollDelta + (_approachSpeed * deltaTime));
            if (transform.position.x < leftDespawnBoundary)
            {
                gameObject.SetActive(false);
            }
        }

        private void TickPoison(float deltaTime)
        {
            if (_poisonDamage <= 0 || _poisonRemainingTime <= 0f)
            {
                return;
            }

            _poisonRemainingTime -= deltaTime;
            _poisonTickTimer -= deltaTime;
            if (_poisonTickTimer > 0f)
            {
                if (_poisonRemainingTime <= 0f)
                {
                    ClearPoison();
                }

                return;
            }

            _poisonTickTimer = PoisonTickInterval;
            bool wasDefeated = TryTakeDamage(_poisonDamage, _poisonHitDirection);
            if (wasDefeated || _poisonRemainingTime <= 0f)
            {
                ClearPoison();
            }
        }

        private void ClearPoison()
        {
            _poisonDamage = 0;
            _poisonRemainingTime = 0f;
            _poisonTickTimer = 0f;
            _poisonHitDirection = Vector2.right;
        }

        private void Awake()
        {
            _baseLocalScale = transform.localScale;
            _spawnLocalScale = _baseLocalScale;
            _presentation ??= GetComponentInChildren<RunSpriteMotion>();
            EnsurePresentation();
        }

        private void OnValidate()
        {
            _maximumHealth = Mathf.Max(1, _maximumHealth);
            _contactDamage = Mathf.Max(1, _contactDamage);
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
            _baseColor = _color;
        }

        private void UpdateHitFeedback(float deltaTime)
        {
            if (_hitFeedbackRemaining <= 0f)
            {
                return;
            }

            _hitFeedbackRemaining = Mathf.Max(0f, _hitFeedbackRemaining - deltaTime);
            var normalizedRemaining = _hitFeedbackRemaining / HitFeedbackDuration;
            _spriteRenderer.color = Color.Lerp(_baseColor, HitColor, normalizedRemaining);

            if (_hitFeedbackRemaining <= 0f)
            {
                _spriteRenderer.color = _baseColor;
            }
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
