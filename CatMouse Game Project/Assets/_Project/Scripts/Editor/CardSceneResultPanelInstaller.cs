#if UNITY_EDITOR
using CatMouse.Game.Player;
using CatMouse.Game.Run;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatMouse.Game.Editor
{
    public static class CardSceneResultPanelInstaller
    {
        private const string ScenePath = "Assets/_Project/Scenes/Development/CardScene.unity";

        [MenuItem("CatMouse/Development/Install CardScene Result Panel")]
        public static void Install()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            PlayerRunGameOverController controller =
                Object.FindFirstObjectByType<PlayerRunGameOverController>();
            RunProgressController runProgress =
                Object.FindFirstObjectByType<RunProgressController>();
            if (controller == null || runProgress == null)
            {
                Debug.LogError("[CardSceneResultPanelInstaller] Required CardScene components are missing.");
                return;
            }

            SerializedObject serializedController = new SerializedObject(controller);
            GameObject panel =
                serializedController.FindProperty("_gameOverPanel").objectReferenceValue as GameObject;
            if (panel == null)
            {
                Debug.LogError("[CardSceneResultPanelInstaller] Result panel reference is missing.");
                return;
            }

            ClearChildren(panel.transform);
            Image overlayImage = panel.GetComponent<Image>();
            if (overlayImage != null)
            {
                overlayImage.color = new Color(0.08f, 0.04f, 0.08f, 0.78f);
            }

            GameObject resultCard = CreateUiObject("ResultCard", panel.transform);
            RectTransform cardRect = resultCard.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(700f, 620f);

            Image cardImage = resultCard.AddComponent<Image>();
            cardImage.color = new Color(1f, 0.91f, 0.82f, 1f);
            Shadow cardShadow = resultCard.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
            cardShadow.effectDistance = new Vector2(0f, -8f);

            CreateText(
                "Title",
                resultCard.transform,
                "\uB7F0 \uACB0\uACFC",
                56,
                new Vector2(0f, 210f));
            CreateText(
                "DistanceLabel",
                resultCard.transform,
                "\uC774\uB3D9 \uAC70\uB9AC",
                28,
                new Vector2(0f, 128f));

            Text distanceValueLabel = CreateText(
                "DistanceValue",
                resultCard.transform,
                "0 m",
                48,
                new Vector2(0f, 74f));

            CreateText(
                "CoinLabel",
                resultCard.transform,
                "\uD68D\uB4DD \uCF54\uC778",
                28,
                Vector2.zero);

            Text coinValueLabel = CreateText(
                "CoinValue",
                resultCard.transform,
                "0\uAC1C",
                48,
                new Vector2(0f, -54f));

            CreateText(
                "NextGoalLabel",
                resultCard.transform,
                "\uB2E4\uC74C \uB808\uBCA8 \uBAA9\uD45C",
                28,
                new Vector2(0f, -128f));

            Text nextGoalValueLabel = CreateText(
                "NextGoalValue",
                resultCard.transform,
                "100 m",
                48,
                new Vector2(0f, -182f));

            Button retryButton = CreateRetryButton(resultCard.transform);

            serializedController.FindProperty("_runProgress").objectReferenceValue = runProgress;
            serializedController.FindProperty("_distanceValueLabel").objectReferenceValue = distanceValueLabel;
            serializedController.FindProperty("_nextGoalValueLabel").objectReferenceValue = nextGoalValueLabel;
            serializedController.FindProperty("_coinValueLabel").objectReferenceValue = coinValueLabel;
            serializedController.FindProperty("_retryButton").objectReferenceValue = retryButton;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            panel.SetActive(false);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[CardSceneResultPanelInstaller] CardScene result panel configured.");
        }

        private static Button CreateRetryButton(Transform parent)
        {
            GameObject buttonObject = CreateUiObject("RetryButton", parent);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(380f, 92f);
            buttonRect.anchoredPosition = new Vector2(0f, -250f);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.98f, 0.63f, 0.68f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            Shadow buttonShadow = buttonObject.AddComponent<Shadow>();
            buttonShadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
            buttonShadow.effectDistance = new Vector2(0f, -5f);

            CreateText(
                "Label",
                buttonObject.transform,
                "\uB2E4\uC2DC \uC2DC\uC791",
                34,
                Vector2.zero);

            return button;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            Vector2 anchoredPosition)
        {
            GameObject textObject = CreateUiObject(objectName, parent);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(600f, 64f);
            textRect.anchoredPosition = anchoredPosition;

            Text text = textObject.AddComponent<Text>();
            text.font = AssetDatabase.LoadAssetAtPath<Font>(
                "Assets/_Project/Font/MemomentKkukkukk.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.18f, 0.1f, 0.16f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static GameObject CreateUiObject(string objectName, Transform parent)
        {
            GameObject uiObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int index = parent.childCount - 1; index >= 0; index--)
            {
                Object.DestroyImmediate(parent.GetChild(index).gameObject);
            }
        }
    }
}
#endif
