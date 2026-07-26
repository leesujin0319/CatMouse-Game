using UnityEngine;

namespace CatMouse.Game.Enemy
{
    [CreateAssetMenu(
        fileName = "EnemyArchetypeDefinition",
        menuName = "CatMouse/Run/Enemy Archetype Definition")]
    public sealed class EnemyArchetypeDefinition : ScriptableObject
    {
        [Header("Presentation")]
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _color = Color.white;
        [SerializeField, Min(0.1f)] private float _visualScale = 1f;

        [Header("Stats")]
        [SerializeField, Min(1)] private int _maximumHealth = 1;
        [SerializeField, Min(0.1f)] private float _speedMultiplier = 1f;

        public Sprite Sprite => _sprite;
        public Color Color => _color;
        public float VisualScale => _visualScale;
        public int MaximumHealth => _maximumHealth;
        public float SpeedMultiplier => _speedMultiplier;

        private void OnValidate()
        {
            _visualScale = Mathf.Max(0.1f, _visualScale);
            _maximumHealth = Mathf.Max(1, _maximumHealth);
            _speedMultiplier = Mathf.Max(0.1f, _speedMultiplier);
        }
    }
}
