using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class RunSpriteMotion : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _bobHeight = 0.035f;
        [SerializeField, Min(0f)] private float _bobFrequency = 7f;
        [SerializeField, Min(0f)] private float _attackLungeDistance = 0.08f;
        [SerializeField, Min(0.01f)] private float _attackLungeDuration = 0.14f;
        [SerializeField] private float _attackDirection = 1f;

        private Vector3 _baseLocalPosition;
        private float _animationTime;
        private float _attackRemaining;

        private void Awake()
        {
            _baseLocalPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            _animationTime = 0f;
            _attackRemaining = 0f;
        }

        private void OnDisable()
        {
            transform.localPosition = _baseLocalPosition;
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _animationTime += Time.deltaTime;
            _attackRemaining = Mathf.Max(0f, _attackRemaining - Time.deltaTime);

            float bobOffset = Mathf.Sin(_animationTime * _bobFrequency) * _bobHeight;
            float attackProgress = _attackLungeDuration > 0f
                ? _attackRemaining / _attackLungeDuration
                : 0f;
            float attackOffset = Mathf.Sin(attackProgress * Mathf.PI)
                * _attackLungeDistance
                * Mathf.Sign(Mathf.Approximately(_attackDirection, 0f) ? 1f : _attackDirection);
            transform.localPosition = _baseLocalPosition + new Vector3(attackOffset, bobOffset, 0f);
        }

        public void PlayAttack()
        {
            _attackRemaining = _attackLungeDuration;
        }

        private void OnValidate()
        {
            _bobHeight = Mathf.Max(0f, _bobHeight);
            _bobFrequency = Mathf.Max(0f, _bobFrequency);
            _attackLungeDistance = Mathf.Max(0f, _attackLungeDistance);
            _attackLungeDuration = Mathf.Max(0.01f, _attackLungeDuration);
        }
    }
}
