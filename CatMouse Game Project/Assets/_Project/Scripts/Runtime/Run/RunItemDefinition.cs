using System.Collections.Generic;
using UnityEngine;

namespace CatMouse.Game.Run
{
    [CreateAssetMenu(
        fileName = "RunItemDefinition",
        menuName = "CatMouse/Run/Item Definition")]
    public sealed class RunItemDefinition : ScriptableObject
    {
        [Header("Presentation")]
        [SerializeField] private string _displayName;
        [SerializeField, TextArea] private string _description;
        [SerializeField] private Sprite _icon;

        [Header("Stacking")]
        [SerializeField, Min(1)] private int _maximumStacks = 1;

        [Header("Temporary Effect")]
        [SerializeField, Min(0f)] private float _temporaryDuration;
        [Header("Combat Evolution")]
        [SerializeField] private RunCombatEffectType _combatEffectType;

        [Header("Stat Modifiers")]
        [SerializeField] private RunItemModifier[] _modifiers = System.Array.Empty<RunItemModifier>();

        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public int MaximumStacks => _maximumStacks;
        public float TemporaryDuration => _temporaryDuration;
        public bool IsTemporary => _temporaryDuration > 0f;
        public RunCombatEffectType CombatEffectType => _combatEffectType;
        public IReadOnlyList<RunItemModifier> Modifiers => _modifiers;

        private void OnValidate()
        {
            _maximumStacks = Mathf.Max(1, _maximumStacks);
            _temporaryDuration = Mathf.Max(0f, _temporaryDuration);
            _modifiers ??= System.Array.Empty<RunItemModifier>();
        }
    }
}
