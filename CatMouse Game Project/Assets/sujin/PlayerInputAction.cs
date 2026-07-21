using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputAction : IDisposable
{
    private readonly InputActionAsset asset;
    private readonly InputActionMap playerMouse;
    private readonly InputAction move;

    public PlayerInputAction()
    {
        // 수정: PlayerInputAction.inputactions의 현재 설정(Space, Mouse Left)을 코드에서 바로 사용하기 위한 경량 래퍼입니다.
        asset = InputActionAsset.FromJson(@"{
    ""version"": 1,
    ""name"": ""PlayerInputAction"",
    ""maps"": [
        {
            ""name"": ""PlayerMouse"",
            ""id"": ""c0f97928-11ea-47b5-9019-1d1aee1f73da"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Button"",
                    ""id"": ""db62f14c-a5f8-4b1c-9e9e-a5740d2538fb"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""c0e08853-400f-488a-9861-20fc87b79baa"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""664445ed-c68e-48a0-8b90-07d518a6b652"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");

        playerMouse = asset.FindActionMap("PlayerMouse", throwIfNotFound: true);
        move = playerMouse.FindAction("Move", throwIfNotFound: true);
    }

    public void Dispose()
    {
        if (asset != null)
        {
            UnityEngine.Object.Destroy(asset);
        }
    }

    public PlayerMouseActions PlayerMouse => new PlayerMouseActions(this);

    public readonly struct PlayerMouseActions
    {
        private readonly PlayerInputAction wrapper;

        public PlayerMouseActions(PlayerInputAction wrapper)
        {
            this.wrapper = wrapper;
        }

        public InputAction Move => wrapper.move;

        public void Enable()
        {
            wrapper.playerMouse.Enable();
        }

        public void Disable()
        {
            wrapper.playerMouse.Disable();
        }
    }
}
