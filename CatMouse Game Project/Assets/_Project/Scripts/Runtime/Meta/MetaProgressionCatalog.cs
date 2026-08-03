using System;
using System.Collections.Generic;
using CatMouse.Game.Run;
using UnityEngine;

namespace CatMouse.Game.Meta
{
    [Serializable]
    public sealed class MetaUpgradeDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private RunItemDefinition _runItem;
        [SerializeField, Min(1)] private int _maximumLevel = 1;
        [SerializeField, Min(1)] private int _baseCheeseCost = 1;
        [SerializeField, Min(0)] private int _cheeseCostIncrease;

        public string Id => _id;
        public RunItemDefinition RunItem => _runItem;
        public int MaximumLevel => _maximumLevel;

        public int GetCheeseCost(int currentLevel)
        {
            return _baseCheeseCost + _cheeseCostIncrease * Mathf.Max(0, currentLevel);
        }
    }

    [Serializable]
    public sealed class MetaEquipmentDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private MetaEquipmentSlot _slot;
        [SerializeField] private RunItemDefinition _runItem;

        public string Id => _id;
        public MetaEquipmentSlot Slot => _slot;
        public RunItemDefinition RunItem => _runItem;
    }

    [CreateAssetMenu(
        fileName = "MetaProgressionCatalog",
        menuName = "CatMouse/Meta/Progression Catalog")]
    public sealed class MetaProgressionCatalog : ScriptableObject
    {
        [SerializeField] private MetaUpgradeDefinition[] _upgrades = Array.Empty<MetaUpgradeDefinition>();
        [SerializeField] private MetaEquipmentDefinition[] _equipment = Array.Empty<MetaEquipmentDefinition>();

        public IReadOnlyList<MetaUpgradeDefinition> Upgrades => _upgrades;
        public IReadOnlyList<MetaEquipmentDefinition> Equipment => _equipment;

        public MetaUpgradeDefinition FindUpgrade(string id)
        {
            for (int index = 0; index < _upgrades.Length; index++)
            {
                MetaUpgradeDefinition definition = _upgrades[index];
                if (definition != null && string.Equals(definition.Id, id, StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            return null;
        }

        public MetaEquipmentDefinition FindEquipment(string id)
        {
            for (int index = 0; index < _equipment.Length; index++)
            {
                MetaEquipmentDefinition definition = _equipment[index];
                if (definition != null && string.Equals(definition.Id, id, StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            return null;
        }

        public MetaEquipmentDefinition FindEquipment(MetaEquipmentSlot slot, string id)
        {
            MetaEquipmentDefinition definition = FindEquipment(id);
            return definition != null && definition.Slot == slot ? definition : null;
        }

        private void OnValidate()
        {
            _upgrades ??= Array.Empty<MetaUpgradeDefinition>();
            _equipment ??= Array.Empty<MetaEquipmentDefinition>();
        }

    }
}
