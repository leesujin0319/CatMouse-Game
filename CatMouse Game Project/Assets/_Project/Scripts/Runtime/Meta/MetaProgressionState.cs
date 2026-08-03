using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatMouse.Game.Meta
{
    [Serializable]
    public sealed class MetaProgressionState
    {
        public const int CurrentSchemaVersion = 4;
        public const int InitialCoinBalance = 120;
        private const int CompletedRunHistoryCapacity = 32;

        [SerializeField] private int _schemaVersion;
        // 기존 로컬 저장 데이터와의 호환을 위해 직렬화 필드명은 유지합니다.
        [SerializeField] private int _cheeseBalance;
        [SerializeField] private List<MetaUpgradeLevel> _upgradeLevels = new();
        [SerializeField] private List<MetaEquippedEquipment> _equippedEquipmentBySlot = new();

        // Schema 3 compatibility field. Schema 4 stores one equipped ID per slot.
        [SerializeField] private string _equippedEquipmentId;

        // 스키마 2까지의 로컬 저장값을 한 번만 ID 기반 데이터로 옮기기 위한 필드입니다.
        [SerializeField] private int _attackDamageLevel;
        [SerializeField] private int _attackSpeedLevel;
        [SerializeField] private int _forwardSpeedLevel;
        [SerializeField] private int _equippedEquipment;
        [SerializeField] private float _masterVolume;
        [SerializeField] private List<string> _rewardedRunIds = new();

        public int CoinBalance => _cheeseBalance;
        public int CheeseBalance => CoinBalance;
        public float MasterVolume => _masterVolume;

        public static MetaProgressionState CreateDefault()
        {
            return new MetaProgressionState
            {
                _schemaVersion = CurrentSchemaVersion,
                _cheeseBalance = InitialCoinBalance,
                _masterVolume = 1f,
            };
        }

        public bool EnsureCurrentSchema()
        {
            _upgradeLevels ??= new List<MetaUpgradeLevel>();
            _equippedEquipmentBySlot ??= new List<MetaEquippedEquipment>();
            bool migrated = _schemaVersion < CurrentSchemaVersion;
            if (migrated)
            {
                MigrateLegacyProgression();
                _schemaVersion = CurrentSchemaVersion;
            }

            _cheeseBalance = Mathf.Max(0, _cheeseBalance);
            _upgradeLevels.RemoveAll(level => string.IsNullOrWhiteSpace(level.Id));
            for (int index = 0; index < _upgradeLevels.Count; index++)
            {
                _upgradeLevels[index].Clamp();
            }

            _equippedEquipmentBySlot.RemoveAll(equipment => string.IsNullOrWhiteSpace(equipment.EquipmentId));
            for (int index = 0; index < _equippedEquipmentBySlot.Count; index++)
            {
                _equippedEquipmentBySlot[index].Normalize();
            }

            _masterVolume = Mathf.Clamp01(_masterVolume);
            _rewardedRunIds ??= new List<string>();
            _rewardedRunIds.RemoveAll(string.IsNullOrWhiteSpace);
            if (_rewardedRunIds.Count > CompletedRunHistoryCapacity)
            {
                _rewardedRunIds.RemoveRange(0, _rewardedRunIds.Count - CompletedRunHistoryCapacity);
            }

            return migrated;
        }

        public int GetUpgradeLevel(string upgradeId)
        {
            for (int index = 0; index < _upgradeLevels.Count; index++)
            {
                if (string.Equals(_upgradeLevels[index].Id, upgradeId, StringComparison.Ordinal))
                {
                    return _upgradeLevels[index].Level;
                }
            }

            return 0;
        }

        public void IncreaseUpgradeLevel(string upgradeId)
        {
            for (int index = 0; index < _upgradeLevels.Count; index++)
            {
                if (string.Equals(_upgradeLevels[index].Id, upgradeId, StringComparison.Ordinal))
                {
                    _upgradeLevels[index].Increase();
                    return;
                }
            }

            _upgradeLevels.Add(new MetaUpgradeLevel(upgradeId, 1));
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0 || _cheeseBalance < amount)
            {
                return false;
            }

            _cheeseBalance -= amount;
            return true;
        }

        public bool TrySpendCheese(int amount)
        {
            return TrySpendCoins(amount);
        }

        public bool TryApplyRunReward(string runId, int coinReward)
        {
            if (string.IsNullOrWhiteSpace(runId)
                || coinReward <= 0
                || _rewardedRunIds.Contains(runId))
            {
                return false;
            }

            _rewardedRunIds.Add(runId);
            if (_rewardedRunIds.Count > CompletedRunHistoryCapacity)
            {
                _rewardedRunIds.RemoveAt(0);
            }

            _cheeseBalance += coinReward;
            return true;
        }

        public void SetEquippedEquipment(string equipmentId)
        {
            SetEquippedEquipment(MetaEquipmentSlot.Armor, equipmentId);
        }

        public string GetEquippedEquipmentId(MetaEquipmentSlot slot)
        {
            for (int index = 0; index < _equippedEquipmentBySlot.Count; index++)
            {
                MetaEquippedEquipment equipped = _equippedEquipmentBySlot[index];
                if (equipped.Slot == slot)
                {
                    return equipped.EquipmentId;
                }
            }

            return string.Empty;
        }

        public void SetEquippedEquipment(MetaEquipmentSlot slot, string equipmentId)
        {
            for (int index = 0; index < _equippedEquipmentBySlot.Count; index++)
            {
                if (_equippedEquipmentBySlot[index].Slot == slot)
                {
                    _equippedEquipmentBySlot[index].SetEquipmentId(equipmentId);
                    return;
                }
            }

            _equippedEquipmentBySlot.Add(new MetaEquippedEquipment(slot, equipmentId));
        }

        public bool ClearEquippedEquipment(MetaEquipmentSlot slot)
        {
            for (int index = 0; index < _equippedEquipmentBySlot.Count; index++)
            {
                if (_equippedEquipmentBySlot[index].Slot != slot)
                {
                    continue;
                }

                _equippedEquipmentBySlot.RemoveAt(index);
                return true;
            }

            return false;
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
        }

        private void MigrateLegacyProgression()
        {
            SetUpgradeLevel("attack_damage", _attackDamageLevel);
            SetUpgradeLevel("attack_speed", _attackSpeedLevel);
            SetUpgradeLevel("forward_speed", _forwardSpeedLevel);
            MigrateLegacyEquipment(_equippedEquipmentId);
            MigrateLegacyEquipment(_equippedEquipment switch
            {
                1 => "hardened_acorn",
                2 => "windup_slingshot",
                3 => "long_tail_scope",
                _ => string.Empty,
            });
        }

        private void MigrateLegacyEquipment(string legacyEquipmentId)
        {
            switch (legacyEquipmentId)
            {
                case "hardened_acorn":
                case "acorn_armor":
                    SetEquippedEquipment(MetaEquipmentSlot.Armor, "acorn_armor");
                    break;
                case "windup_slingshot":
                case "windup_shoes":
                    SetEquippedEquipment(MetaEquipmentSlot.Shoes, "windup_shoes");
                    break;
                case "long_tail_scope":
                case "pantry_cap":
                    SetEquippedEquipment(MetaEquipmentSlot.Hat, "pantry_cap");
                    break;
            }
        }

        private void SetUpgradeLevel(string upgradeId, int level)
        {
            if (level <= 0)
            {
                return;
            }

            for (int index = 0; index < _upgradeLevels.Count; index++)
            {
                if (string.Equals(_upgradeLevels[index].Id, upgradeId, StringComparison.Ordinal))
                {
                    _upgradeLevels[index].Set(level);
                    return;
                }
            }

            _upgradeLevels.Add(new MetaUpgradeLevel(upgradeId, level));
        }
    }

    [Serializable]
    public sealed class MetaUpgradeLevel
    {
        [SerializeField] private string _id;
        [SerializeField] private int _level;

        public string Id => _id;
        public int Level => _level;

        public MetaUpgradeLevel(string id, int level)
        {
            _id = id;
            _level = level;
        }

        public void Increase()
        {
            _level++;
        }

        public void Set(int level)
        {
            _level = level;
        }

        public void Clamp()
        {
            _level = Mathf.Max(0, _level);
        }
    }

    [Serializable]
    public sealed class MetaEquippedEquipment
    {
        [SerializeField] private MetaEquipmentSlot _slot;
        [SerializeField] private string _equipmentId;

        public MetaEquipmentSlot Slot => _slot;
        public string EquipmentId => _equipmentId;

        public MetaEquippedEquipment(MetaEquipmentSlot slot, string equipmentId)
        {
            _slot = slot;
            _equipmentId = equipmentId ?? string.Empty;
        }

        public void SetEquipmentId(string equipmentId)
        {
            _equipmentId = equipmentId ?? string.Empty;
        }

        public void Normalize()
        {
            _equipmentId ??= string.Empty;
        }
    }
}
