using System;
using System.Collections.Generic;
using CatMouse.Game.Run;
using UnityEngine;

namespace CatMouse.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRunStats : MonoBehaviour
    {
        private const float MinimumAttackSpeed = 0.05f;

        [SerializeField] private PlayerBaseStatsDefinition _baseStatsDefinition;

        private readonly Dictionary<RunItemDefinition, int> _persistentItemStacks = new();
        private readonly Dictionary<RunItemDefinition, int> _itemStacks = new();
        private readonly Dictionary<RunItemDefinition, int> _temporaryItemStacks = new();
        private readonly Dictionary<RunItemDefinition, float> _temporaryItemStrengths = new();

        public event Action<PlayerStatsSnapshot> StatsChanged;

        public bool IsInitialized { get; private set; }
        public PlayerStatsSnapshot Current { get; private set; }

        private void Awake()
        {
            Initialize(_baseStatsDefinition);
        }

        public void Initialize(PlayerBaseStatsDefinition baseStatsDefinition)
        {
            _baseStatsDefinition = baseStatsDefinition;
            _persistentItemStacks.Clear();
            _itemStacks.Clear();
            _temporaryItemStacks.Clear();
            _temporaryItemStrengths.Clear();
            IsInitialized = _baseStatsDefinition != null;

            if (IsInitialized)
            {
                Recalculate();
            }
        }

        public bool TryAcquire(RunItemDefinition itemDefinition)
        {
            if (!IsInitialized || itemDefinition == null)
            {
                return false;
            }

            _itemStacks.TryGetValue(itemDefinition, out int currentStacks);
            if (currentStacks >= itemDefinition.MaximumStacks)
            {
                return false;
            }

            _itemStacks[itemDefinition] = currentStacks + 1;
            Recalculate();
            return true;
        }

        public int GetStackCount(RunItemDefinition itemDefinition)
        {
            return itemDefinition != null && _itemStacks.TryGetValue(itemDefinition, out int stacks)
                ? stacks
                : 0;
        }

        public int GetPersistentStackCount(RunItemDefinition itemDefinition)
        {
            return itemDefinition != null && _persistentItemStacks.TryGetValue(itemDefinition, out int stacks)
                ? stacks
                : 0;
        }

        public void SetPersistentStackCount(RunItemDefinition itemDefinition, int stackCount)
        {
            if (!IsInitialized || itemDefinition == null)
            {
                return;
            }

            int clampedStackCount = Mathf.Clamp(stackCount, 0, itemDefinition.MaximumStacks);
            int currentStackCount = GetPersistentStackCount(itemDefinition);
            if (currentStackCount == clampedStackCount)
            {
                return;
            }

            if (clampedStackCount == 0)
            {
                _persistentItemStacks.Remove(itemDefinition);
            }
            else
            {
                _persistentItemStacks[itemDefinition] = clampedStackCount;
            }

            Recalculate();
        }

        public void ClearPersistentItems()
        {
            if (!IsInitialized || _persistentItemStacks.Count == 0)
            {
                return;
            }

            _persistentItemStacks.Clear();
            Recalculate();
        }

        public bool TryApplyTemporary(RunItemDefinition itemDefinition)
        {
            if (!IsInitialized || itemDefinition == null || !itemDefinition.IsTemporary)
            {
                return false;
            }

            _temporaryItemStacks.TryGetValue(itemDefinition, out int currentStacks);
            _temporaryItemStacks[itemDefinition] = currentStacks + 1;
            _temporaryItemStrengths[itemDefinition] = 1f;
            Recalculate();
            return true;
        }

        public void SetTemporaryEffectStrength(RunItemDefinition itemDefinition, float strength)
        {
            if (itemDefinition == null || !_temporaryItemStacks.ContainsKey(itemDefinition))
            {
                return;
            }

            float clampedStrength = Mathf.Clamp01(strength);
            if (_temporaryItemStrengths.TryGetValue(itemDefinition, out float currentStrength)
                && Mathf.Approximately(currentStrength, clampedStrength))
            {
                return;
            }

            _temporaryItemStrengths[itemDefinition] = clampedStrength;
            Recalculate();
        }

        public void RemoveTemporary(RunItemDefinition itemDefinition)
        {
            if (itemDefinition == null || !_temporaryItemStacks.TryGetValue(itemDefinition, out int currentStacks))
            {
                return;
            }

            if (currentStacks <= 1)
            {
                _temporaryItemStacks.Remove(itemDefinition);
                _temporaryItemStrengths.Remove(itemDefinition);
            }
            else
            {
                _temporaryItemStacks[itemDefinition] = currentStacks - 1;
            }

            Recalculate();
        }

        public void ResetRunItems()
        {
            if (!IsInitialized || (_itemStacks.Count == 0 && _temporaryItemStacks.Count == 0))
            {
                return;
            }

            _itemStacks.Clear();
            _temporaryItemStacks.Clear();
            _temporaryItemStrengths.Clear();
            Recalculate();
        }

        private void Recalculate()
        {
            PlayerStatsSnapshot baseStats = _baseStatsDefinition.CreateSnapshot();
            Current = new PlayerStatsSnapshot(
                Mathf.Max(1, Mathf.RoundToInt(CalculateValue(PlayerStatType.AttackDamage, baseStats.AttackDamage))),
                Mathf.Max(MinimumAttackSpeed, CalculateValue(PlayerStatType.AttackSpeed, baseStats.AttacksPerSecond)),
                Mathf.Max(0f, CalculateValue(PlayerStatType.AttackRange, baseStats.AttackRange)),
                Mathf.Max(0f, CalculateValue(PlayerStatType.ProjectileSpeed, baseStats.ProjectileSpeed)),
                Mathf.Max(0f, CalculateValue(PlayerStatType.ForwardSpeed, baseStats.ForwardSpeed)),
                Mathf.Max(0f, CalculateValue(PlayerStatType.VerticalSpeed, baseStats.VerticalSpeed)));
            StatsChanged?.Invoke(Current);
        }

        private float CalculateValue(PlayerStatType statType, float baseValue)
        {
            float flatBonus = 0f;
            float percentBonus = 0f;

            foreach (KeyValuePair<RunItemDefinition, int> itemStack in _itemStacks)
            {
                AddModifierValues(itemStack, statType, ref flatBonus, ref percentBonus);
            }

            foreach (KeyValuePair<RunItemDefinition, int> itemStack in _persistentItemStacks)
            {
                AddModifierValues(itemStack, statType, ref flatBonus, ref percentBonus);
            }

            foreach (KeyValuePair<RunItemDefinition, int> itemStack in _temporaryItemStacks)
            {
                float strength = _temporaryItemStrengths.TryGetValue(itemStack.Key, out float temporaryStrength)
                    ? temporaryStrength
                    : 1f;
                AddModifierValues(itemStack, statType, ref flatBonus, ref percentBonus, strength);
            }

            return (baseValue + flatBonus) * (1f + percentBonus);
        }

        private static void AddModifierValues(
            KeyValuePair<RunItemDefinition, int> itemStack,
            PlayerStatType statType,
            ref float flatBonus,
            ref float percentBonus,
            float strength = 1f)
        {
            IReadOnlyList<RunItemModifier> modifiers = itemStack.Key.Modifiers;
            for (int index = 0; index < modifiers.Count; index++)
            {
                RunItemModifier modifier = modifiers[index];
                if (modifier.StatType != statType)
                {
                    continue;
                }

                if (modifier.Operation == PlayerStatModifierOperation.Flat)
                {
                    flatBonus += modifier.Value * itemStack.Value * strength;
                }
                else
                {
                    percentBonus += modifier.Value * itemStack.Value * strength;
                }
            }
        }
    }
}
