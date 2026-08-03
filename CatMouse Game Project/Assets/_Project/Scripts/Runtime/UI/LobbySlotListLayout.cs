using UnityEngine;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbySlotListLayout : MonoBehaviour
    {
        [SerializeField] private Vector2 _firstSlotPosition;
        [SerializeField] private Vector2 _slotPitch;
        [SerializeField] private bool _isInitialized;

        public void InitializeIfNeeded(Vector2 firstSlotPosition, Vector2 slotPitch)
        {
            if (_isInitialized)
            {
                return;
            }

            _firstSlotPosition = firstSlotPosition;
            _slotPitch = slotPitch;
            _isInitialized = true;
        }

        public Vector2 GetSlotPosition(int index)
        {
            return _firstSlotPosition + _slotPitch * index;
        }
    }
}
