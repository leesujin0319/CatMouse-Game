#if UNITY_EDITOR
using System.Collections.Generic;
using CatMouse.Game.Meta;
using CatMouse.Game.Player;
using CatMouse.Game.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatMouse.Game.Editor
{
    public static class GameSceneMetaStatusInstaller
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Gameplay/GameScene.unity";
        private const string MetaCatalogPath = "Assets/_Project/Data/Meta/MetaProgressionCatalog.asset";
        private const string PrefabFolderPath = "Assets/_Project/Prefabs/UI/Run";
        private const string PrefabPath = PrefabFolderPath + "/RunMetaStatusPanel.prefab";
        private const string TmpFontAssetPath = "Assets/_Project/Art/UI/Fonts/TMP/NEXONLv1GothicBold_TMP.asset";
        private const string StatusFontCharacters = "성장정보장착장비영구강화최종적용능력치닫기모자갑옷신발미공격력속도범위투사체전진상하이동레벨최대";
        private const int UpgradeSlotCount = 5;

        private static readonly Vector2 PanelSize = new(1120f, 700f);
        private static readonly Color PanelColor = new(0.08f, 0.12f, 0.17f, 0.96f);
        private static readonly Color SlotColor = new(0.11f, 0.17f, 0.22f, 1f);
        private static readonly Color AccentColor = new(0.96f, 0.72f, 0.25f, 1f);
        private static readonly Color TextColor = new(0.96f, 0.97f, 0.93f, 1f);
        private static readonly Color SecondaryTextColor = new(0.7f, 0.82f, 0.86f, 1f);

        private static TMP_FontAsset _statusFont;

        [MenuItem("CatMouse/Gameplay/Install GameScene Meta Status Panel")]
        public static void Install()
        {
            MetaProgressionCatalog catalog = AssetDatabase.LoadAssetAtPath<MetaProgressionCatalog>(MetaCatalogPath);
            if (catalog == null)
            {
                Debug.LogError($"[GameSceneMetaStatusInstaller] 메타 카탈로그를 찾지 못했습니다: {MetaCatalogPath}");
                return;
            }

            EnsureFolder(PrefabFolderPath);
            CreateOrReplacePrefab();

            Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            PlayerRunStats runStats = UnityEngine.Object.FindFirstObjectByType<PlayerRunStats>();
            Canvas hudCanvas = FindHudCanvas();
            if (runStats == null || hudCanvas == null)
            {
                Debug.LogError("[GameSceneMetaStatusInstaller] PlayerRunStats 또는 PlayerRunHud Canvas를 찾지 못했습니다.");
                return;
            }

            Transform existing = hudCanvas.transform.Find("RunMetaStatusPanel");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, hudCanvas.transform) as GameObject;
            instance.name = "RunMetaStatusPanel";
            instance.transform.SetAsLastSibling();

            RunMetaStatusView view = instance.GetComponent<RunMetaStatusView>();
            SerializedObject serializedView = new(view);
            serializedView.FindProperty("_runStats").objectReferenceValue = runStats;
            serializedView.FindProperty("_catalog").objectReferenceValue = catalog;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(view);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[GameSceneMetaStatusInstaller] GameScene 성장 정보창을 고정 UI 프리팹으로 설치했습니다.");
        }

        private static void CreateOrReplacePrefab()
        {
            GameObject root = CreateUiObject("RunMetaStatusPanel", null);
            Stretch(root.GetComponent<RectTransform>());

            RunMetaStatusView view = root.AddComponent<RunMetaStatusView>();
            Button openButton = CreateButton(
                "OpenButton",
                root.transform,
                "성장 정보",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(180f, 64f),
                new Vector2(-124f, -172f),
                AccentColor,
                new Color(0.12f, 0.16f, 0.18f, 1f),
                28);

            GameObject panel = CreatePanel(
                "Panel",
                root.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                PanelSize,
                Vector2.zero,
                PanelColor);
            CreateText(
                "Title",
                panel.transform,
                "성장 정보",
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(560f, 64f),
                new Vector2(0f, -48f),
                42,
                TextAnchor.MiddleCenter,
                TextColor);
            CreateText(
                "Guide",
                panel.transform,
                "장착 장비와 영구 강화가 반영된 현재 능력치입니다.",
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(920f, 42f),
                new Vector2(0f, -96f),
                22,
                TextAnchor.MiddleCenter,
                SecondaryTextColor);
            Button closeButton = CreateButton(
                "CloseButton",
                panel.transform,
                "닫기",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(116f, 52f),
                new Vector2(-76f, -52f),
                new Color(0.32f, 0.43f, 0.5f, 1f),
                TextColor,
                22);

            CreateSectionHeader(panel.transform, "EquipmentHeader", "장착 장비", new Vector2(-270f, 206f), new Vector2(480f, 46f));
            TMP_Text[] equipmentLabels = new TMP_Text[3];
            for (int index = 0; index < equipmentLabels.Length; index++)
            {
                equipmentLabels[index] = CreateListEntry(
                    panel.transform,
                    $"Equipment_{index + 1}",
                    new Vector2(-270f, 135f - index * 64f),
                    new Vector2(480f, 54f));
            }

            CreateSectionHeader(panel.transform, "UpgradeHeader", "영구 강화", new Vector2(270f, 206f), new Vector2(480f, 46f));
            TMP_Text[] upgradeLabels = new TMP_Text[UpgradeSlotCount];
            for (int index = 0; index < upgradeLabels.Length; index++)
            {
                upgradeLabels[index] = CreateListEntry(
                    panel.transform,
                    $"Upgrade_{index + 1}",
                    new Vector2(270f, 140f - index * 49f),
                    new Vector2(480f, 40f));
            }

            CreateSectionHeader(panel.transform, "StatsHeader", "최종 적용 능력치", new Vector2(0f, -132f), new Vector2(960f, 46f));
            TMP_Text[] statValues = new TMP_Text[6];
            for (int index = 0; index < statValues.Length; index++)
            {
                int column = index % 3;
                int row = index / 3;
                statValues[index] = CreateStatRow(
                    panel.transform,
                    $"StatRow_{index + 1}",
                    new Vector2(-320f + column * 320f, -205f - row * 70f));
            }

            panel.SetActive(false);
            ConfigureView(
                view,
                openButton,
                panel,
                closeButton,
                equipmentLabels,
                upgradeLabels,
                statValues);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static Canvas FindHudCanvas()
        {
            Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int index = 0; index < canvases.Length; index++)
            {
                if (canvases[index].name == "PlayerRunHud")
                {
                    return canvases[index];
                }
            }

            return null;
        }

        private static void ConfigureView(
            RunMetaStatusView view,
            Button openButton,
            GameObject panel,
            Button closeButton,
            TMP_Text[] equipmentLabels,
            TMP_Text[] upgradeLabels,
            TMP_Text[] statValues)
        {
            SerializedObject serializedView = new(view);
            serializedView.FindProperty("_openButton").objectReferenceValue = openButton;
            serializedView.FindProperty("_panel").objectReferenceValue = panel;
            serializedView.FindProperty("_closeButton").objectReferenceValue = closeButton;
            SetObjectReferences(serializedView.FindProperty("_equipmentLabels"), equipmentLabels);
            SetObjectReferences(serializedView.FindProperty("_upgradeLabels"), upgradeLabels);
            SetObjectReferences(serializedView.FindProperty("_statValueLabels"), statValues);
            serializedView.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TMP_Text CreateListEntry(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size)
        {
            GameObject slot = CreatePanel(
                name,
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                size,
                position,
                SlotColor);
            TMP_Text label = CreateText(
                "Value",
                slot.transform,
                string.Empty,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                20,
                TextAnchor.MiddleLeft,
                TextColor);
            RectTransform labelRect = label.rectTransform;
            labelRect.offsetMin = new Vector2(18f, 0f);
            labelRect.offsetMax = new Vector2(-18f, 0f);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }

        private static TMP_Text CreateStatRow(
            Transform parent,
            string name,
            Vector2 position)
        {
            GameObject row = CreatePanel(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 58f), position, SlotColor);
            TMP_Text value = CreateText("Value", row.transform, "공격력  0", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 21, TextAnchor.MiddleCenter, TextColor);
            value.textWrappingMode = TextWrappingModes.NoWrap;
            value.overflowMode = TextOverflowModes.Ellipsis;
            return value;
        }

        private static void CreateSectionHeader(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            CreateText(name, parent, label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position, 26, TextAnchor.MiddleLeft, AccentColor);
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position,
            Color backgroundColor,
            Color textColor,
            int fontSize)
        {
            GameObject buttonObject = CreatePanel(name, parent, anchorMin, anchorMax, size, position, backgroundColor);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();
            CreateText("Label", buttonObject.transform, label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, fontSize, TextAnchor.MiddleCenter, textColor);
            return button;
        }

        private static GameObject CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position,
            Color color)
        {
            GameObject panel = CreateUiObject(name, parent);
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;

            Image image = panel.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = color;
            return panel;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            string value,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            GameObject textObject = CreateUiObject(name, parent);
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;

            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = LoadStatusFont();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Normal;
            text.alignment = ToTextAlignment(alignment);
            text.color = color;
            text.richText = false;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        private static TMP_FontAsset LoadStatusFont()
        {
            if (_statusFont == null)
            {
                _statusFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontAssetPath);
                if (_statusFont != null)
                {
                    _statusFont.TryAddCharacters(StatusFontCharacters, out _, true);
                    EditorUtility.SetDirty(_statusFont);
                }
            }

            return _statusFont != null ? _statusFont : TMP_Settings.defaultFontAsset;
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
            if (parent != null)
            {
                uiObject.transform.SetParent(parent, false);
            }

            return uiObject;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void SetObjectReferences<T>(SerializedProperty property, IReadOnlyList<T> values)
            where T : UnityEngine.Object
        {
            property.arraySize = values.Count;
            for (int index = 0; index < values.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }
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
    }
}
#endif
