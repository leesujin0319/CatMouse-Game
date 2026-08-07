using UnityEngine;

namespace CatMouse.Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SortingOrderOffset : MonoBehaviour
    {
        [SerializeField] private int _value;

        public int Value => _value;

        public void Configure(int value)
        {
            _value = value;
        }
    }
}
