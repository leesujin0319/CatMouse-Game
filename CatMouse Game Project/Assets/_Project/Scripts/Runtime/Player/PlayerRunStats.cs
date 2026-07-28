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

        private readonly Dictionary<RunItemDefinition, int> _itemStacks = new();

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
            _itemStacks.Clear();
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

        public void ResetRunItems()
        {
            if (!IsInitialized || _itemStacks.Count == 0)
            {
                return;
            }

            _itemStacks.Clear();
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
                        flatBonus += modifier.Value * itemStack.Value;
                    }
                    else
                    {
                        percentBonus += modifier.Value * itemStack.Value;
                    }
                }
            }

            return (baseValue + flatBonus) * (1f + percentBonus);
        }
    }
}
