using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerMovement : MonoBehaviour
{
    [Header("이동 속도")]

    [SerializeField]
    [Min(0f)]
    private float moveUpSpeed = 5f;

    [SerializeField]
    [Min(0f)]
    private float moveDownSpeed = 5f;

    [Header("이동 범위")]

    [SerializeField]
    private float minY = -4f;

    [SerializeField]
    private float maxY = 4f;

    private Rigidbody2D rigidbody2D;

    // 1: 위, -1: 아래, 0: 정지
    private float verticalDirection;

    private float fixedXPosition;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        fixedXPosition = rigidbody2D.position.x;
    }

    public void SetVerticalDirection(float direction)
    {
        verticalDirection = Mathf.Clamp(direction, -1f, 1f);
    }
    public void Move()
    {
        float speed = GetCurrentSpeed();

        Vector2 currentPosition = rigidbody2D.position;

        Vector2 nextPosition =
            currentPosition
            + Vector2.up
            * verticalDirection
            * speed
            * Time.fixedDeltaTime;

        nextPosition.x = fixedXPosition;
        nextPosition.y = Mathf.Clamp(nextPosition.y, minY, maxY);

        rigidbody2D.MovePosition(nextPosition);
    }

    public void Stop()
    {
        verticalDirection = 0f;
    }

    private float GetCurrentSpeed()
    {
        if (verticalDirection > 0f)
        {
            return moveUpSpeed;
        }

        if (verticalDirection < 0f)
        {
            return moveDownSpeed;
        }

        return 0f;
    }

    private void OnValidate()
    {
        // Inspector에서 minY가 maxY보다 커지는 잘못된 설정 방지
        if (minY > maxY)
        {
            float temporaryValue = minY;
            minY = maxY;
            maxY = temporaryValue;
        }
    }
}