#if UNITY_EDITOR
using System.Collections.Generic;
using CatMouse.Game.Player;
using CatMouse.Game.Run;
using CatMouse.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatMouse.Game.Editor
{
    public static class PlayerSceneRunItemChoiceInstaller
    {
        private const string ScenePath = "Assets/_Project/Scenes/Development/PlayerScene.unity";
        private const string ItemFolderPath = "Assets/_Project/Data/Run/Items";
        private const string ChoicePanelName = "PlayerRunItemChoicePanel";
        private const string GameOverPanelName = "PlayerRunGameOverPanel";

        [MenuItem("CatMouse/Development/Install PlayerScene Item Choices")]
        public static void Install()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            PlayerRunStats runStats = Object.FindFirstObjectByType<PlayerRunStats>();
            Canvas hudCanvas = Object.FindFirstObjectByType<Canvas>();
            if (runStats == null || hudCanvas == null)
            {
                Debug.LogError("[PlayerSceneRunItemChoiceInstaller] PlayerRunStats 또는 HUD Canvas를 찾을 수 없습니다.");
                return;
            }

            RunItemDefinition[] itemDefinitions = CreateItemDefinitions();
            PlayerScreenPositionController screenPositionController = ConfigureScreenPositionController(runStats.gameObject);
            RunItemChoiceView choiceView = CreateChoiceView(hudCanvas.transform);
            Button retryButton = CreateGameOverPanel(hudCanvas.transform, out GameObject gameOverPanel);
            RunItemChoiceController controller = runStats.GetComponent<RunItemChoiceController>();
            if (controller == null)
            {
                controller = runStats.gameObject.AddComponent<RunItemChoiceController>();
            }

            ConfigureController(controller, runStats, choiceView, screenPositionController, itemDefinitions);
            ConfigureGameOverController(runStats.gameObject, gameOverPanel, retryButton);
            EnsureEventSystem();
            EnsurePlayerSceneIsInBuildSettings();

            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[PlayerSceneRunItemChoiceInstaller] PlayerScene 아이템 선택 카드 설치를 완료했습니다.");
        }

        private static RunItemDefinition[] CreateItemDefinitions()
        {
            EnsureFolder("Assets/_Project/Data");
            EnsureFolder("Assets/_Project/Data/Run");
            EnsureFolder(ItemFolderPath);

            return new[]
            {
                CreateOrUpdateItem("RunItem_HardAcorn", "단단한 도토리", "공격력 +1", PlayerStatType.AttackDamage, PlayerStatModifierOperation.Flat, 1f),
                CreateOrUpdateItem("RunItem_SharpIncisor", "날카로운 앞니", "공격 속도 +20%", PlayerStatType.AttackSpeed, PlayerStatModifierOperation.Percent, 0.2f),
                CreateOrUpdateItem("RunItem_LongTailSight", "긴 꼬리 조준기", "공격 범위 +25%", PlayerStatType.AttackRange, PlayerStatModifierOperation.Percent, 0.25f),
                CreateOrUpdateItem("RunItem_WindWhisker", "바람 수염", "도토리 속도 +25%", PlayerStatType.ProjectileSpeed, PlayerStatModifierOperation.Percent, 0.25f),
                CreateOrUpdateItem("RunItem_RunCheese", "달리기 치즈", "3초 동안 전진 속도 +15%", PlayerStatType.ForwardSpeed, PlayerStatModifierOperation.Percent, 0.15f, 3f),
                CreateOrUpdateItem("RunItem_SpringCheese", "통통 치즈", "상하 이동 속도 +15%", PlayerStatType.VerticalSpeed, PlayerStatModifierOperation.Percent, 0.15f),
            };
        }

        private static RunItemDefinition CreateOrUpdateItem(
            string assetName,
            string displayName,
            string description,
            PlayerStatType statType,
            PlayerStatModifierOperation operation,
            float value,
            float temporaryDuration = 0f)
        {
            string assetPath = $"{ItemFolderPath}/{assetName}.asset";
            RunItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<RunItemDefinition>(assetPath);
            if (itemDefinition == null)
            {
                itemDefinition = ScriptableObject.CreateInstance<RunItemDefinition>();
                AssetDatabase.CreateAsset(itemDefinition, assetPath);
            }

            SerializedObject serializedItem = new(itemDefinition);
            serializedItem.FindProperty("_displayName").stringValue = displayName;
            serializedItem.FindProperty("_description").stringValue = description;
            serializedItem.FindProperty("_maximumStacks").intValue = 10;
            serializedItem.FindProperty("_temporaryDuration").floatValue = temporaryDuration;

            SerializedProperty modifiers = serializedItem.FindProperty("_modifiers");
            modifiers.arraySize = 1;
            SerializedProperty modifier = modifiers.GetArrayElementAtIndex(0);
            modifier.FindPropertyRelative("_statType").enumValueIndex = (int)statType;
            modifier.FindPropertyRelative("_operation").enumValueIndex = (int)operation;
            modifier.FindPropertyRelative("_value").floatValue = value;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static RunItemChoiceView CreateChoiceView(Transform hudParent)
        {
            Transform existing = hudParent.Find(ChoicePanelName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject panel = CreateUiObject(ChoicePanelName, hudParent);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            Stretch(panelRect);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.08f, 0.05f, 0.1f, 0.72f);

            RunItemChoiceView choiceView = panel.AddComponent<RunItemChoiceView>();
            CreateText("Title", panel.transform, "아이템 선택", 48, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(760f, 70f), new Vector2(0f, -150f));
            CreateText("Guide", panel.transform, "하나를 선택하면 능력치가 즉시 상승합니다", 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(900f, 46f), new Vector2(0f, -215f));

            RunItemChoiceCardView[] cards = new RunItemChoiceCardView[3];
            for (int index = 0; index < cards.Length; index++)
            {
                cards[index] = CreateCard(panel.transform, index - 1);
            }

            SerializedObject serializedView = new(choiceView);
            SerializedProperty cardsProperty = serializedView.FindProperty("_cards");
            cardsProperty.arraySize = cards.Length;
            for (int index = 0; index < cards.Length; index++)
            {
                cardsProperty.GetArrayElementAtIndex(index).objectReferenceValue = cards[index];
            }

            serializedView.ApplyModifiedPropertiesWithoutUndo();
            panel.SetActive(false);
            return choiceView;
        }

        private static RunItemChoiceCardView CreateCard(Transform parent, int horizontalIndex)
        {
            GameObject card = CreateUiObject($"ItemCard_{horizontalIndex + 2}", parent);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(420f, 430f);
            cardRect.anchoredPosition = new Vector2(horizontalIndex * 470f, -40f);

            Image cardImage = card.AddComponent<Image>();
            cardImage.color = horizontalIndex switch
            {
                -1 => new Color(0.96f, 0.83f, 0.62f, 1f),
                0 => new Color(0.77f, 0.86f, 0.97f, 1f),
                _ => new Color(0.82f, 0.95f, 0.8f, 1f),
            };

            Button button = card.AddComponent<Button>();
            Shadow shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.25f);
            shadow.effectDistance = new Vector2(0f, -8f);

            CreateText("ItemName", card.transform, "아이템 이름", 36, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(360f, 80f), new Vector2(0f, -84f));
            CreateText("ItemDescription", card.transform, "능력치 설명", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(340f, 150f), Vector2.zero);
            CreateText("StackLabel", card.transform, "보유 0 / 10", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(340f, 50f), new Vector2(0f, 72f));
            CreateText("SelectLabel", card.transform, "선택", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(220f, 54f), new Vector2(0f, 18f));

            RunItemChoiceCardView cardView = card.AddComponent<RunItemChoiceCardView>();
            SerializedObject serializedCard = new(cardView);
            serializedCard.FindProperty("_selectButton").objectReferenceValue = button;
            serializedCard.FindProperty("_nameLabel").objectReferenceValue = card.transform.Find("ItemName").GetComponent<Text>();
            serializedCard.FindProperty("_descriptionLabel").objectReferenceValue = card.transform.Find("ItemDescription").GetComponent<Text>();
            serializedCard.FindProperty("_stackLabel").objectReferenceValue = card.transform.Find("StackLabel").GetComponent<Text>();
            serializedCard.ApplyModifiedPropertiesWithoutUndo();
            return cardView;
        }

        private static void ConfigureController(
            RunItemChoiceController controller,
            PlayerRunStats runStats,
            RunItemChoiceView choiceView,
            PlayerScreenPositionController screenPositionController,
            IReadOnlyList<RunItemDefinition> itemDefinitions)
        {
            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("_runStats").objectReferenceValue = runStats;
            serializedController.FindProperty("_choiceView").objectReferenceValue = choiceView;
            serializedController.FindProperty("_screenPositionController").objectReferenceValue = screenPositionController;

            SerializedProperty itemsProperty = serializedController.FindProperty("_availableItems");
            itemsProperty.arraySize = itemDefinitions.Count;
            for (int index = 0; index < itemDefinitions.Count; index++)
            {
                itemsProperty.GetArrayElementAtIndex(index).objectReferenceValue = itemDefinitions[index];
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PlayerScreenPositionController ConfigureScreenPositionController(GameObject playerObject)
        {
            global::Player player = playerObject.GetComponent<global::Player>();
            PlayerRunStats runStats = playerObject.GetComponent<PlayerRunStats>();
            Camera worldCamera = Object.FindFirstObjectByType<Camera>();
            PlayerScreenPositionController controller = playerObject.GetComponent<PlayerScreenPositionController>();
            if (controller == null)
            {
                controller = playerObject.AddComponent<PlayerScreenPositionController>();
            }

            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("_player").objectReferenceValue = player;
            serializedController.FindProperty("_runStats").objectReferenceValue = runStats;
            serializedController.FindProperty("_worldCamera").objectReferenceValue = worldCamera;
            serializedController.FindProperty("_leftViewportX").floatValue = 0.25f;
            serializedController.FindProperty("_centerViewportX").floatValue = 0.5f;
            serializedController.FindProperty("_baseForwardSpeed").floatValue = 3.5f;
            serializedController.FindProperty("_forwardSpeedForCenter").floatValue = 7f;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            if (player != null && worldCamera != null)
            {
                Vector3 position = player.transform.position;
                float cameraDistance = Mathf.Abs(worldCamera.transform.position.z - position.z);
                position.x = worldCamera.ViewportToWorldPoint(new Vector3(0.25f, 0.5f, cameraDistance)).x;
                player.transform.position = position;
            }

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static Button CreateGameOverPanel(Transform hudParent, out GameObject panel)
        {
            Transform existing = hudParent.Find(GameOverPanelName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            panel = CreateUiObject(GameOverPanelName, hudParent);
            Stretch(panel.GetComponent<RectTransform>());

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.08f, 0.04f, 0.08f, 0.78f);
            CreateText("Title", panel.transform, "게임 오버", 56, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(700f, 90f), new Vector2(0f, 120f));
            CreateText("Guide", panel.transform, "체력이 모두 소진되었습니다", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 60f), new Vector2(0f, 45f));

            GameObject retryObject = CreateUiObject("RetryButton", panel.transform);
            RectTransform retryRect = retryObject.GetComponent<RectTransform>();
            retryRect.anchorMin = new Vector2(0.5f, 0.5f);
            retryRect.anchorMax = new Vector2(0.5f, 0.5f);
            retryRect.pivot = new Vector2(0.5f, 0.5f);
            retryRect.sizeDelta = new Vector2(360f, 92f);
            retryRect.anchoredPosition = new Vector2(0f, -90f);

            Image retryImage = retryObject.AddComponent<Image>();
            retryImage.color = new Color(0.98f, 0.63f, 0.68f, 1f);
            Button retryButton = retryObject.AddComponent<Button>();
            Shadow retryShadow = retryObject.AddComponent<Shadow>();
            retryShadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
            retryShadow.effectDistance = new Vector2(0f, -5f);
            CreateText("Label", retryObject.transform, "다시 시도", 34, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            panel.SetActive(false);
            return retryButton;
        }

        private static void ConfigureGameOverController(GameObject playerObject, GameObject panel, Button retryButton)
        {
            PlayerRunHealth runHealth = playerObject.GetComponent<PlayerRunHealth>();
            PlayerRunGameOverController gameOverController = playerObject.GetComponent<PlayerRunGameOverController>();
            if (gameOverController == null)
            {
                gameOverController = playerObject.AddComponent<PlayerRunGameOverController>();
            }

            SerializedObject serializedController = new(gameOverController);
            serializedController.FindProperty("_runHealth").objectReferenceValue = runHealth;
            serializedController.FindProperty("_gameOverPanel").objectReferenceValue = panel;
            serializedController.FindProperty("_retryButton").objectReferenceValue = retryButton;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gameOverController);
        }

        private static void EnsurePlayerSceneIsInBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
            for (int index = 0; index < scenes.Count; index++)
            {
                if (scenes[index].path == ScenePath)
                {
                    scenes[index] = new EditorBuildSettingsScene(ScenePath, true);
                    EditorBuildSettings.scenes = scenes.ToArray();
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static GameObject CreateUiObject(string objectName, Transform parent)
        {
            GameObject uiObject = new(objectName, typeof(RectTransform), typeof(CanvasRenderer));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 position)
        {
            GameObject textObject = CreateUiObject(objectName, parent);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = size;
            textRect.anchoredPosition = position;

            Text text = textObject.AddComponent<Text>();
            text.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Font/MemomentKkukkukk.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.18f, 0.1f, 0.16f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
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
