using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class DevelopmentAutoRunner : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _forwardSpeed = 3.5f;
        [SerializeField] private bool _isPreviewEnabled = true;

        private void Update()
        {
            if (!_isPreviewEnabled)
            {
                return;
            }

            transform.position += Vector3.right * (_forwardSpeed * Time.deltaTime);
        }
    }
}
