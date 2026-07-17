using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private PlayerInputAction playerInputAction;

    public bool IsPressed
    {
        get
        {
            if (playerInputAction == null)
            {
                return false;
            }

            return playerInputAction.PlayerMouse.Move.IsPressed();
        }
    }

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();
    }

    private void OnEnable()
    {
        playerInputAction.PlayerMouse.Enable();
    }

    private void OnDisable()
    {
        if (playerInputAction != null)
        {
            playerInputAction.PlayerMouse.Disable();
        }
    }

    private void OnDestroy()
    {
        playerInputAction?.Dispose();
    }
}
