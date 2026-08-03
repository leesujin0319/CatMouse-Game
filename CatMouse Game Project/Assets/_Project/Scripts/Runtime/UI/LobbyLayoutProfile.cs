using UnityEngine;

namespace CatMouse.Game.UI
{
    public sealed class LobbyLayoutProfile : ScriptableObject
    {
        public const string AssetPath = "Assets/_Project/Data/UI/LobbyLayoutProfile.asset";

        [Header("Grid")]
        [SerializeField, Min(1)] private int _gridUnit;
        [SerializeField] private Vector2 _referenceResolution;

        [Header("Lobby Stage")]
        [SerializeField] private Vector2 _currencyChipSize;
        [SerializeField] private Vector2 _currencyChipPosition;
        [SerializeField] private Vector2 _playerImageSize;
        [SerializeField] private Vector2 _playerImagePosition;

        [Header("Navigation")]
        [SerializeField] private Vector2 _menuDockSize;
        [SerializeField] private Vector2 _menuDockPosition;
        [SerializeField] private Vector2 _menuButtonSize;
        [SerializeField] private Vector2 _upgradeButtonPosition;
        [SerializeField] private Vector2 _equipmentButtonPosition;
        [SerializeField] private Vector2 _settingsButtonPosition;
        [SerializeField] private Vector2 _navigationIconSize;
        [SerializeField] private float _navigationIconCenterX;
        [SerializeField] private Vector2 _navigationContentInsets;

        [Header("Start Button")]
        [SerializeField] private Vector2 _startButtonSize;
        [SerializeField] private Vector2 _startButtonPosition;
        [SerializeField] private Vector2 _startIconSize;
        [SerializeField] private float _startIconCenterX;
        [SerializeField] private Vector2 _startContentInsets;

        [Header("Side Panel")]
        [SerializeField] private Vector2 _sidePanelCardSize;
        [SerializeField] private Vector2 _sidePanelCardPosition;
        [SerializeField] private Vector2 _sidePanelRowSize;
        [SerializeField] private float _sidePanelFirstRowTopOffset;
        [SerializeField] private float _sidePanelRowPitch;

        [Header("Side Panel Content")]
        [SerializeField] private Vector2 _panelHeaderSize;
        [SerializeField] private Vector2 _panelHeaderPosition;
        [SerializeField] private Vector2 _panelSecondaryTextSize;
        [SerializeField] private Vector2 _panelSecondaryTextPosition;
        [SerializeField] private Vector2 _sidePanelCloseButtonSize;
        [SerializeField] private Vector2 _sidePanelCloseButtonPosition;
        [SerializeField] private Vector2 _metaItemIconSize;
        [SerializeField] private Vector2 _metaItemIconPosition;
        [SerializeField] private Vector2 _metaItemNameSize;
        [SerializeField] private Vector2 _metaItemNamePosition;
        [SerializeField] private Vector2 _metaItemDescriptionSize;
        [SerializeField] private Vector2 _metaItemDescriptionPosition;
        [SerializeField] private Vector2 _upgradeLevelSize;
        [SerializeField] private Vector2 _upgradeLevelPosition;
        [SerializeField] private Vector2 _upgradeRowButtonSize;
        [SerializeField] private Vector2 _upgradeRowButtonPosition;
        [SerializeField] private Vector2 _equipmentRowButtonSize;
        [SerializeField] private Vector2 _equipmentRowButtonPosition;
        [SerializeField] private Vector2 _settingsSliderSize;
        [SerializeField] private Vector2 _settingsSliderPosition;
        [SerializeField] private Vector2 _settingsVolumeLabelSize;
        [SerializeField] private Vector2 _settingsVolumeLabelPosition;

