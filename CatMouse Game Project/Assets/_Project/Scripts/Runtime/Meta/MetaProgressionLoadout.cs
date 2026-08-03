using System;
using System.Collections.Generic;
using CatMouse.Game.Run;

namespace CatMouse.Game.Meta
{
    public readonly struct MetaRunItemStack
    {
        public MetaRunItemStack(RunItemDefinition itemDefinition, int stackCount)
        {
            ItemDefinition = itemDefinition;
            StackCount = stackCount;
        }

        public RunItemDefinition ItemDefinition { get; }
        public int StackCount { get; }
    }

    public readonly struct MetaEquippedLoadoutItem
    {
        public MetaEquippedLoadoutItem(
            MetaEquipmentSlot slot,
            string equipmentId,
            RunItemDefinition itemDefinition)
        {
            Slot = slot;
            EquipmentId = equipmentId;
            ItemDefinition = itemDefinition;
        }

        public MetaEquipmentSlot Slot { get; }
        public string EquipmentId { get; }
        public RunItemDefinition ItemDefinition { get; }
    }

    public sealed class MetaProgressionLoadout
    {
        private readonly MetaRunItemStack[] _persistentItemStacks;
        private readonly MetaEquippedLoadoutItem[] _equippedItems;

        public MetaProgressionLoadout(
            MetaRunItemStack[] persistentItemStacks,
            MetaEquippedLoadoutItem[] equippedItems)
        {
            _persistentItemStacks = persistentItemStacks ?? Array.Empty<MetaRunItemStack>();
            _equippedItems = equippedItems ?? Array.Empty<MetaEquippedLoadoutItem>();
        }

        public IReadOnlyList<MetaRunItemStack> PersistentItemStacks => _persistentItemStacks;
        public IReadOnlyList<MetaEquippedLoadoutItem> EquippedItems => _equippedItems;

        public bool TryGetEquippedItem(MetaEquipmentSlot slot, out MetaEquippedLoadoutItem item)
        {
            for (int index = 0; index < _equippedItems.Length; index++)
            {
                if (_equippedItems[index].Slot == slot)
                {
                    item = _equippedItems[index];
                    return true;
                }
            }

            item = default;
            return false;
        }
    }
}
