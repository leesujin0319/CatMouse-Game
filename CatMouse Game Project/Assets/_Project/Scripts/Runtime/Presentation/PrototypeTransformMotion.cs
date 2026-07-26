using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PrototypeTransformMotion : MonoBehaviour
    {
        private const float TwoPi = Mathf.PI * 2f;

        [SerializeField, Min(0f)] private float _verticalAmplitude = 0.04f;
        [SerializeField, Min(0f)] private float _rotationAmplitudeDegrees = 2f;
        [SerializeField, Min(0.01f)] private float _cyclesPerSecond = 3f;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private float _phaseRadians;

        private void Awake()
        {
            _phaseRadians = Mathf.Abs(GetInstanceID() % 1000) * 0.001f * TwoPi;
            CaptureBaseTransform();
        }

        private void OnEnable()
        {
            CaptureBaseTransform();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var wave = Mathf.Sin((Time.time * _cyclesPerSecond * TwoPi) + _phaseRadians);
            transform.localPosition =
                _baseLocalPosition +
                (Vector3.up * (Mathf.Abs(wave) * _verticalAmplitude));
            transform.localRotation =
                _baseLocalRotation *
                Quaternion.Euler(0f, 0f, wave * _rotationAmplitudeDegrees);
        }

        private void OnDisable()
        {
            transform.localPosition = _baseLocalPosition;
            transform.localRotation = _baseLocalRotation;
        }

        private void OnValidate()
        {
            _verticalAmplitude = Mathf.Max(0f, _verticalAmplitude);
            _rotationAmplitudeDegrees = Mathf.Max(0f, _rotationAmplitudeDegrees);
            _cyclesPerSecond = Mathf.Max(0.01f, _cyclesPerSecond);
        }

        private void CaptureBaseTransform()
        {
            _baseLocalPosition = transform.localPosition;
            _baseLocalRotation = transform.localRotation;
        }
    }
}
