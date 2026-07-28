using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class YAxisSortingOrder : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] _renderers;
        [SerializeField] private int _sortingOrderOffset = 1000;
        [SerializeField, Min(1)] private int _ordersPerUnit = 100;

        public void Configure(int sortingOrderOffset)
        {
            _sortingOrderOffset = sortingOrderOffset;
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void LateUpdate()
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            var sortingOrder = _sortingOrderOffset
                - Mathf.RoundToInt(transform.position.y * _ordersPerUnit);
            for (var index = 0; index < _renderers.Length; index++)
            {
                if (_renderers[index] != null)
                {
                    _renderers[index].sortingOrder = sortingOrder;
                }
            }
        }
    }
}
