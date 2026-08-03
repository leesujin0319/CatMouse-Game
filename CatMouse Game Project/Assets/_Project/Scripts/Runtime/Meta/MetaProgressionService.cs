using System;
using System.Collections.Generic;
using CatMouse.Game.Run;
using UnityEngine;

namespace CatMouse.Game.Meta
{
    public static class MetaProgressionService
    {
        private const string SaveKey = "CatMouse.MetaProgression.v1";

        private static readonly ISaveStore SaveStore = new PlayerPrefsSaveStore();

        private static MetaProgressionCatalog _catalog;
        private static MetaProgressionState _state;

        public static event Action ProgressionChanged;

        public static int CoinBalance => GetState().CoinBalance;
        public static int CheeseBalance => CoinBalance;
        public static float MasterVolume => GetState().MasterVolume;

        public static void Initialize(MetaProgressionCatalog catalog)
        {
            if (catalog != null)
            {
                _catalog = catalog;
            }
        }

        public static int GetUpgradeLevel(string upgradeId)
        {
            return GetState().GetUpgradeLevel(upgradeId);
        }

        public static int GetUpgradeCost(string upgradeId)
        {
            MetaUpgradeDefinition definition = GetUpgradeDefinition(upgradeId);
            return definition == null ? 0 : definition.GetCheeseCost(GetUpgradeLevel(upgradeId));
        }

        public static bool IsUpgradeMaxed(string upgradeId)
        {
            MetaUpgradeDefinition definition = GetUpgradeDefinition(upgradeId);
            return definition == null || GetUpgradeLevel(upgradeId) >= definition.MaximumLevel;
        }

        public static bool TryPurchaseUpgrade(string upgradeId)
        {
            MetaUpgradeDefinition definition = GetUpgradeDefinition(upgradeId);
            if (definition == null || IsUpgradeMaxed(upgradeId))
            {
                return false;
            }

            MetaProgressionState state = GetState();
            if (!state.TrySpendCoins(GetUpgradeCost(upgradeId)))
            {
                return false;
            }

            state.IncreaseUpgradeLevel(upgradeId);
            SaveAndNotify();
            return true;
        }

        public static bool TryEquip(string equipmentId)
        {
            MetaEquipmentDefinition equipment = _catalog == null ? null : _catalog.FindEquipment(equipmentId);
            if (equipment == null)
            {
                return false;
            }

            MetaProgressionState state = GetState();
            state.SetEquippedEquipment(equipment.Slot, equipment.Id);
            SaveAndNotify();
            return true;
        }

        public static bool TryUnequip(MetaEquipmentSlot slot)
        {
            MetaProgressionState state = GetState();
            if (!state.ClearEquippedEquipment(slot))
            {
                return false;
            }

            SaveAndNotify();
            return true;
        }

        public static string GetEquippedEquipmentId(MetaEquipmentSlot slot)
        {
            return GetState().GetEquippedEquipmentId(slot);
        }

        public static bool IsEquipmentEquipped(string equipmentId)
        {
            MetaEquipmentDefinition equipment = _catalog == null ? null : _catalog.FindEquipment(equipmentId);
            return equipment != null
                && string.Equals(
                    GetEquippedEquipmentId(equipment.Slot),
                    equipment.Id,
                    System.StringComparison.Ordinal);
        }

        public static bool TryApplyRunResult(RunResult runResult)
        {
            MetaProgressionState state = GetState();
            if (!state.TryApplyRunReward(runResult.RunId, runResult.CoinReward))
            {
                return false;
            }

            SaveAndNotify();
            return true;
        }

        public static void SetMasterVolume(float volume)
        {
            MetaProgressionState state = GetState();
            state.SetMasterVolume(volume);
            AudioListener.volume = state.MasterVolume;
            SaveAndNotify();
        }

        public static void ApplySettings()
        {
            AudioListener.volume = GetState().MasterVolume;
        }

        public static MetaProgressionLoadout GetLoadout()
        {
            if (_catalog == null)
            {
                return new MetaProgressionLoadout(null, null);
            }

            MetaProgressionState state = GetState();
            List<MetaRunItemStack> persistentItemStacks = new();
            List<MetaEquippedLoadoutItem> equippedItems = new();

            foreach (MetaUpgradeDefinition upgrade in _catalog.Upgrades)
            {
                if (upgrade?.RunItem == null)
                {
                    continue;
                }

                int stackCount = state.GetUpgradeLevel(upgrade.Id);
                if (stackCount > 0)
                {
                    persistentItemStacks.Add(new MetaRunItemStack(upgrade.RunItem, stackCount));
                }
            }

            foreach (MetaEquipmentDefinition equipment in _catalog.Equipment)
            {
                if (equipment?.RunItem == null
                    || !string.Equals(
                        state.GetEquippedEquipmentId(equipment.Slot),
                        equipment.Id,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                persistentItemStacks.Add(new MetaRunItemStack(equipment.RunItem, 1));
                equippedItems.Add(new MetaEquippedLoadoutItem(
                    equipment.Slot,
                    equipment.Id,
                    equipment.RunItem));
            }

            return new MetaProgressionLoadout(
                persistentItemStacks.ToArray(),
                equippedItems.ToArray());
        }

        private static MetaProgressionState GetState()
        {
            if (_state != null)
            {
                return _state;
            }

            if (SaveStore.TryLoad(SaveKey, out string payload))
            {
                _state = JsonUtility.FromJson<MetaProgressionState>(payload);
            }

            _state ??= MetaProgressionState.CreateDefault();
            if (_state.EnsureCurrentSchema())
            {
                SaveStore.Save(SaveKey, JsonUtility.ToJson(_state));
            }

            return _state;
        }

        private static void Save()
        {
            SaveStore.Save(SaveKey, JsonUtility.ToJson(GetState()));
        }

        private static void SaveAndNotify()
        {
            Save();
            ProgressionChanged?.Invoke();
        }

        private static MetaUpgradeDefinition GetUpgradeDefinition(string upgradeId)
        {
            return _catalog == null ? null : _catalog.FindUpgrade(upgradeId);
        }
    }
}
