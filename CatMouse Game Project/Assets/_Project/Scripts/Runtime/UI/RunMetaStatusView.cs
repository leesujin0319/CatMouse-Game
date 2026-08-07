using System;
using CatMouse.Game.Meta;
using CatMouse.Game.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class RunMetaStatusView : MonoBehaviour
    {
        private static readonly MetaEquipmentSlot[] EquipmentSlots =
        {
            MetaEquipmentSlot.Hat,
            MetaEquipmentSlot.Armor,
            MetaEquipmentSlot.Shoes,
        };

        private static readonly PlayerStatType[] DisplayedStats =
        {
            PlayerStatType.AttackDamage,
            PlayerStatType.AttackSpeed,
            PlayerStatType.AttackRange,
            PlayerStatType.ProjectileSpeed,
            PlayerStatType.ForwardSpeed,
            PlayerStatType.VerticalSpeed,
        };

        [Header("Data")]
        [SerializeField] private PlayerRunStats _runStats;
        [SerializeField] private MetaProgressionCatalog _catalog;

        [Header("Panel")]
        [SerializeField] private Button _openButton;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _closeButton;

        [Header("Equipped Items")]
        [SerializeField] private TMP_Text[] _equipmentLabels = Array.Empty<TMP_Text>();

        [Header("Permanent Upgrades")]
        [SerializeField] private TMP_Text[] _upgradeLabels = Array.Empty<TMP_Text>();

        [Header("Current Stats")]
        [SerializeField] private TMP_Text[] _statValueLabels = Array.Empty<TMP_Text>();

        private void Awake()
        {
            _openButton?.onClick.AddListener(Open);
            _closeButton?.onClick.AddListener(Close);
            _panel?.SetActive(false);
        }

        private void OnEnable()
        {
            MetaProgressionService.ProgressionChanged += Refresh;

            if (_runStats != null)
            {
                _runStats.StatsChanged += HandleStatsChanged;
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            MetaProgressionService.ProgressionChanged -= Refresh;

            if (_runStats != null)
            {
                _runStats.StatsChanged -= HandleStatsChanged;
            }
        }

        private void OnDestroy()
        {
            _openButton?.onClick.RemoveListener(Open);
            _closeButton?.onClick.RemoveListener(Close);
        }

        private void Open()
        {
            Refresh();
            _panel?.transform.SetAsLastSibling();
            _panel?.SetActive(true);
        }

        private void Close()
        {
            _panel?.SetActive(false);
        }

        private void HandleStatsChanged(PlayerStatsSnapshot _)
        {
            RefreshStats();
        }

        private void Refresh()
        {
            MetaProgressionService.Initialize(_catalog);
            RefreshEquipment();
            RefreshUpgrades();
            RefreshStats();
        }

        private void RefreshEquipment()
        {
            MetaProgressionLoadout loadout = MetaProgressionService.GetLoadout();
            for (int index = 0; index < EquipmentSlots.Length; index++)
            {
                SetLabel(
                    _equipmentLabels,
                    index,
                    loadout.TryGetEquippedItem(EquipmentSlots[index], out MetaEquippedLoadoutItem equippedItem)
                        && equippedItem.ItemDefinition != null
                        ? $"{GetEquipmentSlotName(EquipmentSlots[index])}: {equippedItem.ItemDefinition.DisplayName}"
                        : $"{GetEquipmentSlotName(EquipmentSlots[index])}: 미장착");
            }
        }

        private void RefreshUpgrades()
        {
            int upgradeCount = _catalog != null ? _catalog.Upgrades.Count : 0;
            for (int index = 0; index < _upgradeLabels.Length; index++)
            {
                MetaUpgradeDefinition definition = index < upgradeCount ? _catalog.Upgrades[index] : null;
                if (definition?.RunItem == null)
                {
                    SetLabel(_upgradeLabels, index, string.Empty);
                    continue;
                }

                int level = MetaProgressionService.GetUpgradeLevel(definition.Id);
                SetLabel(_upgradeLabels, index, $"{definition.RunItem.DisplayName}  Lv. {level} / {definition.MaximumLevel}");
            }
        }

        private void RefreshStats()
        {
            if (_runStats == null || !_runStats.IsInitialized)
            {
                return;
            }

            for (int index = 0; index < DisplayedStats.Length; index++)
            {
                PlayerStatType statType = DisplayedStats[index];
                SetLabel(
                    _statValueLabels,
                    index,
                    $"{GetStatName(statType)}  {FormatValue(statType, GetCurrentValue(statType))}");
            }
        }

        private float GetCurrentValue(PlayerStatType statType)
        {
            PlayerStatsSnapshot current = _runStats.Current;
            return statType switch
            {
                PlayerStatType.AttackDamage => current.AttackDamage,
                PlayerStatType.AttackSpeed => current.AttacksPerSecond,
                PlayerStatType.AttackRange => current.AttackRange,
                PlayerStatType.ProjectileSpeed => current.ProjectileSpeed,
                PlayerStatType.ForwardSpeed => current.ForwardSpeed,
                PlayerStatType.VerticalSpeed => current.VerticalSpeed,
                _ => 0f,
            };
        }

        private static string GetEquipmentSlotName(MetaEquipmentSlot slot)
        {
            return slot switch
            {
                MetaEquipmentSlot.Hat => "모자",
                MetaEquipmentSlot.Armor => "갑옷",
                MetaEquipmentSlot.Shoes => "신발",
                _ => string.Empty,
            };
        }

        private static string GetStatName(PlayerStatType statType)
        {
            return statType switch
            {
                PlayerStatType.AttackDamage => "공격력",
                PlayerStatType.AttackSpeed => "공격 속도",
                PlayerStatType.AttackRange => "공격 범위",
                PlayerStatType.ProjectileSpeed => "투사체 속도",
                PlayerStatType.ForwardSpeed => "전진 속도",
                PlayerStatType.VerticalSpeed => "상하 이동 속도",
                _ => string.Empty,
            };
        }

        private static string FormatValue(PlayerStatType statType, float value)
        {
            return statType == PlayerStatType.AttackDamage
                ? Mathf.RoundToInt(value).ToString()
                : value.ToString("0.##");
        }

        private static void SetLabel(TMP_Text[] labels, int index, string value)
        {
            if (index >= 0 && index < labels.Length && labels[index] != null)
            {
                labels[index].text = value;
            }
        }
    }
}
