using UnityEngine;
using UnityEngine.EventSystems;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const float PressedScale = 0.96f;
        private const float RecoverySpeed = 18f;

        private Vector3 _defaultScale;
        private Vector3 _targetScale;

        private void Awake()
        {
            _defaultScale = transform.localScale;
            _targetScale = _defaultScale;
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                _targetScale,
                Time.unscaledDeltaTime * RecoverySpeed);
        }

        private void OnDisable()
        {
            transform.localScale = _defaultScale;
            _targetScale = _defaultScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _targetScale = _defaultScale * PressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _targetScale = _defaultScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _targetScale = _defaultScale;
        }
    }
}
