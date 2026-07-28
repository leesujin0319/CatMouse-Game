using CatMouse.Game.Player;
using UnityEngine;

namespace CatMouse.Game.Run
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class RunStatItemPickup : MonoBehaviour
    {
        [SerializeField] private RunItemDefinition _itemDefinition;
        [SerializeField] private bool _destroyOnPickup = true;

        private bool _isConsumed;

        private void Reset()
        {
            Collider2D collider2D = GetComponent<Collider2D>();
            if (collider2D != null)
            {
                collider2D.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isConsumed || _itemDefinition == null)
            {
                return;
            }

            PlayerRunStats playerRunStats = other.GetComponentInParent<PlayerRunStats>();
            if (playerRunStats == null || !playerRunStats.TryAcquire(_itemDefinition))
            {
                return;
            }

            _isConsumed = true;
            if (_destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
