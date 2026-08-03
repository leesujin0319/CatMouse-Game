using CatMouse.Game.Meta;
using CatMouse.Game.Run;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatMouse.Game.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbySceneController : MonoBehaviour
    {
        [Header("Lobby")]
        [SerializeField] private string _gameSceneName = "GameScene";
        [SerializeField] private MetaProgressionCatalog _catalog;
        [SerializeField] private TMP_Text _coinLabel;
        [SerializeField] private Image[] _upgradeIcons;
        [SerializeField] private TMP_Text[] _upgradeNameLabels;
        [SerializeField] private TMP_Text[] _upgradeDescriptionLabels;
        [SerializeField] private TMP_Text[] _upgradeLevelLabels;
        [SerializeField] private TMP_Text[] _upgradeButtonLabels;
        [SerializeField] private Button[] _upgradeButtons;
        [SerializeField] private Image[] _equipmentRowBackgrounds;
        [SerializeField] private Sprite _equipmentSlotSprite;
        [SerializeField] private Sprite _selectedEquipmentSlotSprite;
        [SerializeField] private Image[] _equipmentIcons;
        [SerializeField] private TMP_Text[] _equipmentNameLabels;
        [SerializeField] private TMP_Text[] _equipmentDescriptionLabels;
        [SerializeField] private TMP_Text[] _equipmentButtonLabels;
        [SerializeField] private Button[] _equipmentButtons;
        [SerializeField] private Button[] _equipmentSlotButtons;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _upgradeMenuButton;
        [SerializeField] private Button _equipmentMenuButton;
        [SerializeField] private Button _settingsButton;

        [Header("Overlay Panels")]
        [SerializeField] private GameObject _upgradePanel;
        [SerializeField] private Button _closeUpgradeButton;
        [SerializeField] private GameObject _equipmentPanel;
        [SerializeField] private Button _closeEquipmentButton;

        [Header("Settings")]
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private TMP_Text _masterVolumeLabel;
        [SerializeField] private Button _closeSettingsButton;

        private MetaEquipmentSlot _activeEquipmentSlot = MetaEquipmentSlot.Hat;

        private void Awake()
        {
            MetaProgressionService.Initialize(_catalog);
            MetaProgressionService.ApplySettings();
            BindButtons();
            HideAllPanels();

            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.SetValueWithoutNotify(MetaProgressionService.MasterVolume);
            }

            Refresh();
        }

        private void OnEnable()
        {
            MetaProgressionService.ProgressionChanged += Refresh;
        }

        private void OnDisable()
        {
            MetaProgressionService.ProgressionChanged -= Refresh;
        }

        private void BindButtons()
        {
            for (int index = 0; index < _upgradeButtons.Length; index++)
            {
                MetaUpgradeDefinition definition = GetUpgradeDefinition(index);
                if (_upgradeButtons[index] == null || definition == null)
                {
                    continue;
                }

                string upgradeId = definition.Id;
                _upgradeButtons[index].onClick.AddListener(() => PurchaseUpgrade(upgradeId));
            }

            for (int index = 0; index < _equipmentButtons.Length; index++)
            {
                if (_equipmentButtons[index] == null)
                {
                    continue;
                }

                int buttonIndex = index;
                _equipmentButtons[index].onClick.AddListener(() => EquipVisibleEquipment(buttonIndex));
            }

            for (int index = 0; index < _equipmentSlotButtons.Length; index++)
            {
                if (_equipmentSlotButtons[index] == null)
                {
                    continue;
                }

                MetaEquipmentSlot slot = (MetaEquipmentSlot)index;
                _equipmentSlotButtons[index].onClick.AddListener(() => SelectEquipmentSlot(slot));
            }

            _startButton?.onClick.AddListener(StartGame);
            _upgradeMenuButton?.onClick.AddListener(ShowUpgradePanel);
            _equipmentMenuButton?.onClick.AddListener(ShowEquipmentPanel);
            _settingsButton?.onClick.AddListener(ShowSettings);
            _closeUpgradeButton?.onClick.AddListener(HideAllPanels);
            _closeEquipmentButton?.onClick.AddListener(HideAllPanels);
            _closeSettingsButton?.onClick.AddListener(HideSettings);
            _masterVolumeSlider?.onValueChanged.AddListener(SetMasterVolume);
        }

        private void PurchaseUpgrade(string upgradeId)
        {
            MetaProgressionService.TryPurchaseUpgrade(upgradeId);
        }

        private void EquipVisibleEquipment(int index)
        {
            MetaEquipmentDefinition definition = GetEquipmentDefinition(_activeEquipmentSlot, index);
            if (definition == null)
            {
                return;
            }

            if (MetaProgressionService.IsEquipmentEquipped(definition.Id))
            {
                MetaProgressionService.TryUnequip(definition.Slot);
                return;
            }

            MetaProgressionService.TryEquip(definition.Id);
        }

        private void SelectEquipmentSlot(MetaEquipmentSlot slot)
        {
            _activeEquipmentSlot = slot;
            RefreshEquipment();
        }

        private void StartGame()
        {
            SceneManager.LoadScene(_gameSceneName);
        }

        private void ShowSettings()
        {
            ShowOnlyPanel(_settingsPanel);
        }

        private void HideSettings()
        {
            HideAllPanels();
        }

        private void ShowUpgradePanel()
        {
            ShowOnlyPanel(_upgradePanel);
        }

        private void ShowEquipmentPanel()
        {
            ShowOnlyPanel(_equipmentPanel);
        }

        private void ShowOnlyPanel(GameObject panel)
        {
            HideAllPanels();
            panel?.SetActive(true);
        }

        private void HideAllPanels()
        {
            _upgradePanel?.SetActive(false);
            _equipmentPanel?.SetActive(false);
            _settingsPanel?.SetActive(false);
        }

        private void SetMasterVolume(float volume)
        {
            MetaProgressionService.SetMasterVolume(volume);
        }

        private void Refresh()
        {
            if (_coinLabel != null)
            {
                _coinLabel.text = $"코인 {MetaProgressionService.CoinBalance}";
            }

            RefreshUpgrades();
            RefreshEquipment();
            RefreshMasterVolume();
        }

        private void RefreshUpgrades()
        {
            for (int index = 0; index < _upgradeButtons.Length; index++)
            {
                MetaUpgradeDefinition definition = GetUpgradeDefinition(index);
                RunItemDefinition item = definition?.RunItem;
                RefreshDefinition(item, _upgradeIcons, _upgradeNameLabels, _upgradeDescriptionLabels, index);

                if (definition == null)
                {
                    continue;
                }

                int level = MetaProgressionService.GetUpgradeLevel(definition.Id);
                bool isMaxed = MetaProgressionService.IsUpgradeMaxed(definition.Id);
                if (index < _upgradeLevelLabels.Length && _upgradeLevelLabels[index] != null)
                {
                    _upgradeLevelLabels[index].text = $"Lv. {level} / {definition.MaximumLevel}";
                }

                if (index < _upgradeButtonLabels.Length && _upgradeButtonLabels[index] != null)
                {
                    _upgradeButtonLabels[index].text = isMaxed
                        ? "최대"
                        : $"강화 {MetaProgressionService.GetUpgradeCost(definition.Id)}";
                }

                if (_upgradeButtons[index] != null)
                {
                    _upgradeButtons[index].interactable = !isMaxed
                        && MetaProgressionService.CoinBalance >= MetaProgressionService.GetUpgradeCost(definition.Id);
                }
            }
        }

        private void RefreshEquipment()
        {
            for (int index = 0; index < _equipmentButtons.Length; index++)
            {
                MetaEquipmentDefinition definition = GetEquipmentDefinition(_activeEquipmentSlot, index);
                RunItemDefinition item = definition?.RunItem;

                if (definition == null)
                {
                    continue;
                }

                RefreshDefinition(item, _equipmentIcons, _equipmentNameLabels, _equipmentDescriptionLabels, index);

                bool isEquipped = definition != null && MetaProgressionService.IsEquipmentEquipped(definition.Id);
                if (index < _equipmentButtonLabels.Length && _equipmentButtonLabels[index] != null)
                {
                    _equipmentButtonLabels[index].text = isEquipped ? "장착 해제" : "장착";
                }

                if (_equipmentButtons[index] != null)
                {
                    _equipmentButtons[index].interactable = definition != null;
                }

                if (index < _equipmentRowBackgrounds.Length && _equipmentRowBackgrounds[index] != null)
                {
                    _equipmentRowBackgrounds[index].sprite = isEquipped ? _selectedEquipmentSlotSprite : _equipmentSlotSprite;
                    _equipmentRowBackgrounds[index].color = isEquipped
                        ? Color.white
                        : new Color(1f, 1f, 1f, 0.72f);
                }
            }

            RefreshEquipmentSlotButtons();
        }

        private void RefreshEquipmentSlotButtons()
        {
            for (int index = 0; index < _equipmentSlotButtons.Length; index++)
            {
                Button button = _equipmentSlotButtons[index];
                if (button == null)
                {
                    continue;
                }

                bool isSelected = (MetaEquipmentSlot)index == _activeEquipmentSlot;
                Image image = button.targetGraphic as Image;
                if (image != null)
                {
                    image.color = isSelected ? Color.white : new Color(1f, 1f, 1f, 0.68f);
                }
            }
        }

        private void RefreshMasterVolume()
        {
            if (_masterVolumeLabel != null)
            {
                _masterVolumeLabel.text = $"마스터 볼륨 {Mathf.RoundToInt(MetaProgressionService.MasterVolume * 100f)}%";
            }
        }

        private MetaUpgradeDefinition GetUpgradeDefinition(int index)
        {
            return _catalog != null && index < _catalog.Upgrades.Count ? _catalog.Upgrades[index] : null;
        }

        private MetaEquipmentDefinition GetEquipmentDefinition(MetaEquipmentSlot slot, int index)
        {
            if (_catalog == null || index < 0)
            {
                return null;
            }

            int visibleIndex = 0;
            for (int catalogIndex = 0; catalogIndex < _catalog.Equipment.Count; catalogIndex++)
            {
                MetaEquipmentDefinition definition = _catalog.Equipment[catalogIndex];
                if (definition == null || definition.Slot != slot)
                {
                    continue;
                }

                if (visibleIndex == index)
                {
                    return definition;
                }

                visibleIndex++;
            }

            return null;
        }

        private static void RefreshDefinition(
            RunItemDefinition definition,
            Image[] icons,
            TMP_Text[] nameLabels,
            TMP_Text[] descriptionLabels,
            int index)
        {
            if (definition == null)
            {
                return;
            }

            if (index < icons.Length && icons[index] != null)
            {
                icons[index].sprite = definition.Icon;
            }

            if (index < nameLabels.Length && nameLabels[index] != null)
            {
                nameLabels[index].text = definition.DisplayName;
            }

            if (index < descriptionLabels.Length && descriptionLabels[index] != null)
            {
                descriptionLabels[index].text = definition.Description;
            }
        }
    }
}