        public int GridUnit => _gridUnit;
        public Vector2 ReferenceResolution => _referenceResolution;
        public Vector2 CurrencyChipSize => _currencyChipSize;
        public Vector2 CurrencyChipPosition => _currencyChipPosition;
        public Vector2 PlayerImageSize => _playerImageSize;
        public Vector2 PlayerImagePosition => _playerImagePosition;
        public Vector2 MenuDockSize => _menuDockSize;
        public Vector2 MenuDockPosition => _menuDockPosition;
        public Vector2 MenuButtonSize => _menuButtonSize;
        public Vector2 UpgradeButtonPosition => _upgradeButtonPosition;
        public Vector2 EquipmentButtonPosition => _equipmentButtonPosition;
        public Vector2 SettingsButtonPosition => _settingsButtonPosition;
        public Vector2 NavigationIconSize => _navigationIconSize;
        public float NavigationIconCenterX => _navigationIconCenterX;
        public Vector2 NavigationContentInsets => _navigationContentInsets;
        public Vector2 StartButtonSize => _startButtonSize;
        public Vector2 StartButtonPosition => _startButtonPosition;
        public Vector2 StartIconSize => _startIconSize;
        public float StartIconCenterX => _startIconCenterX;
        public Vector2 StartContentInsets => _startContentInsets;
        public Vector2 SidePanelCardSize => _sidePanelCardSize;
        public Vector2 SidePanelCardPosition => _sidePanelCardPosition;
        public Vector2 SidePanelRowSize => _sidePanelRowSize;
        public float SidePanelFirstRowTopOffset => _sidePanelFirstRowTopOffset;
        public float SidePanelRowPitch => _sidePanelRowPitch;
        public Vector2 PanelHeaderSize => _panelHeaderSize;
        public Vector2 PanelHeaderPosition => _panelHeaderPosition;
        public Vector2 PanelSecondaryTextSize => _panelSecondaryTextSize;
        public Vector2 PanelSecondaryTextPosition => _panelSecondaryTextPosition;
        public Vector2 SidePanelCloseButtonSize => _sidePanelCloseButtonSize;
        public Vector2 SidePanelCloseButtonPosition => _sidePanelCloseButtonPosition;
        public Vector2 MetaItemIconSize => _metaItemIconSize;
        public Vector2 MetaItemIconPosition => _metaItemIconPosition;
        public Vector2 MetaItemNameSize => _metaItemNameSize;
        public Vector2 MetaItemNamePosition => _metaItemNamePosition;
        public Vector2 MetaItemDescriptionSize => _metaItemDescriptionSize;
        public Vector2 MetaItemDescriptionPosition => _metaItemDescriptionPosition;
        public Vector2 UpgradeLevelSize => _upgradeLevelSize;
        public Vector2 UpgradeLevelPosition => _upgradeLevelPosition;
        public Vector2 UpgradeRowButtonSize => _upgradeRowButtonSize;
        public Vector2 UpgradeRowButtonPosition => _upgradeRowButtonPosition;
        public Vector2 EquipmentRowButtonSize => _equipmentRowButtonSize;
        public Vector2 EquipmentRowButtonPosition => _equipmentRowButtonPosition;
        public Vector2 SettingsSliderSize => _settingsSliderSize;
        public Vector2 SettingsSliderPosition => _settingsSliderPosition;
        public Vector2 SettingsVolumeLabelSize => _settingsVolumeLabelSize;
        public Vector2 SettingsVolumeLabelPosition => _settingsVolumeLabelPosition;

        private void OnValidate()
        {
            if (_gridUnit <= 0)
            {
                return;
            }

            _referenceResolution = Snap(_referenceResolution);
            _currencyChipSize = Snap(_currencyChipSize);
            _currencyChipPosition = Snap(_currencyChipPosition);
            _playerImageSize = Snap(_playerImageSize);
            _playerImagePosition = Snap(_playerImagePosition);
            _menuDockSize = Snap(_menuDockSize);
            _menuDockPosition = Snap(_menuDockPosition);
            _menuButtonSize = Snap(_menuButtonSize);
            _upgradeButtonPosition = Snap(_upgradeButtonPosition);
            _equipmentButtonPosition = Snap(_equipmentButtonPosition);
            _settingsButtonPosition = Snap(_settingsButtonPosition);
            _navigationIconSize = Snap(_navigationIconSize);
            _navigationIconCenterX = Snap(_navigationIconCenterX);
            _navigationContentInsets = Snap(_navigationContentInsets);
            _startButtonSize = Snap(_startButtonSize);
            _startButtonPosition = Snap(_startButtonPosition);
            _startIconSize = Snap(_startIconSize);
            _startIconCenterX = Snap(_startIconCenterX);
            _startContentInsets = Snap(_startContentInsets);
            _sidePanelCardSize = Snap(_sidePanelCardSize);
            _sidePanelCardPosition = Snap(_sidePanelCardPosition);
            _sidePanelRowSize = Snap(_sidePanelRowSize);
            _sidePanelFirstRowTopOffset = Snap(_sidePanelFirstRowTopOffset);
            _sidePanelRowPitch = Snap(_sidePanelRowPitch);
            _panelHeaderSize = Snap(_panelHeaderSize);
            _panelHeaderPosition = Snap(_panelHeaderPosition);
            _panelSecondaryTextSize = Snap(_panelSecondaryTextSize);
            _panelSecondaryTextPosition = Snap(_panelSecondaryTextPosition);
            _sidePanelCloseButtonSize = Snap(_sidePanelCloseButtonSize);
            _sidePanelCloseButtonPosition = Snap(_sidePanelCloseButtonPosition);
            _metaItemIconSize = Snap(_metaItemIconSize);
            _metaItemIconPosition = Snap(_metaItemIconPosition);
            _metaItemNameSize = Snap(_metaItemNameSize);
            _metaItemNamePosition = Snap(_metaItemNamePosition);
            _metaItemDescriptionSize = Snap(_metaItemDescriptionSize);
            _metaItemDescriptionPosition = Snap(_metaItemDescriptionPosition);
            _upgradeLevelSize = Snap(_upgradeLevelSize);
            _upgradeLevelPosition = Snap(_upgradeLevelPosition);
            _upgradeRowButtonSize = Snap(_upgradeRowButtonSize);
            _upgradeRowButtonPosition = Snap(_upgradeRowButtonPosition);
            _equipmentRowButtonSize = Snap(_equipmentRowButtonSize);
            _equipmentRowButtonPosition = Snap(_equipmentRowButtonPosition);
            _settingsSliderSize = Snap(_settingsSliderSize);
            _settingsSliderPosition = Snap(_settingsSliderPosition);
            _settingsVolumeLabelSize = Snap(_settingsVolumeLabelSize);
            _settingsVolumeLabelPosition = Snap(_settingsVolumeLabelPosition);
        }

        private Vector2 Snap(Vector2 value)
        {
            return new Vector2(Snap(value.x), Snap(value.y));
        }

        private float Snap(float value)
        {
            return Mathf.Round(value / _gridUnit) * _gridUnit;
        }
    }
}
