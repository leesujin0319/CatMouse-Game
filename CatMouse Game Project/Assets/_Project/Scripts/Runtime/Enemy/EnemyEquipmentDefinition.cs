using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [CreateAssetMenu(
        fileName = "EnemyEquipmentDefinition",
        menuName = "CatMouse/Run/Enemy Equipment Definition")]
    public sealed class EnemyEquipmentDefinition : ScriptableObject
    {
        [SerializeField, Min(0)] private int _healthBonus = 1;
        [SerializeField, Min(0)] private int _contactDamageBonus = 1;
        [SerializeField, Min(0.1f)] private float _speedMultiplier = 1f;

        public int HealthBonus => _healthBonus;
        public int ContactDamageBonus => _contactDamageBonus;
        public float SpeedMultiplier => _speedMultiplier;

        private void OnValidate()
        {
            _healthBonus = Mathf.Max(0, _healthBonus);
            _contactDamageBonus = Mathf.Max(0, _contactDamageBonus);
            _speedMultiplier = Mathf.Max(0.1f, _speedMultiplier);
        }
    }
}
