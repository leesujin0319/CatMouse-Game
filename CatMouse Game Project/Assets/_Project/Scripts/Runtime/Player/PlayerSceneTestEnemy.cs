using UnityEngine;

namespace CatMouse.Game.Player
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class PlayerSceneTestEnemy : MonoBehaviour
    {
        [SerializeField, Min(1)] private int _maximumHealth = 1;
        [SerializeField, Min(0f)] private float _healthRestoreOnDefeat = 8f;
        [SerializeField, Min(0f)] private float _approachSpeed = 1.5f;
        [SerializeField] private float _despawnX = -8f;

        private int _currentHealth;
        private Vector3 _spawnPosition;

        public bool IsAlive => gameObject.activeInHierarchy && _currentHealth > 0;

        private void Awake()
        {
            _spawnPosition = transform.position;
            _currentHealth = _maximumHealth;
        }

        private void Update()
        {
            if (!Application.isPlaying || !IsAlive)
            {
                return;
            }

            transform.position += Vector3.left * (_approachSpeed * Time.deltaTime);
            if (transform.position.x <= _despawnX)
            {
                Respawn();
            }
        }

        private void OnValidate()
        {
            _maximumHealth = Mathf.Max(1, _maximumHealth);
            _approachSpeed = Mathf.Max(0f, _approachSpeed);
        }

        public bool TryTakeDamage(int damage, out float healthRestoreAmount)
        {
            healthRestoreAmount = 0f;
            if (!IsAlive || damage <= 0)
            {
                return false;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            if (_currentHealth == 0)
            {
                healthRestoreAmount = _healthRestoreOnDefeat;
                Respawn();
            }

            return true;
        }

        private void Respawn()
        {
            transform.position = _spawnPosition;
            _currentHealth = _maximumHealth;
        }
    }
}
