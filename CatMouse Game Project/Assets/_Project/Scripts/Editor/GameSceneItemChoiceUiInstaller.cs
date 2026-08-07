#if UNITY_EDITOR
using System.Collections.Generic;
using CatMouse.Game.Run;
using CatMouse.Game.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatMouse.Game.Editor
{
    public static class GameSceneItemChoiceUiInstaller
    {
        private const string PrefabFolderPath = "Assets/_Project/Prefabs/UI/Run";
        private const string PrefabPath = PrefabFolderPath + "/RunItemChoicePanel.prefab";
        private const string ModalSpritePath = "Assets/_Project/Art/UI/Lobby/Panels/LobbyModalPanel_v1.png";
        private const string CardSpritePath = "Assets/_Project/Art/UI/Lobby/Buttons/LobbyNavButton_Cropped_v1.png";
        private const string IconSpritePath = "Assets/_Project/Art/UI/Lobby/Icons/LobbyIcon_Upgrade_v1.png";
        private const string FontPath = "Assets/_Project/Art/UI/Fonts/TMP/NEXONLv1GothicBold_TMP.asset";
        private const string PanelName = "PlayerRunItemChoicePanel";

        private static readonly string[] ScenePaths =
        {
            "Assets/_Project/Scenes/Gameplay/GameScene.unity",
            "Assets/_Project/Scenes/Development/CardScene.unity",
            "Assets/_Project/Scenes/Development/PlayerScene.unity",
        };

        private static readonly Color OverlayColor = new(0.06f, 0.05f, 0.04f, 0.7f);
        private static readonly Color TitleColor = new(0.25f, 0.12f, 0.05f, 1f);
        private static readonly Color DescriptionColor = new(0.35f, 0.24f, 0.16f, 1f);
        private static readonly Color StackColor = new(0.46f, 0.3f, 0.17f, 1f);
        private static readonly Color AccentColor = new(0.78f, 0.42f, 0.11f, 1f);

        [MenuItem("CatMouse/Gameplay/Install Run Item Choice UI")]
        public static void Install()
        {
            EnsureFolder(PrefabFolderPath);
            CreateOrReplacePrefab();

            for (int index = 0; index < ScenePaths.Length; index++)
            {
                InstallInScene(ScenePaths[index]);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[GameSceneItemChoiceUiInstaller] 런 아이템 선택 UI를 패널 프리팹으로 설치했습니다.");
        }

        private static void InstallInScene(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            RunItemChoiceController controller = Object.FindFirstObjectByType<RunItemChoiceController>();
            Canvas canvas = FindHudCanvas();
            if (controller == null || canvas == null)
            {
                return;
            }

            Transform existing = canvas.transform.Find(PanelName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            GameObject panel = PrefabUtility.InstantiatePrefab(prefab, canvas.transform) as GameObject;
            panel.name = PanelName;
            panel.transform.SetAsLastSibling();

            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("_choiceView").objectReferenceValue = panel.GetComponent<RunItemChoiceView>();
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void CreateOrReplacePrefab()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            Sprite modalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ModalSpritePath);
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CardSpritePath);
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(IconSpritePath);
            if (font == null || modalSprite == null || cardSprite == null || iconSprite == null)
            {
                Debug.LogError("[GameSceneItemChoiceUiInstaller] 카드 UI에 필요한 폰트 또는 스프라이트를 찾을 수 없습니다.");
                return;
            }

            GameObject root = CreateUiObject(PanelName, null);
            Stretch(root.GetComponent<RectTransform>());
            Image overlay = root.AddComponent<Image>();
            overlay.color = OverlayColor;
            RunItemChoiceView view = root.AddComponent<RunItemChoiceView>();

            GameObject board = CreateImage(
                "ChoiceBoard",
                root.transform,
                modalSprite,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(1560f, 900f),
                Vector2.zero,
                Color.white);

            CreateText(
                "Title",
                board.transform,
                "레벨 업",
                font,
                56,
                FontStyles.Bold,
                TitleColor,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(640f, 76f),
                new Vector2(0f, -102f));
            CreateText(
                "Guide",
                board.transform,
                "성장할 능력을 하나 선택하세요",
                font,
                26,
                FontStyles.Normal,
                DescriptionColor,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(760f, 44f),
                new Vector2(0f, -162f));

            RunItemChoiceCardView[] cards = new RunItemChoiceCardView[3];
            for (int index = 0; index < cards.Length; index++)
            {
                cards[index] = CreateCard(
                    board.transform,
                    font,
                    cardSprite,
                    iconSprite,
                    index,
                    new Vector2((index - 1) * 450f, -64f));
            }

            SerializedObject serializedView = new(view);
            SerializedProperty cardsProperty = serializedView.FindProperty("_cards");
            cardsProperty.arraySize = cards.Length;
            for (int index = 0; index < cards.Length; index++)
            {
                cardsProperty.GetArrayElementAtIndex(index).objectReferenceValue = cards[index];
            }

            serializedView.ApplyModifiedPropertiesWithoutUndo();
            root.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }

        private static RunItemChoiceCardView CreateCard(
            Transform parent,
            TMP_FontAsset font,
            Sprite cardSprite,
            Sprite iconSprite,
            int index,
            Vector2 position)
        {
            GameObject card = CreateImage(
                $"ItemCard_{index + 1}",
                parent,
                cardSprite,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(400f, 470f),
                position,
                Color.white);
            Button selectButton = card.AddComponent<Button>();
            selectButton.targetGraphic = card.GetComponent<Image>();

            CreateImage(
                "Icon",
                card.transform,
                iconSprite,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(88f, 88f),
                new Vector2(0f, -68f),
                Color.white);
            TMP_Text name = CreateText(
                "ItemName",
                card.transform,
                "능력 이름",
                font,
                34,
                FontStyles.Bold,
                TitleColor,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(324f, 58f),
                new Vector2(0f, -138f));
            TMP_Text description = CreateText(
                "ItemDescription",
                card.transform,
                "능력 설명",
                font,
                23,
                FontStyles.Normal,
                DescriptionColor,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(308f, 104f),
                new Vector2(0f, -4f));
            TMP_Text stack = CreateText(
                "StackLabel",
                card.transform,
                "보유 0 / 0",
                font,
                21,
                FontStyles.Normal,
                StackColor,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(280f, 42f),
                new Vector2(0f, 108f));

            GameObject selectHint = CreateImage(
                "SelectHint",
                card.transform,
                cardSprite,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(236f, 58f),
                new Vector2(0f, 38f),
                new Color(1f, 0.84f, 0.61f, 1f));
            CreateText(
                "Label",
                selectHint.transform,
                "선택",
                font,
                24,
                FontStyles.Bold,
                AccentColor,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            RunItemChoiceCardView cardView = card.AddComponent<RunItemChoiceCardView>();
            SerializedObject serializedCard = new(cardView);
            serializedCard.FindProperty("_selectButton").objectReferenceValue = selectButton;
            serializedCard.FindProperty("_nameLabel").objectReferenceValue = name;
            serializedCard.FindProperty("_descriptionLabel").objectReferenceValue = description;
            serializedCard.FindProperty("_stackLabel").objectReferenceValue = stack;
            serializedCard.ApplyModifiedPropertiesWithoutUndo();
            return cardView;
        }

        private static Canvas FindHudCanvas()
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int index = 0; index < canvases.Length; index++)
            {
                if (canvases[index].name == "PlayerRunHud")
                {
                    return canvases[index];
                }
            }

            return canvases.Length > 0 ? canvases[0] : null;
        }

        private static GameObject CreateImage(
            string name,
            Transform parent,
            Sprite sprite,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position,
            Color color)
        {
            GameObject imageObject = CreateUiObject(name, parent);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            return imageObject;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            string value,
            TMP_FontAsset font,
            float fontSize,
            FontStyles style,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position)
        {
            GameObject textObject = CreateUiObject(name, parent);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
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

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            int separator = folderPath.LastIndexOf('/');
            EnsureFolder(folderPath[..separator]);
            AssetDatabase.CreateFolder(folderPath[..separator], folderPath[(separator + 1)..]);
        }
    }
}
#endif
