using UnityEngine;

namespace CatMouse.Game.Player
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class PlayerSceneTestEnemy : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _hitCollider;

        [Header("Combat")]
        [SerializeField, Min(1)] private int _maximumHealth = 1;
        [SerializeField, Min(0f)] private float _healthRestoreOnDefeat = 8f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _approachSpeed = 1.5f;
        [SerializeField] private float _despawnX = -8f;
        [SerializeField, Min(0f)] private float _respawnDelay = 0.5f;
        [SerializeField, Min(0f)] private float _rightEdgePadding = 0.5f;

        private int _currentHealth;
        private Vector3 _spawnPosition;
        private bool _isRespawning;
        private float _respawnTimer;

        public bool IsAlive => gameObject.activeInHierarchy && !_isRespawning && _currentHealth > 0;
        public float HealthRestoreOnDefeat => _healthRestoreOnDefeat;

        private void Awake()
        {
            _spawnPosition = transform.position;
            _currentHealth = _maximumHealth;
            SetPresentationVisible(true);
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (_isRespawning)
            {
                _respawnTimer -= Time.deltaTime;
                if (_respawnTimer <= 0f)
                {
                    RespawnAtRightEdge();
                }

                return;
            }

            if (!IsAlive)
            {
                return;
            }

            transform.position += Vector3.left * (_approachSpeed * Time.deltaTime);
            if (transform.position.x <= _despawnX)
            {
                BeginRespawn();
            }
        }

        private void OnValidate()
        {
            _maximumHealth = Mathf.Max(1, _maximumHealth);
            _approachSpeed = Mathf.Max(0f, _approachSpeed);
            _respawnDelay = Mathf.Max(0f, _respawnDelay);
            _rightEdgePadding = Mathf.Max(0f, _rightEdgePadding);
        }

        public bool TryTakeDamage(int damage, out bool wasDefeated)
        {
            wasDefeated = false;
            if (!IsAlive || damage <= 0)
            {
                return false;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            if (_currentHealth > 0)
            {
                return true;
            }

            wasDefeated = true;
            BeginRespawn();
            return true;
        }

        private void BeginRespawn()
        {
            _isRespawning = true;
            _respawnTimer = _respawnDelay;
            _currentHealth = 0;
            SetPresentationVisible(false);
        }

        private void RespawnAtRightEdge()
        {
            float cameraDistance = Mathf.Abs(transform.position.z - _worldCamera.transform.position.z);
            Vector3 rightEdge = _worldCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, cameraDistance));

            transform.position = new Vector3(
                rightEdge.x + _rightEdgePadding,
                _spawnPosition.y,
                _spawnPosition.z);
            _currentHealth = _maximumHealth;
            _isRespawning = false;
            SetPresentationVisible(true);
        }

        private void SetPresentationVisible(bool visible)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = visible;
            }

            if (_hitCollider != null)
            {
                _hitCollider.enabled = visible;
            }
        }
    }
}
