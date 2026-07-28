using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RunItemPickup : MonoBehaviour
{
    [SerializeField] private float runDuration = 2f;
    [SerializeField] private bool destroyOnPickup = true;

    private bool isConsumed;

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
        if (isConsumed)
        {
            return;
        }

        if (!other.TryGetComponent(out Player player))
        {
            return;
        }

        player.ActivateRunEffect(runDuration);
        isConsumed = true;

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
}
