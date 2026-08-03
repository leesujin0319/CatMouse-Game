#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using CatMouse.Game.Meta;
using CatMouse.Game.Player;
using CatMouse.Game.Run;
using CatMouse.Game.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatMouse.Game.Editor
{
    public static class LobbySceneInstaller
    {
        private const string LobbyScenePath = "Assets/_Project/Scenes/Lobby/LobbyScene.unity";
        private const string GameScenePath = "Assets/_Project/Scenes/Gameplay/GameScene.unity";
        private const string MetaItemFolderPath = "Assets/_Project/Data/Meta/Items";
        private const string MetaCatalogAssetPath = "Assets/_Project/Data/Meta/MetaProgressionCatalog.asset";
        private const string LobbyUiInputActionsAssetPath = "Assets/_Project/Data/Meta/LobbyUiInputActions.asset";
        private const string LobbyUiAssetRootPath = "Assets/_Project/Art/UI/Lobby";
        private const string LobbyBackgroundSpritePath = LobbyUiAssetRootPath + "/Backgrounds/PantryLobby_Background_v2.png";
        private const string DrawerPanelSpritePath = LobbyUiAssetRootPath + "/Panels/LobbyPanel_SlideDrawer_v1.png";
        private const string MetaSlotSpritePath = LobbyUiAssetRootPath + "/Panels/LobbyPanel_MetaSlot_v1.png";
        private const string MetaSlotSelectedSpritePath = LobbyUiAssetRootPath + "/Panels/LobbyPanel_MetaSlotSelected_v1.png";
        private const string CurrencyChipSpritePath = LobbyUiAssetRootPath + "/Panels/LobbyPanel_CurrencyChip_v1.png";
        private const string PrimaryButtonSpritePath = LobbyUiAssetRootPath + "/Buttons/LobbyButton_Primary_v1.png";
        private const string SecondaryButtonSpritePath = LobbyUiAssetRootPath + "/Buttons/LobbyButton_Secondary_v1.png";
        private const string NavigationButtonSpritePath = LobbyUiAssetRootPath + "/Buttons/LobbyNavButton_Cropped_v1.png";
        private const string StartButtonSpritePath = LobbyUiAssetRootPath + "/Buttons/LobbyStartButton_Cropped_v1.png";
        private const string LobbyUpgradeIconSpritePath = LobbyUiAssetRootPath + "/Icons/LobbyIcon_Upgrade_v1.png";
        private const string LobbyEquipmentIconSpritePath = LobbyUiAssetRootPath + "/Icons/LobbyIcon_Equipment_v1.png";
        private const string LobbySettingsIconSpritePath = LobbyUiAssetRootPath + "/Icons/LobbyIcon_Settings_v1.png";
        private const string LobbyStartIconSpritePath = LobbyUiAssetRootPath + "/Icons/LobbyIcon_Start_v1.png";
        private const string MetaAttackDamageIconSpritePath = LobbyUiAssetRootPath + "/Icons/Progression/LobbyMetaIcon_AttackDamage_v1.png";
        private const string MetaAttackSpeedIconSpritePath = LobbyUiAssetRootPath + "/Icons/Progression/LobbyMetaIcon_AttackSpeed_v1.png";
        private const string MetaForwardSpeedIconSpritePath = LobbyUiAssetRootPath + "/Icons/Progression/LobbyMetaIcon_ForwardSpeed_v1.png";
        private const string MetaHardenedAcornIconSpritePath = LobbyUiAssetRootPath + "/Icons/Progression/LobbyMetaIcon_AcornArmor_v1.png";
        private const string MetaWindupSlingshotIconSpritePath = LobbyUiAssetRootPath + "/Icons/Progression/LobbyMetaIcon_WindupShoes_v1.png";
        private const string MetaLongTailScopeIconSpritePath = LobbyUiAssetRootPath + "/Icons/Progression/LobbyMetaIcon_PantryCap_v1.png";
        private const string LobbyFontPath = "Assets/_Project/Art/UI/Fonts/NEXONLv1GothicRegular.ttf";
        private const string LobbyBoldFontPath = "Assets/_Project/Art/UI/Fonts/NEXONLv1GothicBold.ttf";
        private const string LobbyDisplayFontPath = "Assets/_Project/Art/UI/Fonts/NEXONKartGothicExtraBold.ttf";
        private const string LobbyTmpFontFolderPath = "Assets/_Project/Art/UI/Fonts/TMP";
        private const string LobbyTmpFontAssetPath = LobbyTmpFontFolderPath + "/NEXONLv1GothicRegular_TMP.asset";
        private const string LobbyTmpBoldFontAssetPath = LobbyTmpFontFolderPath + "/NEXONLv1GothicBold_TMP.asset";
        private const string LobbyTmpDisplayFontAssetPath = LobbyTmpFontFolderPath + "/NEXONKartGothicExtraBold_TMP.asset";
        private const string LobbyFontCharacters = "코인영구강화장비착용설정게임시작도토리단련앞발질주탐험모자갑옷태엽신발단단한껍질수납주머니걸음공격력속도전진획득한다음런의기본능력치를올립니다최대중닫기마스터볼륨없음배경캐릭터무대작업공통영역콘텐츠행";
        private const string LobbyModulePrefabFolderPath = "Assets/_Project/Prefabs/UI/Lobby/Modules";
        private const string LobbyStagePrefabPath = LobbyModulePrefabFolderPath + "/Stage/LobbyStage.prefab";
        private const string LobbyNavigationPrefabPath = LobbyModulePrefabFolderPath + "/Navigation/LobbyNavigation.prefab";
        private const string LobbyUpgradePanelPrefabPath = LobbyModulePrefabFolderPath + "/Panels/LobbyPanel_Upgrade.prefab";
        private const string LobbyEquipmentPanelPrefabPath = LobbyModulePrefabFolderPath + "/Panels/LobbyPanel_Equipment.prefab";
        private const string LobbySettingsPanelPrefabPath = LobbyModulePrefabFolderPath + "/Panels/LobbyPanel_Settings.prefab";
        private const string LobbyUpgradeSlotPrefabPath = "Assets/_Project/Prefabs/UI/Lobby/Components/Slots/LobbyUpgradeSlot.prefab";
        private const string LobbyEquipmentSlotPrefabPath = "Assets/_Project/Prefabs/UI/Lobby/Components/Slots/LobbyEquipmentSlot.prefab";
        private const string LobbyStageModuleName = "LobbyStage";
        private const string LobbyNavigationModuleName = "LobbyNavigation";
        private const string LobbyUpgradePanelModuleName = "UpgradePanel";
        private const string LobbyEquipmentPanelModuleName = "EquipmentPanel";
        private const string LobbySettingsPanelModuleName = "SettingsPanel";
        private const string LobbyStageGroupName = "Stage";
        private const string LobbyNavigationGroupName = "Navigation";
        private const string LobbyPanelsGroupName = "Panels";

        private static readonly Vector4 DrawerPanelSpriteBorder = new(32f, 32f, 32f, 32f);
        private static readonly Vector4 SlotSpriteBorder = new(28f, 28f, 28f, 28f);
        private static readonly Vector4 ButtonSpriteBorder = new(72f, 60f, 72f, 60f);
        private static readonly Vector4 CurrencyChipSpriteBorder = new(20f, 20f, 20f, 20f);
        private static readonly Vector4 NavigationButtonSpriteBorder = new(200f, 110f, 200f, 110f);

        private static TMP_FontAsset _lobbyFont;
        private static TMP_FontAsset _lobbyBoldFont;
        private static TMP_FontAsset _lobbyDisplayFont;

        [MenuItem("CatMouse/Install/Create Lobby Scene")]
        public static void Install()
        {
            MetaRunItems items = CreateMetaRunItems();
            MetaProgressionCatalog catalog = CreateMetaProgressionCatalog(items);
            ConfigureGameScene(catalog);
            CreateLobbyScene(catalog);
            EnsureLobbyIsFirstBuildScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[LobbySceneInstaller] 로비 씬과 메타 성장 연결을 구성했습니다.");
        }

        [MenuItem("CatMouse/Install/Refresh Lobby UI")]
        public static void RefreshLobbyUi()
        {
            MetaProgressionCatalog catalog = CreateMetaProgressionCatalog(CreateMetaRunItems());
            CreateLobbyScene(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("[LobbySceneInstaller] 로비 강화·장비 UI를 갱신했습니다.");
        }

        [MenuItem("CatMouse/Install/Sync Lobby UI With Meta Catalog")]
        public static void SyncLobbyUiWithMetaCatalog()
        {
            MetaProgressionCatalog catalog = CreateMetaProgressionCatalog(CreateMetaRunItems());
            SynchronizeLobbyPanelSlots(catalog);
            AssetDatabase.SaveAssets();

            if (TryGetLobbySceneContext(out _, out _, out _))
            {
                RefreshLobbyUpgradePanel();
                RefreshLobbyEquipmentPanel();
            }
        }

        [MenuItem("CatMouse/Install/Refresh Lobby/Stage")]
        public static void RefreshLobbyStage()
        {
            if (!TryGetLobbySceneContext(out Scene scene, out Transform canvasTransform, out LobbySceneController controller))
            {
                return;
            }

            Transform stageGroup = GetOrCreateLobbyGroup(canvasTransform, LobbyStageGroupName, 0);
            RemoveLobbyModule(stageGroup, LobbyStageModuleName);
            GameObject stage = InstantiateLobbyModule(LobbyStagePrefabPath, stageGroup);
            stage.transform.SetSiblingIndex(0);
            TMP_Text coinLabel = RequireComponent<TMP_Text>(stage.transform, "CurrencyChip/CoinLabel");
            Button startButton = RequireComponent<Button>(stage.transform, "StartButton");
            ConfigureLobbyStageController(controller, coinLabel, startButton);
            SaveLobbyScene(scene);
        }

        [MenuItem("CatMouse/Install/Refresh Lobby/Navigation")]
        public static void RefreshLobbyNavigation()
        {
            if (!TryGetLobbySceneContext(out Scene scene, out Transform canvasTransform, out LobbySceneController controller))
            {
                return;
            }

            Transform navigationGroup = GetOrCreateLobbyGroup(canvasTransform, LobbyNavigationGroupName, 1);
            RemoveLobbyModule(navigationGroup, LobbyNavigationModuleName);
            GameObject navigation = InstantiateLobbyModule(LobbyNavigationPrefabPath, navigationGroup);
            navigation.transform.SetSiblingIndex(1);
            Button upgradeMenuButton = RequireComponent<Button>(navigation.transform, "UpgradeMenuButton");
            Button equipmentMenuButton = RequireComponent<Button>(navigation.transform, "EquipmentMenuButton");
            Button settingsButton = RequireComponent<Button>(navigation.transform, "SettingsButton");
            ConfigureLobbyNavigationController(controller, upgradeMenuButton, equipmentMenuButton, settingsButton);
            SaveLobbyScene(scene);
        }

        [MenuItem("CatMouse/Install/Refresh Lobby/Upgrade Panel")]
        public static void RefreshLobbyUpgradePanel()
        {
            if (!TryGetLobbySceneContext(out Scene scene, out Transform canvasTransform, out LobbySceneController controller))
            {
                return;
            }

            MetaProgressionCatalog catalog = CreateMetaProgressionCatalog(CreateMetaRunItems());
            SynchronizeLobbyPanelSlots(catalog);
            Transform panelsGroup = GetOrCreateLobbyGroup(canvasTransform, LobbyPanelsGroupName, 2);
            RemoveLobbyModule(panelsGroup, LobbyUpgradePanelModuleName);
            GameObject panel = InstantiateLobbyModule(LobbyUpgradePanelPrefabPath, panelsGroup);
            panel.transform.SetSiblingIndex(0);
            panel.SetActive(false);
            ResolveUpgradePanelBindings(panel, catalog.Upgrades.Count, out Button closeButton, out Image[] icons, out TMP_Text[] names, out TMP_Text[] descriptions, out TMP_Text[] levels, out TMP_Text[] buttonLabels, out Button[] buttons);
            ConfigureLobbyUpgradePanelController(controller, catalog, panel, closeButton, icons, names, descriptions, levels, buttonLabels, buttons);
            SaveLobbyScene(scene);
        }

        [MenuItem("CatMouse/Install/Refresh Lobby/Equipment Panel")]
        public static void RefreshLobbyEquipmentPanel()
        {
            if (!TryGetLobbySceneContext(out Scene scene, out Transform canvasTransform, out LobbySceneController controller))
            {
                return;
            }

            MetaProgressionCatalog catalog = CreateMetaProgressionCatalog(CreateMetaRunItems());
            SynchronizeLobbyPanelSlots(catalog);
            Transform panelsGroup = GetOrCreateLobbyGroup(canvasTransform, LobbyPanelsGroupName, 2);
            RemoveLobbyModule(panelsGroup, LobbyEquipmentPanelModuleName);
            GameObject panel = InstantiateLobbyModule(LobbyEquipmentPanelPrefabPath, panelsGroup);
            panel.transform.SetSiblingIndex(1);
            panel.SetActive(false);
            int equipmentSlotCount = GetMaximumEquipmentSlotCount(catalog);
            ResolveEquipmentPanelBindings(panel, equipmentSlotCount, out Button closeButton, out Button[] slotButtons, out Image[] rows, out Image[] icons, out TMP_Text[] names, out TMP_Text[] descriptions, out TMP_Text[] buttonLabels, out Button[] buttons);
            ConfigureLobbyEquipmentPanelController(controller, catalog, panel, closeButton, slotButtons, rows, icons, names, descriptions, buttonLabels, buttons);
            SaveLobbyScene(scene);
        }

        [MenuItem("CatMouse/Install/Refresh Lobby/Settings Panel")]
        public static void RefreshLobbySettingsPanel()
        {
            if (!TryGetLobbySceneContext(out Scene scene, out Transform canvasTransform, out LobbySceneController controller))
            {
                return;
            }

            Transform panelsGroup = GetOrCreateLobbyGroup(canvasTransform, LobbyPanelsGroupName, 2);
            RemoveLobbyModule(panelsGroup, LobbySettingsPanelModuleName);
            GameObject panel = InstantiateLobbyModule(LobbySettingsPanelPrefabPath, panelsGroup);
            panel.transform.SetSiblingIndex(2);
            panel.SetActive(false);
            ResolveSettingsPanelBindings(panel, out Slider volumeSlider, out TMP_Text volumeLabel, out Button closeButton);
            ConfigureLobbySettingsPanelController(controller, panel, volumeSlider, volumeLabel, closeButton);
            SaveLobbyScene(scene);
        }

        private static MetaRunItems CreateMetaRunItems()
        {
            EnsureFolder("Assets/_Project/Data");
            EnsureFolder("Assets/_Project/Data/Meta");
            EnsureFolder(MetaItemFolderPath);

            return new MetaRunItems(
                CreateOrUpdateItem("MetaUpgrade_AttackDamage", "도토리 단련", "공격력 +1", MetaAttackDamageIconSpritePath, 5, PlayerStatType.AttackDamage, PlayerStatModifierOperation.Flat, 1f),
                CreateOrUpdateItem("MetaUpgrade_AttackSpeed", "앞발 훈련", "공격 속도 +15%", MetaAttackSpeedIconSpritePath, 5, PlayerStatType.AttackSpeed, PlayerStatModifierOperation.Percent, 0.15f),
                CreateOrUpdateItem("MetaUpgrade_ForwardSpeed", "질주 훈련", "전진 속도 +10%", MetaForwardSpeedIconSpritePath, 5, PlayerStatType.ForwardSpeed, PlayerStatModifierOperation.Percent, 0.1f),
                CreateOrUpdateItem("MetaEquipment_HardenedAcorn", "도토리 갑옷", "단단한 껍질 · 공격력 +1", MetaHardenedAcornIconSpritePath, 1, PlayerStatType.AttackDamage, PlayerStatModifierOperation.Flat, 1f),
                CreateOrUpdateItem("MetaEquipment_WindupSlingshot", "태엽 신발", "태엽 걸음 · 전진 속도 +12%", MetaWindupSlingshotIconSpritePath, 1, PlayerStatType.ForwardSpeed, PlayerStatModifierOperation.Percent, 0.12f),
                CreateOrUpdateItem("MetaEquipment_LongTailScope", "탐험 모자", "수납 주머니 · 투사체 속도 +40%", MetaLongTailScopeIconSpritePath, 1, PlayerStatType.ProjectileSpeed, PlayerStatModifierOperation.Percent, 0.4f));
        }

        private static MetaProgressionCatalog CreateMetaProgressionCatalog(MetaRunItems items)
        {
            MetaProgressionCatalog catalog = AssetDatabase.LoadAssetAtPath<MetaProgressionCatalog>(MetaCatalogAssetPath);
            if (catalog != null)
            {
                return catalog;
            }

            catalog = ScriptableObject.CreateInstance<MetaProgressionCatalog>();
            AssetDatabase.CreateAsset(catalog, MetaCatalogAssetPath);

            SerializedObject serializedCatalog = new(catalog);
            SerializedProperty upgrades = serializedCatalog.FindProperty("_upgrades");
            upgrades.arraySize = 3;
            SetUpgradeDefinition(upgrades.GetArrayElementAtIndex(0), "attack_damage", items.AttackDamageUpgrade, 5, 20, 20);
            SetUpgradeDefinition(upgrades.GetArrayElementAtIndex(1), "attack_speed", items.AttackSpeedUpgrade, 5, 25, 25);
            SetUpgradeDefinition(upgrades.GetArrayElementAtIndex(2), "forward_speed", items.ForwardSpeedUpgrade, 5, 15, 15);

            SerializedProperty equipment = serializedCatalog.FindProperty("_equipment");
            equipment.arraySize = 3;
            SetEquipmentDefinition(equipment.GetArrayElementAtIndex(0), "pantry_cap", MetaEquipmentSlot.Hat, items.LongTailScopeEquipment);
            SetEquipmentDefinition(equipment.GetArrayElementAtIndex(1), "acorn_armor", MetaEquipmentSlot.Armor, items.HardenedAcornEquipment);
            SetEquipmentDefinition(equipment.GetArrayElementAtIndex(2), "windup_shoes", MetaEquipmentSlot.Shoes, items.WindupSlingshotEquipment);
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void SetUpgradeDefinition(SerializedProperty definition, string id, RunItemDefinition item, int maximumLevel, int baseCheeseCost, int cheeseCostIncrease)
        {
            definition.FindPropertyRelative("_id").stringValue = id;
            definition.FindPropertyRelative("_runItem").objectReferenceValue = item;
            definition.FindPropertyRelative("_maximumLevel").intValue = maximumLevel;
            definition.FindPropertyRelative("_baseCheeseCost").intValue = baseCheeseCost;
            definition.FindPropertyRelative("_cheeseCostIncrease").intValue = cheeseCostIncrease;
        }

        private static void SetEquipmentDefinition(SerializedProperty definition, string id, MetaEquipmentSlot slot, RunItemDefinition item)
        {
            definition.FindPropertyRelative("_id").stringValue = id;
            definition.FindPropertyRelative("_slot").enumValueIndex = (int)slot;
            definition.FindPropertyRelative("_runItem").objectReferenceValue = item;
        }

        private static RunItemDefinition CreateOrUpdateItem(
            string assetName,
            string displayName,
            string description,
            string iconSpritePath,
            int maximumStacks,
            PlayerStatType statType,
            PlayerStatModifierOperation operation,
            float value)
        {
            string assetPath = $"{MetaItemFolderPath}/{assetName}.asset";
            RunItemDefinition definition = AssetDatabase.LoadAssetAtPath<RunItemDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<RunItemDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }
            else
            {
                return definition;
            }

            SerializedObject serializedDefinition = new(definition);
            serializedDefinition.FindProperty("_displayName").stringValue = displayName;
            serializedDefinition.FindProperty("_description").stringValue = description;
            serializedDefinition.FindProperty("_icon").objectReferenceValue = LoadUiSprite(iconSpritePath, Vector4.zero);
            serializedDefinition.FindProperty("_maximumStacks").intValue = maximumStacks;
            serializedDefinition.FindProperty("_temporaryDuration").floatValue = 0f;

            SerializedProperty modifiers = serializedDefinition.FindProperty("_modifiers");
            modifiers.arraySize = 1;
            SerializedProperty modifier = modifiers.GetArrayElementAtIndex(0);
            modifier.FindPropertyRelative("_statType").enumValueIndex = (int)statType;
            modifier.FindPropertyRelative("_operation").enumValueIndex = (int)operation;
            modifier.FindPropertyRelative("_value").floatValue = value;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void ConfigureGameScene(MetaProgressionCatalog catalog)
        {
            EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

            PlayerRunStats runStats = Object.FindFirstObjectByType<PlayerRunStats>();
            if (runStats == null)
            {
                Debug.LogError("[LobbySceneInstaller] GameScene에서 PlayerRunStats를 찾지 못했습니다.");
                return;
            }

            MetaProgressionApplier applier = runStats.GetComponent<MetaProgressionApplier>();
            if (applier == null)
            {
                applier = runStats.gameObject.AddComponent<MetaProgressionApplier>();
            }

            SerializedObject serializedApplier = new(applier);
            serializedApplier.FindProperty("_runStats").objectReferenceValue = runStats;
            serializedApplier.FindProperty("_catalog").objectReferenceValue = catalog;
            serializedApplier.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(applier);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        private static void CreateLobbyScene(MetaProgressionCatalog catalog)
        {
            EnsureFolder("Assets/_Project/Scenes");
            PrepareLobbyFonts();
            EnsureFolder("Assets/_Project/Scenes/Lobby");
            EnsureLobbyPresentationFolders();
            SynchronizeLobbyPanelSlots(catalog);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            LobbyPrefabLayout stageLayout = LoadLobbyModuleLayout(LobbyStagePrefabPath);
            Canvas canvas = CreateCanvas(stageLayout.ReferenceResolution);
            LobbySceneController controller = canvas.gameObject.AddComponent<LobbySceneController>();
            Transform stageGroup = GetOrCreateLobbyGroup(canvas.transform, LobbyStageGroupName, 0);
            Transform navigationGroup = GetOrCreateLobbyGroup(canvas.transform, LobbyNavigationGroupName, 1);
            Transform panelsGroup = GetOrCreateLobbyGroup(canvas.transform, LobbyPanelsGroupName, 2);
            GameObject stage = InstantiateLobbyModule(LobbyStagePrefabPath, stageGroup);
            GameObject navigation = InstantiateLobbyModule(LobbyNavigationPrefabPath, navigationGroup);
            GameObject upgradePanel = InstantiateLobbyModule(LobbyUpgradePanelPrefabPath, panelsGroup);
            GameObject equipmentPanel = InstantiateLobbyModule(LobbyEquipmentPanelPrefabPath, panelsGroup);
            GameObject settingsPanel = InstantiateLobbyModule(LobbySettingsPanelPrefabPath, panelsGroup);
            stage.transform.SetSiblingIndex(0);
            navigation.transform.SetSiblingIndex(0);
            upgradePanel.transform.SetSiblingIndex(0);
            equipmentPanel.transform.SetSiblingIndex(1);
            settingsPanel.transform.SetSiblingIndex(2);
            upgradePanel.SetActive(false);
            equipmentPanel.SetActive(false);
            settingsPanel.SetActive(false);

            TMP_Text coinLabel = RequireComponent<TMP_Text>(stage.transform, "CurrencyChip/CoinLabel");
            Button startButton = RequireComponent<Button>(stage.transform, "StartButton");
            Button upgradeMenuButton = RequireComponent<Button>(navigation.transform, "UpgradeMenuButton");
            Button equipmentMenuButton = RequireComponent<Button>(navigation.transform, "EquipmentMenuButton");
            Button settingsButton = RequireComponent<Button>(navigation.transform, "SettingsButton");
            ResolveUpgradePanelBindings(upgradePanel, catalog.Upgrades.Count, out Button closeUpgradeButton, out Image[] upgradeIcons, out TMP_Text[] upgradeNames, out TMP_Text[] upgradeDescriptions, out TMP_Text[] upgradeLevels, out TMP_Text[] upgradeButtonLabels, out Button[] upgradeButtons);
            int equipmentSlotCount = GetMaximumEquipmentSlotCount(catalog);
            ResolveEquipmentPanelBindings(equipmentPanel, equipmentSlotCount, out Button closeEquipmentButton, out Button[] equipmentSlotButtons, out Image[] equipmentRows, out Image[] equipmentIcons, out TMP_Text[] equipmentNames, out TMP_Text[] equipmentDescriptions, out TMP_Text[] equipmentButtonLabels, out Button[] equipmentButtons);
            ResolveSettingsPanelBindings(settingsPanel, out Slider masterVolumeSlider, out TMP_Text masterVolumeLabel, out Button closeSettingsButton);

            ConfigureLobbyController(
                controller,
                coinLabel,
                upgradeIcons,
                upgradeNames,
                upgradeDescriptions,
                upgradeLevels,
                upgradeButtonLabels,
                upgradeButtons,
                equipmentRows,
                equipmentIcons,
                equipmentNames,
                equipmentDescriptions,
                equipmentButtonLabels,
                equipmentButtons,
                equipmentSlotButtons,
                startButton,
                upgradeMenuButton,
                equipmentMenuButton,
                settingsButton,
                upgradePanel,
                closeUpgradeButton,
                equipmentPanel,
                closeEquipmentButton,
                settingsPanel,
                masterVolumeSlider,
                masterVolumeLabel,
                closeSettingsButton);

            EnsureEventSystem();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, LobbyScenePath);
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.16f, 0.11f, 0.09f, 1f);
            camera.orthographic = true;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static Canvas CreateCanvas(Vector2 referenceResolution)
        {
            GameObject canvasObject = new("LobbyCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1f;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;
            scaler.dynamicPixelsPerUnit = 2f;
            return canvas;
        }

        private static LobbyPrefabLayout LoadLobbyModuleLayout(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new System.InvalidOperationException($"로비 모듈 프리팹을 찾지 못했습니다: {prefabPath}");
            }

            LobbyPrefabLayout layout = prefab.GetComponent<LobbyPrefabLayout>();
            if (layout == null)
            {
                throw new System.InvalidOperationException($"로비 모듈 프리팹에 레이아웃 컴포넌트가 없습니다: {prefabPath}");
            }

            return layout;
        }

        private static Transform GetOrCreateLobbyGroup(Transform canvasTransform, string groupName, int siblingIndex)
        {
            Transform group = canvasTransform.Find(groupName);
            if (group == null)
            {
                GameObject groupObject = new(groupName, typeof(RectTransform));
                group = groupObject.transform;
                group.SetParent(canvasTransform, false);
                Stretch(group.GetComponent<RectTransform>());
            }

            group.SetSiblingIndex(siblingIndex);
            return group;
        }

        private static GameObject InstantiateLobbyModule(string prefabPath, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new System.InvalidOperationException($"로비 모듈 프리팹을 찾지 못했습니다: {prefabPath}");
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private static void SynchronizeLobbyPanelSlots(MetaProgressionCatalog catalog)
        {
            EnsureUpgradeDrawerHeight();
            SynchronizePanelSlots(
                LobbyUpgradePanelPrefabPath,
                "UpgradeDrawer",
                "Upgrade_",
                LobbyUpgradeSlotPrefabPath,
                catalog.Upgrades.Count);
            RemoveEquipmentSlotLabelPrefab();
            EnsureEquipmentSlotTabs();
            SynchronizePanelSlots(
                LobbyEquipmentPanelPrefabPath,
                "EquipmentDrawer",
                "Equipment_",
                LobbyEquipmentSlotPrefabPath,
                GetMaximumEquipmentSlotCount(catalog));
        }

        private static int GetMaximumEquipmentSlotCount(MetaProgressionCatalog catalog)
        {
            int maximumCount = 0;
            for (int slotIndex = (int)MetaEquipmentSlot.Hat; slotIndex <= (int)MetaEquipmentSlot.Shoes; slotIndex++)
            {
                int slotCount = 0;
                MetaEquipmentSlot slot = (MetaEquipmentSlot)slotIndex;
                for (int equipmentIndex = 0; equipmentIndex < catalog.Equipment.Count; equipmentIndex++)
                {
                    if (catalog.Equipment[equipmentIndex]?.Slot == slot)
                    {
                        slotCount++;
                    }
                }

                maximumCount = Mathf.Max(maximumCount, slotCount);
            }

            return maximumCount;
        }

        private static void EnsureUpgradeDrawerHeight()
        {
            GameObject panel = PrefabUtility.LoadPrefabContents(LobbyUpgradePanelPrefabPath);
            try
            {
                RectTransform drawer = RequireTransform(panel.transform, "UpgradeDrawer").GetComponent<RectTransform>();
                Vector2 size = drawer.sizeDelta;
                size.y = 960f;
                drawer.sizeDelta = size;
                RequireComponent<LobbyPrefabLayout>(panel.transform, string.Empty).CaptureFromHierarchy();
                PrefabUtility.SaveAsPrefabAsset(panel, LobbyUpgradePanelPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(panel);
            }
        }

        private static void EnsureEquipmentSlotTabs()
        {
            GameObject panel = PrefabUtility.LoadPrefabContents(LobbyEquipmentPanelPrefabPath);
            try
            {
                Transform drawer = RequireTransform(panel.transform, "EquipmentDrawer");
                LobbySlotListLayout listLayout = drawer.GetComponent<LobbySlotListLayout>();
                SerializedObject serializedListLayout = new(listLayout);
                serializedListLayout.FindProperty("_firstSlotPosition").vector2Value = new Vector2(0f, -232f);
                serializedListLayout.ApplyModifiedPropertiesWithoutUndo();
                Transform existingTabs = drawer.Find("SlotTabs");
                if (existingTabs != null)
                {
                    Object.DestroyImmediate(existingTabs.gameObject);
                }

                GameObject tabs = CreateUiObject("SlotTabs", drawer);
                Stretch(tabs.GetComponent<RectTransform>());
                CreateEquipmentSlotTab(tabs.transform, "HatButton", "모자", -168f);
                CreateEquipmentSlotTab(tabs.transform, "ArmorButton", "갑옷", 0f);
                CreateEquipmentSlotTab(tabs.transform, "ShoesButton", "신발", 168f);
                RequireComponent<LobbyPrefabLayout>(panel.transform, string.Empty).CaptureFromHierarchy();
                PrefabUtility.SaveAsPrefabAsset(panel, LobbyEquipmentPanelPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(panel);
            }
        }

        private static void CreateEquipmentSlotTab(Transform parent, string name, string label, float positionX)
        {
            Button button = CreateButton(
                name,
                parent,
                label,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(152f, 40f),
                new Vector2(positionX, -144f),
                Color.white,
                18,
                out TMP_Text labelText);
            button.image.color = new Color(1f, 1f, 1f, 0.68f);
            labelText.font = LoadLobbyBoldFont();
        }

        private static void SynchronizePanelSlots(
            string panelPrefabPath,
            string drawerName,
            string rowPrefix,
            string slotPrefabPath,
            int itemCount)
        {
            GameObject panel = PrefabUtility.LoadPrefabContents(panelPrefabPath);
            try
            {
                Transform drawer = RequireTransform(panel.transform, drawerName);
                List<Transform> rows = FindPanelRows(drawer, rowPrefix);
                LobbySlotListLayout listLayout = drawer.GetComponent<LobbySlotListLayout>();
                if (listLayout == null)
                {
                    if (rows.Count == 0)
                    {
                        throw new System.InvalidOperationException($"슬롯 배치 기준이 없습니다: {panelPrefabPath}");
                    }

                    RectTransform firstRow = rows[0].GetComponent<RectTransform>();
                    Vector2 slotPitch = rows.Count > 1
                        ? rows[1].GetComponent<RectTransform>().anchoredPosition - firstRow.anchoredPosition
                        : Vector2.zero;
                    listLayout = drawer.gameObject.AddComponent<LobbySlotListLayout>();
                    listLayout.InitializeIfNeeded(firstRow.anchoredPosition, slotPitch);
                }

                GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPrefabPath);
                if (slotPrefab == null)
                {
                    throw new System.InvalidOperationException($"슬롯 프리팹을 찾지 못했습니다: {slotPrefabPath}");
                }

                int firstSlotSiblingIndex = rows.Count > 0 ? rows[0].GetSiblingIndex() : drawer.childCount;
                for (int index = 0; index < itemCount; index++)
                {
                    Transform row;
                    if (index < rows.Count)
                    {
                        row = rows[index];
                    }
                    else
                    {
                        GameObject rowObject = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab);
                        rowObject.transform.SetParent(drawer, false);
                        row = rowObject.transform;
                    }

                    row.name = $"{rowPrefix}{index + 1}";
                    row.SetSiblingIndex(firstSlotSiblingIndex + index);
                    row.gameObject.SetActive(true);
                    row.GetComponent<RectTransform>().anchoredPosition = listLayout.GetSlotPosition(index);
                }

                for (int index = rows.Count - 1; index >= itemCount; index--)
                {
                    UnityEngine.Object.DestroyImmediate(rows[index].gameObject);
                }

                RequireComponent<LobbyPrefabLayout>(panel.transform, string.Empty).CaptureFromHierarchy();
                PrefabUtility.SaveAsPrefabAsset(panel, panelPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(panel);
            }
        }

        private static void RemoveEquipmentSlotLabelPrefab()
        {
            GameObject slot = PrefabUtility.LoadPrefabContents(LobbyEquipmentSlotPrefabPath);
            try
            {
                Transform slotLabel = slot.transform.Find("Slot");
                if (slotLabel == null)
                {
                    return;
                }

                Object.DestroyImmediate(slotLabel.gameObject);
                RequireComponent<LobbyPrefabLayout>(slot.transform, string.Empty).CaptureFromHierarchy();
                PrefabUtility.SaveAsPrefabAsset(slot, LobbyEquipmentSlotPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(slot);
            }
        }

        private static List<Transform> FindPanelRows(Transform drawer, string rowPrefix)
        {
            List<Transform> rows = new();
            for (int index = 0; index < drawer.childCount; index++)
            {
                Transform child = drawer.GetChild(index);
                if (child.name.StartsWith(rowPrefix, System.StringComparison.Ordinal))
                {
                    rows.Add(child);
                }
            }

            return rows;
        }

        private static void ResolveUpgradePanelBindings(
            GameObject panel,
            int itemCount,
            out Button closeButton,
            out Image[] icons,
            out TMP_Text[] names,
            out TMP_Text[] descriptions,
            out TMP_Text[] levels,
            out TMP_Text[] buttonLabels,
            out Button[] buttons)
        {
            Transform drawer = RequireTransform(panel.transform, "UpgradeDrawer");
            closeButton = RequireComponent<Button>(drawer, "CloseButton");
            icons = new Image[itemCount];
            names = new TMP_Text[itemCount];
            descriptions = new TMP_Text[itemCount];
            levels = new TMP_Text[itemCount];
            buttonLabels = new TMP_Text[itemCount];
            buttons = new Button[itemCount];

            for (int index = 0; index < itemCount; index++)
            {
                Transform row = RequireTransform(drawer, $"Upgrade_{index + 1}");
                icons[index] = RequireComponent<Image>(row, "ItemIcon");
                names[index] = RequireComponent<TMP_Text>(row, "Name");
                descriptions[index] = RequireComponent<TMP_Text>(row, "Description");
                levels[index] = RequireComponent<TMP_Text>(row, "Level");
                buttons[index] = RequireComponent<Button>(row, "UpgradeButton");
                buttonLabels[index] = RequireComponent<TMP_Text>(row, "UpgradeButton/Label");
            }
        }

        private static void ResolveEquipmentPanelBindings(
            GameObject panel,
            int itemCount,
            out Button closeButton,
            out Button[] slotButtons,
            out Image[] rows,
            out Image[] icons,
            out TMP_Text[] names,
            out TMP_Text[] descriptions,
            out TMP_Text[] buttonLabels,
            out Button[] buttons)
        {
            Transform drawer = RequireTransform(panel.transform, "EquipmentDrawer");
            closeButton = RequireComponent<Button>(drawer, "CloseButton");
            slotButtons = new[]
            {
                RequireComponent<Button>(drawer, "SlotTabs/HatButton"),
                RequireComponent<Button>(drawer, "SlotTabs/ArmorButton"),
                RequireComponent<Button>(drawer, "SlotTabs/ShoesButton"),
            };
            rows = new Image[itemCount];
            icons = new Image[itemCount];
            names = new TMP_Text[itemCount];
            descriptions = new TMP_Text[itemCount];
            buttonLabels = new TMP_Text[itemCount];
            buttons = new Button[itemCount];

            for (int index = 0; index < itemCount; index++)
            {
                Transform row = RequireTransform(drawer, $"Equipment_{index + 1}");
                rows[index] = RequireComponent<Image>(row, string.Empty);
                icons[index] = RequireComponent<Image>(row, "ItemIcon");
                names[index] = RequireComponent<TMP_Text>(row, "Name");
                descriptions[index] = RequireComponent<TMP_Text>(row, "Description");
                buttons[index] = RequireComponent<Button>(row, "EquipButton");
                buttonLabels[index] = RequireComponent<TMP_Text>(row, "EquipButton/Label");
            }
        }

        private static void ResolveSettingsPanelBindings(GameObject panel, out Slider volumeSlider, out TMP_Text volumeLabel, out Button closeButton)
        {
            Transform drawer = RequireTransform(panel.transform, "SettingsDrawer");
            volumeSlider = RequireComponent<Slider>(drawer, "MasterVolumeSlider");
            volumeLabel = RequireComponent<TMP_Text>(drawer, "VolumeLabel");
            closeButton = RequireComponent<Button>(drawer, "CloseButton");
        }

        private static Transform RequireTransform(Transform root, string relativePath)
        {
            Transform target = string.IsNullOrEmpty(relativePath) ? root : root.Find(relativePath);
            if (target == null)
            {
                throw new System.InvalidOperationException($"로비 모듈 경로를 찾지 못했습니다: {root.name}/{relativePath}");
            }

            return target;
        }

        private static T RequireComponent<T>(Transform root, string relativePath) where T : Component
        {
            Transform target = RequireTransform(root, relativePath);
            T component = target.GetComponent<T>();
            if (component == null)
            {
                throw new System.InvalidOperationException($"로비 모듈 컴포넌트를 찾지 못했습니다: {root.name}/{relativePath} ({typeof(T).Name})");
            }

            return component;
        }

        private static GameObject CreateLobbyStage(
            Transform parent,
            LobbyLayoutProfile layout,
            out TMP_Text cheeseLabel,
            out Button startButton)
        {
            GameObject stage = CreateUiObject("LobbyStage", parent);
            Stretch(stage.GetComponent<RectTransform>());
            CreateBackground(stage.transform);
            cheeseLabel = CreateLobbyCurrencyChip(stage.transform, layout);
            CreatePlayerPreview(stage.transform, layout);
            startButton = CreateLobbyStartButton(stage.transform, layout);
            return stage;
        }

        private static GameObject CreateLobbyNavigation(
            Transform parent,
            LobbyLayoutProfile layout,
            out Button upgradeMenuButton,
            out Button equipmentMenuButton,
            out Button settingsButton)
        {
            GameObject navigation = CreateUiObject("LobbyNavigation", parent);
            Stretch(navigation.GetComponent<RectTransform>());
            CreateNavigationDock(navigation.transform, layout);
            upgradeMenuButton = CreateLobbyNavigationButton("UpgradeMenuButton", navigation.transform, "영구 강화", LobbyUpgradeIconSpritePath, layout.UpgradeButtonPosition, layout);
            equipmentMenuButton = CreateLobbyNavigationButton("EquipmentMenuButton", navigation.transform, "장비 착용", LobbyEquipmentIconSpritePath, layout.EquipmentButtonPosition, layout);
            settingsButton = CreateLobbyNavigationButton("SettingsButton", navigation.transform, "설정", LobbySettingsIconSpritePath, layout.SettingsButtonPosition, layout);
            return navigation;
        }

        private static void CreateBackground(Transform parent)
        {
            GameObject background = CreateUiObject("Background", parent);
            Stretch(background.GetComponent<RectTransform>());
            Image image = background.AddComponent<Image>();
            image.sprite = LoadLobbyBackgroundSprite();
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = image.sprite == null ? new Color(0.55f, 0.37f, 0.24f, 1f) : Color.white;
        }

        private static Sprite LoadLobbyBackgroundSprite()
        {
            return LoadUiSprite(LobbyBackgroundSpritePath, Vector4.zero);
        }

        private static Sprite LoadUiSprite(string assetPath, Vector4 border)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return null;
            }

            bool needsImport = importer.textureType != TextureImporterType.Sprite
                || importer.spriteImportMode != SpriteImportMode.Single
                || importer.mipmapEnabled
                || importer.filterMode != FilterMode.Bilinear
                || importer.wrapMode != TextureWrapMode.Clamp
                || !Mathf.Approximately(importer.spritePixelsPerUnit, 200f)
                || importer.spriteBorder != border;

            if (needsImport)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.spritePixelsPerUnit = 200f;
                importer.spriteBorder = border;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void ApplySlicedSprite(Image image, string assetPath, Vector4 border)
        {
            Sprite sprite = LoadUiSprite(assetPath, border);
            if (sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            image.preserveAspect = false;
            image.color = Color.white;
        }

        private static TMP_FontAsset LoadLobbyFont()
        {
            if (!HasUsableAtlas(_lobbyFont))
            {
                _lobbyFont = LoadOrCreateTmpFontAsset(LobbyFontPath, LobbyTmpFontAssetPath);
            }

            return _lobbyFont != null ? _lobbyFont : TMP_Settings.defaultFontAsset;
        }

        private static TMP_FontAsset LoadLobbyBoldFont()
        {
            if (!HasUsableAtlas(_lobbyBoldFont))
            {
                _lobbyBoldFont = LoadOrCreateTmpFontAsset(LobbyBoldFontPath, LobbyTmpBoldFontAssetPath);
            }

            return _lobbyBoldFont != null ? _lobbyBoldFont : LoadLobbyFont();
        }

        private static TMP_FontAsset LoadLobbyDisplayFont()
        {
            if (!HasUsableAtlas(_lobbyDisplayFont))
            {
                _lobbyDisplayFont = LoadOrCreateTmpFontAsset(LobbyDisplayFontPath, LobbyTmpDisplayFontAssetPath);
            }

            return _lobbyDisplayFont != null ? _lobbyDisplayFont : LoadLobbyBoldFont();
        }

        private static TMP_FontAsset LoadOrCreateTmpFontAsset(string sourceFontPath, string fontAssetPath)
        {
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);
            if (sourceFont == null)
            {
                return TMP_Settings.defaultFontAsset;
            }

            EnsureFolder(LobbyTmpFontFolderPath);
            if (!HasUsableAtlas(fontAsset))
            {
                TMP_FontAsset rebuiltFontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
                if (fontAsset == null)
                {
                    fontAsset = rebuiltFontAsset;
                    AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
                }
                else
                {
                    fontAsset.atlasTextures = rebuiltFontAsset.atlasTextures;
                    fontAsset.material = rebuiltFontAsset.material;
                }
            }

            fontAsset.name = Path.GetFileNameWithoutExtension(fontAssetPath);
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            AddFontSubAsset(fontAsset.atlasTexture, fontAsset);
            AddFontSubAsset(fontAsset.material, fontAsset);
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            return fontAsset;
        }

        private static void PrepareLobbyFonts()
        {
            PopulateFontGlyphs(LoadLobbyFont());
            PopulateFontGlyphs(LoadLobbyBoldFont());
            PopulateFontGlyphs(LoadLobbyDisplayFont());
            AssetDatabase.SaveAssets();
        }

        private static void PopulateFontGlyphs(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null)
            {
                return;
            }

            fontAsset.TryAddCharacters(LobbyFontCharacters, out _, true);
            EditorUtility.SetDirty(fontAsset);
        }

        private static bool HasUsableAtlas(TMP_FontAsset fontAsset)
        {
            return fontAsset != null && fontAsset.atlasTexture != null && fontAsset.material != null;
        }

        private static void AddFontSubAsset(Object subAsset, TMP_FontAsset fontAsset)
        {
            if (subAsset != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(subAsset)))
            {
                AssetDatabase.AddObjectToAsset(subAsset, fontAsset);
            }
        }

        private static void ApplyLobbyButtonLabelStyle(TMP_Text text, int fontSize, Color color)
        {
            text.font = LoadLobbyBoldFont();
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Normal;
            text.color = color;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
        }

        private static void ApplyLobbyDisplayLabelStyle(TMP_Text text, int fontSize, Color color)
        {
            text.font = LoadLobbyDisplayFont();
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Normal;
            text.color = color;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
        }

        private static void ApplyTextOutline(TMP_Text text, Color effectColor)
        {
            text.outlineColor = effectColor;
            text.outlineWidth = 0.08f;
        }

        private static void CreatePlayerPreview(Transform parent, LobbyLayoutProfile layout)
        {
            Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Characters/PlayerMouse_v1.png");
            if (playerSprite == null)
            {
                return;
            }

            GameObject playerImageObject = CreateUiObject("PlayerPreview", parent);
            RectTransform playerImageRect = playerImageObject.GetComponent<RectTransform>();
            playerImageRect.anchorMin = new Vector2(0.5f, 0.5f);
            playerImageRect.anchorMax = new Vector2(0.5f, 0.5f);
            playerImageRect.pivot = new Vector2(0.5f, 0.5f);
            playerImageRect.sizeDelta = layout.PlayerImageSize;
            playerImageRect.anchoredPosition = layout.PlayerImagePosition;

            Image playerImage = playerImageObject.AddComponent<Image>();
            playerImage.sprite = playerSprite;
            playerImage.preserveAspect = true;
            playerImage.raycastTarget = false;
        }

        private static TMP_Text CreateLobbyCurrencyChip(Transform parent, LobbyLayoutProfile layout)
        {
            GameObject chip = CreateUiObject("CurrencyChip", parent);
            RectTransform chipRect = chip.GetComponent<RectTransform>();
            chipRect.anchorMin = new Vector2(0f, 1f);
            chipRect.anchorMax = new Vector2(0f, 1f);
            chipRect.pivot = new Vector2(0.5f, 0.5f);
            chipRect.sizeDelta = layout.CurrencyChipSize;
            chipRect.anchoredPosition = layout.CurrencyChipPosition;

            Image chipImage = chip.AddComponent<Image>();
            ApplySlicedSprite(chipImage, CurrencyChipSpritePath, CurrencyChipSpriteBorder);

            TMP_Text label = CreateText("CoinLabel", chip.transform, "코인 120", 32, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.24f, 0.14f, 0.1f, 1f));
            ApplyLobbyButtonLabelStyle(label, 32, new Color(0.24f, 0.14f, 0.1f, 1f));
            return label;
        }

        private static Button CreateLobbyNavigationButton(
            string name,
            Transform parent,
            string label,
            string iconSpritePath,
            Vector2 position,
            LobbyLayoutProfile layout)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 0.5f);
            rectTransform.anchorMax = new Vector2(1f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = layout.MenuButtonSize;
            rectTransform.anchoredPosition = position;

            Image image = buttonObject.AddComponent<Image>();
            ApplySlicedSprite(image, NavigationButtonSpritePath, NavigationButtonSpriteBorder);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            ConfigureLobbyNavigationButton(button, image);
            buttonObject.AddComponent<LobbyButtonPressFeedback>();

            CreateLobbyIcon(buttonObject.transform, iconSpritePath, layout);
            TMP_Text labelText = CreateText("Label", buttonObject.transform, label, 32, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.24f, 0.14f, 0.1f, 1f));
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.offsetMin = new Vector2(layout.NavigationContentInsets.x, 0f);
            labelRect.offsetMax = new Vector2(-layout.NavigationContentInsets.y, 0f);
            ApplyLobbyDisplayLabelStyle(labelText, 32, new Color(0.24f, 0.14f, 0.1f, 1f));
            return button;
        }

        private static void CreateNavigationDock(Transform parent, LobbyLayoutProfile layout)
        {
            GameObject dock = CreatePanel(
                "NavigationDock",
                parent,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                layout.MenuDockSize,
                layout.MenuDockPosition,
                Color.white);

            Image image = dock.GetComponent<Image>();
            image.sprite = null;
            image.color = new Color(0.11f, 0.05f, 0.02f, 0.72f);
            image.raycastTarget = false;
        }

        private static Button CreateLobbyStartButton(Transform parent, LobbyLayoutProfile layout)
        {
            GameObject buttonObject = CreateUiObject("StartButton", parent);
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = layout.StartButtonSize;
            rectTransform.anchoredPosition = layout.StartButtonPosition;

            Image image = buttonObject.AddComponent<Image>();
            ApplySlicedSprite(image, StartButtonSpritePath, NavigationButtonSpriteBorder);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            ConfigureLobbyButtonColors(button);
            buttonObject.AddComponent<LobbyButtonPressFeedback>();

            CreateLobbyStartIcon(buttonObject.transform, layout);
            TMP_Text labelText = CreateText("Label", buttonObject.transform, "게임 시작", 40, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.29f, 0.15f, 0.04f, 1f));
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.offsetMin = new Vector2(layout.StartContentInsets.x, 0f);
            labelRect.offsetMax = new Vector2(-layout.StartContentInsets.y, 0f);
            ApplyLobbyDisplayLabelStyle(labelText, 48, new Color(0.29f, 0.15f, 0.04f, 1f));
            return button;
        }

        private static void CreateLobbyIcon(Transform parent, string iconSpritePath, LobbyLayoutProfile layout)
        {
            GameObject icon = CreateUiObject("Icon", parent);
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = layout.NavigationIconSize;
            iconRect.anchoredPosition = new Vector2(layout.NavigationIconCenterX, 0f);

            Image iconImage = icon.AddComponent<Image>();
            iconImage.sprite = LoadUiSprite(iconSpritePath, Vector4.zero);
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }

        private static void CreateLobbyStartIcon(Transform parent, LobbyLayoutProfile layout)
        {
            GameObject icon = CreateUiObject("StartIcon", parent);
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = layout.StartIconSize;
            iconRect.anchoredPosition = new Vector2(layout.StartIconCenterX, 0f);

            Image iconImage = icon.AddComponent<Image>();
            iconImage.sprite = LoadUiSprite(LobbyStartIconSpritePath, Vector4.zero);
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }

        private static Image CreateMetaItemIcon(Transform parent, Sprite sprite, LobbyLayoutProfile layout)
        {
            GameObject icon = CreateUiObject("ItemIcon", parent);
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = layout.MetaItemIconSize;
            iconRect.anchoredPosition = layout.MetaItemIconPosition;

            Image image = icon.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static void ConfigureLobbyButtonColors(Button button)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.97f, 0.9f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        private static void ConfigureLobbyNavigationButton(Button button, Image background)
        {
            background.color = Color.white;
            ConfigureLobbyButtonColors(button);
        }

        private static void CreateUpgradePanel(
            Transform parent,
            IReadOnlyList<MetaUpgradeDefinition> definitions,
            LobbyLayoutProfile layout,
            out GameObject panel,
            out Button closeButton,
            out Image[] icons,
            out TMP_Text[] nameLabels,
            out TMP_Text[] descriptionLabels,
            out TMP_Text[] levelLabels,
            out TMP_Text[] buttonLabels,
            out Button[] buttons)
        {
            panel = CreateSidePanel("UpgradePanel", parent);
            GameObject drawer = CreateDrawer("UpgradeDrawer", panel.transform, layout);
            CreateText("Header", drawer.transform, "영구 강화", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), layout.PanelHeaderSize, layout.PanelHeaderPosition, new Color(0.24f, 0.14f, 0.1f, 1f));
            CreateText("Guide", drawer.transform, "획득한 코인으로 다음 런의 기본 능력치를 올립니다.", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), layout.PanelSecondaryTextSize, layout.PanelSecondaryTextPosition, new Color(0.43f, 0.29f, 0.21f, 1f));
            closeButton = CreateButton("CloseButton", drawer.transform, "닫기", new Vector2(1f, 1f), new Vector2(1f, 1f), layout.SidePanelCloseButtonSize, layout.SidePanelCloseButtonPosition, new Color(0.61f, 0.43f, 0.29f, 1f), 20, out _, SecondaryButtonSpritePath);

            icons = new Image[definitions.Count];
            nameLabels = new TMP_Text[definitions.Count];
            descriptionLabels = new TMP_Text[definitions.Count];
            levelLabels = new TMP_Text[definitions.Count];
            buttonLabels = new TMP_Text[definitions.Count];
            buttons = new Button[definitions.Count];

            for (int index = 0; index < definitions.Count; index++)
            {
                RunItemDefinition definition = definitions[index].RunItem;
                float rowY = layout.SidePanelFirstRowTopOffset + index * layout.SidePanelRowPitch;
                GameObject row = CreateMetaSlot($"Upgrade_{index + 1}", drawer.transform, rowY, MetaSlotSpritePath, layout);
                icons[index] = CreateMetaItemIcon(row.transform, definition.Icon, layout);
                nameLabels[index] = CreateText("Name", row.transform, definition.DisplayName, 24, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), layout.MetaItemNameSize, layout.MetaItemNamePosition, new Color(0.22f, 0.13f, 0.1f, 1f));
                descriptionLabels[index] = CreateText("Description", row.transform, definition.Description, 20, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), layout.MetaItemDescriptionSize, layout.MetaItemDescriptionPosition, new Color(0.45f, 0.29f, 0.2f, 1f));
                levelLabels[index] = CreateText("Level", row.transform, "Lv. 0 / 5", 20, TextAnchor.MiddleCenter, new Vector2(1f, 1f), new Vector2(1f, 1f), layout.UpgradeLevelSize, layout.UpgradeLevelPosition, new Color(0.35f, 0.24f, 0.17f, 1f));
                buttons[index] = CreateButton("UpgradeButton", row.transform, "강화", new Vector2(1f, 0f), new Vector2(1f, 0f), layout.UpgradeRowButtonSize, layout.UpgradeRowButtonPosition, new Color(0.75f, 0.47f, 0.24f, 1f), 20, out buttonLabels[index], PrimaryButtonSpritePath);
                ApplyTextOutline(buttonLabels[index], new Color(0.29f, 0.15f, 0.04f, 0.8f));
            }

            panel.SetActive(false);
        }

        private static void CreateEquipmentPanel(
            Transform parent,
            IReadOnlyList<MetaEquipmentDefinition> definitions,
            LobbyLayoutProfile layout,
            out GameObject panel,
            out Button closeButton,
            out Image[] rowBackgrounds,
            out Image[] icons,
            out TMP_Text[] nameLabels,
            out TMP_Text[] descriptionLabels,
            out TMP_Text[] buttonLabels,
            out Button[] buttons)
        {
            panel = CreateSidePanel("EquipmentPanel", parent);
            GameObject drawer = CreateDrawer("EquipmentDrawer", panel.transform, layout);
            CreateText("Header", drawer.transform, "장비 착용", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), layout.PanelHeaderSize, layout.PanelHeaderPosition, new Color(0.16f, 0.25f, 0.15f, 1f));
            closeButton = CreateButton("CloseButton", drawer.transform, "닫기", new Vector2(1f, 1f), new Vector2(1f, 1f), layout.SidePanelCloseButtonSize, layout.SidePanelCloseButtonPosition, new Color(0.35f, 0.5f, 0.3f, 1f), 20, out _, SecondaryButtonSpritePath);

            rowBackgrounds = new Image[definitions.Count];
            icons = new Image[definitions.Count];
            nameLabels = new TMP_Text[definitions.Count];
            descriptionLabels = new TMP_Text[definitions.Count];
            buttonLabels = new TMP_Text[definitions.Count];
            buttons = new Button[definitions.Count];

            for (int index = 0; index < definitions.Count; index++)
            {
                RunItemDefinition definition = definitions[index].RunItem;
                float rowY = layout.SidePanelFirstRowTopOffset + index * layout.SidePanelRowPitch;
                GameObject row = CreateMetaSlot($"Equipment_{index + 1}", drawer.transform, rowY, MetaSlotSpritePath, layout);
                rowBackgrounds[index] = row.GetComponent<Image>();
                icons[index] = CreateMetaItemIcon(row.transform, definition.Icon, layout);
                nameLabels[index] = CreateText("Name", row.transform, definition.DisplayName, 24, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), layout.MetaItemNameSize, layout.MetaItemNamePosition, new Color(0.16f, 0.25f, 0.15f, 1f));
                descriptionLabels[index] = CreateText("Description", row.transform, definition.Description, 20, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), layout.MetaItemDescriptionSize, layout.MetaItemDescriptionPosition, new Color(0.28f, 0.42f, 0.25f, 1f));
                buttons[index] = CreateButton("EquipButton", row.transform, "장착", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), layout.EquipmentRowButtonSize, layout.EquipmentRowButtonPosition, new Color(0.4f, 0.62f, 0.35f, 1f), 20, out buttonLabels[index], PrimaryButtonSpritePath);
                ApplyTextOutline(buttonLabels[index], new Color(0.29f, 0.15f, 0.04f, 0.8f));
            }

            panel.SetActive(false);
        }

        private static void CreateSettingsPanel(Transform parent, LobbyLayoutProfile layout, out GameObject panel, out Slider masterVolumeSlider, out TMP_Text masterVolumeLabel, out Button closeButton)
        {
            panel = CreateSidePanel("SettingsPanel", parent);
            GameObject drawer = CreateDrawer("SettingsDrawer", panel.transform, layout);
            CreateText("Title", drawer.transform, "설정", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), layout.PanelHeaderSize, layout.PanelHeaderPosition, new Color(0.24f, 0.14f, 0.1f, 1f));
            masterVolumeLabel = CreateText("VolumeLabel", drawer.transform, "마스터 볼륨 100%", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), layout.SettingsVolumeLabelSize, layout.SettingsVolumeLabelPosition, new Color(0.3f, 0.2f, 0.14f, 1f));

            GameObject sliderObject = DefaultControls.CreateSlider(new DefaultControls.Resources());
            sliderObject.name = "MasterVolumeSlider";
            sliderObject.transform.SetParent(drawer.transform, false);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.sizeDelta = layout.SettingsSliderSize;
            sliderRect.anchoredPosition = layout.SettingsSliderPosition;
            masterVolumeSlider = sliderObject.GetComponent<Slider>();
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.value = 1f;
            ApplyLobbySliderStyle(masterVolumeSlider);

            closeButton = CreateButton("CloseButton", drawer.transform, "닫기", new Vector2(1f, 1f), new Vector2(1f, 1f), layout.SidePanelCloseButtonSize, layout.SidePanelCloseButtonPosition, new Color(0.61f, 0.43f, 0.29f, 1f), 20, out _, SecondaryButtonSpritePath);
            panel.SetActive(false);
        }

        private static void ConfigureLobbyController(
            LobbySceneController controller,
            TMP_Text coinLabel,
            Image[] upgradeIcons,
            TMP_Text[] upgradeNames,
            TMP_Text[] upgradeDescriptions,
            TMP_Text[] upgradeLevels,
            TMP_Text[] upgradeButtonLabels,
            Button[] upgradeButtons,
            Image[] equipmentRows,
            Image[] equipmentIcons,
            TMP_Text[] equipmentNames,
            TMP_Text[] equipmentDescriptions,
            TMP_Text[] equipmentButtonLabels,
            Button[] equipmentButtons,
            Button[] equipmentSlotButtons,
            Button startButton,
            Button upgradeMenuButton,
            Button equipmentMenuButton,
            Button settingsButton,
            GameObject upgradePanel,
            Button closeUpgradeButton,
            GameObject equipmentPanel,
            Button closeEquipmentButton,
            GameObject settingsPanel,
            Slider masterVolumeSlider,
            TMP_Text masterVolumeLabel,
            Button closeSettingsButton)
        {
            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("_catalog").objectReferenceValue = AssetDatabase.LoadAssetAtPath<MetaProgressionCatalog>(MetaCatalogAssetPath);
            serializedController.FindProperty("_coinLabel").objectReferenceValue = coinLabel;
            SetObjectReferences(serializedController.FindProperty("_upgradeIcons"), upgradeIcons);
            SetObjectReferences(serializedController.FindProperty("_upgradeNameLabels"), upgradeNames);
            SetObjectReferences(serializedController.FindProperty("_upgradeDescriptionLabels"), upgradeDescriptions);
            SetObjectReferences(serializedController.FindProperty("_upgradeLevelLabels"), upgradeLevels);
            SetObjectReferences(serializedController.FindProperty("_upgradeButtonLabels"), upgradeButtonLabels);
            SetObjectReferences(serializedController.FindProperty("_upgradeButtons"), upgradeButtons);
            SetObjectReferences(serializedController.FindProperty("_equipmentRowBackgrounds"), equipmentRows);
            serializedController.FindProperty("_equipmentSlotSprite").objectReferenceValue = LoadUiSprite(MetaSlotSpritePath, SlotSpriteBorder);
            serializedController.FindProperty("_selectedEquipmentSlotSprite").objectReferenceValue = LoadUiSprite(MetaSlotSelectedSpritePath, SlotSpriteBorder);
            SetObjectReferences(serializedController.FindProperty("_equipmentIcons"), equipmentIcons);
            SetObjectReferences(serializedController.FindProperty("_equipmentNameLabels"), equipmentNames);
            SetObjectReferences(serializedController.FindProperty("_equipmentDescriptionLabels"), equipmentDescriptions);
            SetObjectReferences(serializedController.FindProperty("_equipmentButtonLabels"), equipmentButtonLabels);
            SetObjectReferences(serializedController.FindProperty("_equipmentButtons"), equipmentButtons);
            SetObjectReferences(serializedController.FindProperty("_equipmentSlotButtons"), equipmentSlotButtons);
            serializedController.FindProperty("_startButton").objectReferenceValue = startButton;
            serializedController.FindProperty("_upgradeMenuButton").objectReferenceValue = upgradeMenuButton;
            serializedController.FindProperty("_equipmentMenuButton").objectReferenceValue = equipmentMenuButton;
            serializedController.FindProperty("_settingsButton").objectReferenceValue = settingsButton;
            serializedController.FindProperty("_upgradePanel").objectReferenceValue = upgradePanel;
            serializedController.FindProperty("_closeUpgradeButton").objectReferenceValue = closeUpgradeButton;
            serializedController.FindProperty("_equipmentPanel").objectReferenceValue = equipmentPanel;
            serializedController.FindProperty("_closeEquipmentButton").objectReferenceValue = closeEquipmentButton;
            serializedController.FindProperty("_settingsPanel").objectReferenceValue = settingsPanel;
            serializedController.FindProperty("_masterVolumeSlider").objectReferenceValue = masterVolumeSlider;
            serializedController.FindProperty("_masterVolumeLabel").objectReferenceValue = masterVolumeLabel;
            serializedController.FindProperty("_closeSettingsButton").objectReferenceValue = closeSettingsButton;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureLobbyStageController(
            LobbySceneController controller,
            TMP_Text coinLabel,
            Button startButton)
        {
            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("_coinLabel").objectReferenceValue = coinLabel;
            serializedController.FindProperty("_startButton").objectReferenceValue = startButton;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureLobbyNavigationController(
            LobbySceneController controller,
            Button upgradeMenuButton,
            Button equipmentMenuButton,
            Button settingsButton)
        {
            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("_upgradeMenuButton").objectReferenceValue = upgradeMenuButton;
            serializedController.FindProperty("_equipmentMenuButton").objectReferenceValue = equipmentMenuButton;
            serializedController.FindProperty("_settingsButton").objectReferenceValue = settingsButton;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureLobbyUpgradePanelController(
            LobbySceneController controller,
            MetaProgressionCatalog catalog,
            GameObject panel,
            Button closeButton,
            Image[] icons,
            TMP_Text[] names,
            TMP_Text[] descriptions,
            TMP_Text[] levels,
            TMP_Text[] buttonLabels,
            Button[] buttons)
        {
            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("_catalog").objectReferenceValue = catalog;
            serializedController.FindProperty("_upgradePanel").objectReferenceValue = panel;
            serializedController.FindProperty("_closeUpgradeButton").objectReferenceValue = closeButton;
            SetObjectReferences(serializedController.FindProperty("_upgradeIcons"), icons);
            SetObjectReferences(serializedController.FindProperty("_upgradeNameLabels"), names);
            SetObjectReferences(serializedController.FindProperty("_upgradeDescriptionLabels"), descriptions);
            SetObjectReferences(serializedController.FindProperty("_upgradeLevelLabels"), levels);
            SetObjectReferences(serializedController.FindProperty("_upgradeButtonLabels"), buttonLabels);
            SetObjectReferences(serializedController.FindProperty("_upgradeButtons"), buttons);
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureLobbyEquipmentPanelController(
            LobbySceneController controller,
            MetaProgressionCatalog catalog,
            GameObject panel,
            Button closeButton,
            Button[] slotButtons,
            Image[] rows,
            Image[] icons,
            TMP_Text[] names,
            TMP_Text[] descriptions,
            TMP_Text[] buttonLabels,
            Button[] buttons)
        {
            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("_catalog").objectReferenceValue = catalog;
            serializedController.FindProperty("_equipmentPanel").objectReferenceValue = panel;
            serializedController.FindProperty("_closeEquipmentButton").objectReferenceValue = closeButton;
            SetObjectReferences(serializedController.FindProperty("_equipmentSlotButtons"), slotButtons);
            SetObjectReferences(serializedController.FindProperty("_equipmentRowBackgrounds"), rows);
            SetObjectReferences(serializedController.FindProperty("_equipmentIcons"), icons);
            SetObjectReferences(serializedController.FindProperty("_equipmentNameLabels"), names);
            SetObjectReferences(serializedController.FindProperty("_equipmentDescriptionLabels"), descriptions);
            SetObjectReferences(serializedController.FindProperty("_equipmentButtonLabels"), buttonLabels);
            SetObjectReferences(serializedController.FindProperty("_equipmentButtons"), buttons);
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureLobbySettingsPanelController(
            LobbySceneController controller,
            GameObject panel,
            Slider volumeSlider,
            TMP_Text volumeLabel,
            Button closeButton)
        {
            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("_settingsPanel").objectReferenceValue = panel;
            serializedController.FindProperty("_masterVolumeSlider").objectReferenceValue = volumeSlider;
            serializedController.FindProperty("_masterVolumeLabel").objectReferenceValue = volumeLabel;
            serializedController.FindProperty("_closeSettingsButton").objectReferenceValue = closeButton;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void SetObjectReferences<T>(SerializedProperty property, IReadOnlyList<T> values) where T : Object
        {
            property.arraySize = values.Count;
            for (int index = 0; index < values.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position)
        {
            return CreatePanel(name, parent, anchorMin, anchorMax, size, position, new Color(0.98f, 0.9f, 0.76f, 0.96f));
        }

        private static GameObject CreateSidePanel(string name, Transform parent)
        {
            GameObject panel = CreateUiObject(name, parent);
            Stretch(panel.GetComponent<RectTransform>());
            return panel;
        }

        private static GameObject CreateDrawer(string name, Transform parent, LobbyLayoutProfile layout)
        {
            GameObject drawer = CreatePanel(name, parent, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), layout.SidePanelCardSize, layout.SidePanelCardPosition, Color.white);
            ApplySlicedSprite(drawer.GetComponent<Image>(), DrawerPanelSpritePath, DrawerPanelSpriteBorder);
            drawer.AddComponent<LobbyDrawerMotion>();
            return drawer;
        }

        private static GameObject CreateMetaSlot(string name, Transform parent, float topOffset, string spritePath, LobbyLayoutProfile layout)
        {
            GameObject slot = CreatePanel(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), layout.SidePanelRowSize, new Vector2(0f, -topOffset), Color.white);
            ApplySlicedSprite(slot.GetComponent<Image>(), spritePath, SlotSpriteBorder);
            return slot;
        }

        private static void ApplyLobbySliderStyle(Slider slider)
        {
            Image[] images = slider.GetComponentsInChildren<Image>();
            for (int index = 0; index < images.Length; index++)
            {
                Image image = images[index];
                if (image.gameObject.name == "Background")
                {
                    image.color = new Color(0.28f, 0.16f, 0.1f, 1f);
                }
                else if (image.gameObject.name == "Fill")
                {
                    image.color = new Color(0.92f, 0.59f, 0.18f, 1f);
                }
                else if (image.gameObject.name == "Handle")
                {
                    image.color = new Color(1f, 0.82f, 0.39f, 1f);
                }
            }
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position, Color color)
        {
            GameObject panel = CreateUiObject(name, parent);
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position,
            Color color,
            int fontSize,
            out TMP_Text labelText,
            string spritePath = SecondaryButtonSpritePath)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;

            Image image = buttonObject.AddComponent<Image>();
            ApplySlicedSprite(image, spritePath, ButtonSpriteBorder);
            if (image.sprite == null)
            {
                image.color = color;
            }

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            buttonObject.AddComponent<LobbyButtonPressFeedback>();
            labelText = CreateText("Label", buttonObject.transform, label, fontSize, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            labelText.font = LoadLobbyBoldFont();
            labelText.fontStyle = FontStyles.Normal;
            ApplyTextOutline(labelText, new Color(0f, 0f, 0f, 0.72f));
            return button;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position,
            Color color)
        {
            GameObject textObject = CreateUiObject(name, parent);
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            float pivotX = Mathf.Approximately(anchorMin.x, anchorMax.x) ? anchorMin.x : 0.5f;
            rectTransform.pivot = new Vector2(pivotX, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;

            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = fontSize >= 26 ? LoadLobbyBoldFont() : LoadLobbyFont();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Normal;
            text.alignment = ToTextAlignment(alignment);
            text.color = color;
            text.richText = false;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            return text;
        }

        private static TextAlignmentOptions ToTextAlignment(TextAnchor alignment)
        {
            return alignment switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.MidlineLeft,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Midline,
                TextAnchor.MiddleRight => TextAlignmentOptions.MidlineRight,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.Midline,
            };
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject uiObject = new(name, typeof(RectTransform), typeof(CanvasRenderer));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            InputActionAsset actionsAsset = GetOrCreateLobbyUiInputActions();
            InputActionMap uiMap = actionsAsset.FindActionMap("UI", true);
            GameObject eventSystemObject = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            inputModule.actionsAsset = actionsAsset;
            inputModule.point = GetOrCreateActionReference(actionsAsset, "LobbyUiPoint", uiMap.FindAction("Point", true));
            inputModule.leftClick = GetOrCreateActionReference(actionsAsset, "LobbyUiLeftClick", uiMap.FindAction("LeftClick", true));
        }

        private static InputActionAsset GetOrCreateLobbyUiInputActions()
        {
            InputActionAsset actionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(LobbyUiInputActionsAssetPath);
            if (actionsAsset != null)
            {
                InputAction existingLeftClick = actionsAsset.FindAction("UI/LeftClick", false);
                if (existingLeftClick != null && existingLeftClick.type == InputActionType.PassThrough)
                {
                    return actionsAsset;
                }

                AssetDatabase.DeleteAsset(LobbyUiInputActionsAssetPath);
            }

            actionsAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            actionsAsset.name = "LobbyUiInputActions";
            InputActionMap uiMap = actionsAsset.AddActionMap("UI");
            InputAction point = uiMap.AddAction("Point", InputActionType.PassThrough);
            point.AddBinding("<Pointer>/position");
            InputAction leftClick = uiMap.AddAction("LeftClick", InputActionType.PassThrough);
            leftClick.AddBinding("<Pointer>/press");
            AssetDatabase.CreateAsset(actionsAsset, LobbyUiInputActionsAssetPath);
            EditorUtility.SetDirty(actionsAsset);
            return actionsAsset;
        }

        private static InputActionReference GetOrCreateActionReference(
            InputActionAsset actionsAsset,
            string referenceName,
            InputAction action)
        {
            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(LobbyUiInputActionsAssetPath);
            for (int index = 0; index < subAssets.Length; index++)
            {
                if (subAssets[index] is InputActionReference reference && reference.name == referenceName)
                {
                    return reference;
                }
            }

            InputActionReference newReference = InputActionReference.Create(action);
            newReference.name = referenceName;
            AssetDatabase.AddObjectToAsset(newReference, actionsAsset);
            EditorUtility.SetDirty(newReference);
            return newReference;
        }

        private static void EnsureLobbyIsFirstBuildScene()
        {
            List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
            scenes.RemoveAll(scene => scene.path == LobbyScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(LobbyScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void EnsureLobbyPresentationFolders()
        {
            EnsureFolder("Assets/_Project/Art/UI");
            EnsureFolder(LobbyUiAssetRootPath);
            EnsureFolder(LobbyUiAssetRootPath + "/Backgrounds");
            EnsureFolder(LobbyUiAssetRootPath + "/Buttons");
            EnsureFolder(LobbyUiAssetRootPath + "/Panels");
            EnsureFolder("Assets/_Project/Art/UI/Fonts");
            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder("Assets/_Project/Prefabs/UI");
            EnsureFolder("Assets/_Project/Prefabs/UI/Lobby");
            EnsureFolder(LobbyModulePrefabFolderPath);
            EnsureFolder(LobbyModulePrefabFolderPath + "/Stage");
            EnsureFolder(LobbyModulePrefabFolderPath + "/Navigation");
            EnsureFolder(LobbyModulePrefabFolderPath + "/Panels");
        }

        private static void SaveLobbyPrefab(GameObject source, string assetPath)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(source, assetPath, InteractionMode.AutomatedAction);
        }

        private static bool TryGetLobbySceneContext(
            out Scene scene,
            out Transform canvasTransform,
            out LobbySceneController controller)
        {
            scene = SceneManager.GetActiveScene();
            canvasTransform = null;
            controller = null;

            if (scene.path != LobbyScenePath)
            {
                Debug.LogError("[LobbySceneInstaller] 개별 적용은 LobbyScene을 연 상태에서만 실행할 수 있습니다.");
                return false;
            }

            controller = Object.FindFirstObjectByType<LobbySceneController>();
            if (controller == null)
            {
                Debug.LogError("[LobbySceneInstaller] LobbySceneController를 찾지 못했습니다.");
                return false;
            }

            canvasTransform = controller.transform;
            return true;
        }

        private static void RemoveLobbyModule(Transform parent, string moduleName)
        {
            Transform module = parent.Find(moduleName);
            if (module != null)
            {
                Object.DestroyImmediate(module.gameObject);
            }
        }

        private static void SaveLobbyScene(Scene scene)
        {
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            int separatorIndex = folderPath.LastIndexOf('/');
            string parentFolder = folderPath[..separatorIndex];
            string folderName = folderPath[(separatorIndex + 1)..];
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        private readonly struct MetaRunItems
        {
            public MetaRunItems(
                RunItemDefinition attackDamageUpgrade,
                RunItemDefinition attackSpeedUpgrade,
                RunItemDefinition forwardSpeedUpgrade,
                RunItemDefinition hardenedAcornEquipment,
                RunItemDefinition windupSlingshotEquipment,
                RunItemDefinition longTailScopeEquipment)
            {
                AttackDamageUpgrade = attackDamageUpgrade;
                AttackSpeedUpgrade = attackSpeedUpgrade;
                ForwardSpeedUpgrade = forwardSpeedUpgrade;
                HardenedAcornEquipment = hardenedAcornEquipment;
                WindupSlingshotEquipment = windupSlingshotEquipment;
                LongTailScopeEquipment = longTailScopeEquipment;
            }

            public RunItemDefinition AttackDamageUpgrade { get; }
            public RunItemDefinition AttackSpeedUpgrade { get; }
            public RunItemDefinition ForwardSpeedUpgrade { get; }
            public RunItemDefinition HardenedAcornEquipment { get; }
            public RunItemDefinition WindupSlingshotEquipment { get; }
            public RunItemDefinition LongTailScopeEquipment { get; }

            public RunItemDefinition[] UpgradeDefinitions => new[]
            {
                AttackDamageUpgrade,
                AttackSpeedUpgrade,
                ForwardSpeedUpgrade,
            };

            public RunItemDefinition[] EquipmentDefinitions => new[]
            {
                HardenedAcornEquipment,
                WindupSlingshotEquipment,
                LongTailScopeEquipment,
            };
        }
    }
}
#endif
