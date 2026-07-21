using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RunItemPickup : MonoBehaviour
{
    [SerializeField] private float runDuration = 2f;
    [SerializeField] private bool destroyOnPickup = true;

    private bool isConsumed;

    private void Reset()
    {
        // 수정: 아이템을 트리거로 바로 사용할 수 있도록 기본 콜라이더 설정을 맞춥니다.
        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            collider2D.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 수정: 같은 아이템이 여러 번 발동하지 않도록 한 번만 소비되게 합니다.
        if (isConsumed)
        {
            return;
        }

        if (!other.TryGetComponent(out Player player))
        {
            return;
        }

        // 수정: 플레이어가 아이템을 먹으면 Run 효과를 즉시 활성화합니다.
        player.ActivateRunEffect(runDuration);
        isConsumed = true;

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
}
