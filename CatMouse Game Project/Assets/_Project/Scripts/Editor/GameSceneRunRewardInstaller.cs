#if UNITY_EDITOR
using CatMouse.Game.Enemy;
using CatMouse.Game.Player;
using CatMouse.Game.Run;
using CatMouse.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatMouse.Game.Editor
{
    public static class GameSceneRunRewardInstaller
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Gameplay/GameScene.unity";
        private const string FontPath = "Assets/_Project/Font/MemomentKkukkukk.ttf";

        [MenuItem("CatMouse/Gameplay/Install GameScene Run Reward")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            Camera worldCamera = Camera.main;
            RunVerticalBounds verticalBounds = Object.FindFirstObjectByType<RunVerticalBounds>();
            RunProgressController runProgress = Object.FindFirstObjectByType<RunProgressController>();
            PlayerRunHealth runHealth = Object.FindFirstObjectByType<PlayerRunHealth>();
            PlayerRunGameOverController gameOverController =
                Object.FindFirstObjectByType<PlayerRunGameOverController>();
            Canvas hudCanvas = FindHudCanvas();

            if (worldCamera == null
                || verticalBounds == null
                || runProgress == null
                || runHealth == null
                || gameOverController == null
                || hudCanvas == null)
            {
                Debug.LogError("[GameSceneRunRewardInstaller] GameScene 런 보상 구성에 필요한 참조를 찾지 못했습니다.");
                return;
            }

            RunCoinCollector coinCollector = GetOrCreateCoinCollector();
            ConfigureCoinCollector(coinCollector, worldCamera, verticalBounds, runProgress, runHealth.transform);
            ConfigureEnemyRewardSpawner(coinCollector);
            ConfigureCoinHud(hudCanvas.transform, coinCollector);
            ConfigureGameOverResult(gameOverController, runProgress, coinCollector);

            EditorUtility.SetDirty(coinCollector);
            EditorUtility.SetDirty(gameOverController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[GameSceneRunRewardInstaller] GameScene 코인 수집과 종료 보상 연결을 완료했습니다.");
        }

        private static RunCoinCollector GetOrCreateCoinCollector()
        {
            RunCoinCollector collector = Object.FindFirstObjectByType<RunCoinCollector>();
            if (collector != null)
            {
                return collector;
            }

            return new GameObject("RunCoinCollector").AddComponent<RunCoinCollector>();
        }

        private static void ConfigureCoinCollector(
            RunCoinCollector coinCollector,
            Camera worldCamera,
            RunVerticalBounds verticalBounds,
            RunProgressController runProgress,
            Transform playerTransform)
        {
            Transform coinRoot = coinCollector.transform.Find("CoinRoot");
            if (coinRoot == null)
            {
                coinRoot = new GameObject("CoinRoot").transform;
                coinRoot.SetParent(coinCollector.transform, false);
            }

            RunCoinPickup coinTemplate = coinRoot.GetComponentInChildren<RunCoinPickup>(true);
            if (coinTemplate == null)
            {
                GameObject templateObject = new GameObject("CoinTemplate");
                templateObject.transform.SetParent(coinRoot, false);
                coinTemplate = templateObject.AddComponent<RunCoinPickup>();
            }

            coinTemplate.gameObject.SetActive(false);

            SerializedObject serializedCollector = new(coinCollector);
            serializedCollector.FindProperty("_worldCamera").objectReferenceValue = worldCamera;
            serializedCollector.FindProperty("_verticalBounds").objectReferenceValue = verticalBounds;
            serializedCollector.FindProperty("_runProgress").objectReferenceValue = runProgress;
            serializedCollector.FindProperty("_collector").objectReferenceValue = playerTransform;
            serializedCollector.FindProperty("_coinRoot").objectReferenceValue = coinRoot;
            serializedCollector.FindProperty("_coinTemplate").objectReferenceValue = coinTemplate;
            serializedCollector.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureEnemyRewardSpawner(RunCoinCollector coinCollector)
        {
            PrototypeEnemySpawner enemySpawner = Object.FindFirstObjectByType<PrototypeEnemySpawner>();
            if (enemySpawner == null)
            {
                return;
            }

            SerializedObject serializedSpawner = new(enemySpawner);
            serializedSpawner.FindProperty("_coinCollector").objectReferenceValue = coinCollector;
            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(enemySpawner);
        }

        private static void ConfigureCoinHud(Transform hudRoot, RunCoinCollector coinCollector)
        {
            Transform panelTransform = hudRoot.Find("CoinPanel");
            GameObject panelObject = panelTransform != null
                ? panelTransform.gameObject
                : CreateUiObject("CoinPanel", hudRoot);
            if (panelObject.GetComponent<Image>() == null)
            {
                panelObject.AddComponent<Image>();
            }

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.one;
            panelRect.anchorMax = Vector2.one;
            panelRect.pivot = Vector2.one;
            panelRect.anchoredPosition = new Vector2(-32f, -32f);
            panelRect.sizeDelta = new Vector2(210f, 62f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.3f, 0.18f, 0.04f, 0.86f);
            panelImage.raycastTarget = false;

            Text coinLabel = FindOrCreateText("CoinText", panelObject.transform);
            SetStretch(coinLabel.GetComponent<RectTransform>());
            coinLabel.text = "0000 C";
            coinLabel.fontSize = 34;
            coinLabel.alignment = TextAnchor.MiddleCenter;
            coinLabel.color = new Color(1f, 0.82f, 0.2f, 1f);

            RunCoinHudView coinHud = panelObject.GetComponent<RunCoinHudView>();
            if (coinHud == null)
            {
                coinHud = panelObject.AddComponent<RunCoinHudView>();
            }

            SerializedObject serializedCoinHud = new(coinHud);
            serializedCoinHud.FindProperty("_coinCollector").objectReferenceValue = coinCollector;
            serializedCoinHud.FindProperty("_coinLabel").objectReferenceValue = coinLabel;
            serializedCoinHud.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(coinHud);
        }

        private static void ConfigureGameOverResult(
            PlayerRunGameOverController controller,
            RunProgressController runProgress,
            RunCoinCollector coinCollector)
        {
            SerializedObject serializedController = new(controller);
            GameObject panel = serializedController.FindProperty("_gameOverPanel").objectReferenceValue as GameObject;
            if (panel == null)
            {
                Debug.LogError("[GameSceneRunRewardInstaller] 게임 오버 패널 참조를 찾지 못했습니다.");
                return;
            }

            Text distanceLabel = FindOrCreateText("DistanceValue", panel.transform);
            ConfigureResultLabel(distanceLabel, new Vector2(0f, -8f), "이동 거리  0 m");

            Text coinCaption = FindOrCreateText("CoinCaption", panel.transform);
            ConfigureResultLabel(coinCaption, new Vector2(0f, -44f), "획득 코인");
            coinCaption.fontSize = 22;

            Text coinLabel = FindOrCreateText("CoinValue", panel.transform);
            ConfigureResultLabel(coinLabel, new Vector2(0f, -76f), "0 C");

            Text goalLabel = FindOrCreateText("NextGoalValue", panel.transform);
            ConfigureResultLabel(goalLabel, new Vector2(0f, -118f), "다음 목표  100 m");

            Button retryButton = serializedController.FindProperty("_retryButton").objectReferenceValue as Button;
            if (retryButton != null)
            {
                retryButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -184f);
            }

            serializedController.FindProperty("_runProgress").objectReferenceValue = runProgress;
            serializedController.FindProperty("_runCoinCollector").objectReferenceValue = coinCollector;
            serializedController.FindProperty("_distanceValueLabel").objectReferenceValue = distanceLabel;
            serializedController.FindProperty("_coinValueLabel").objectReferenceValue = coinLabel;
            serializedController.FindProperty("_nextGoalValueLabel").objectReferenceValue = goalLabel;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            panel.SetActive(false);
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

            return null;
        }

        private static Text FindOrCreateText(string objectName, Transform parent)
        {
            Transform existing = parent.Find(objectName);
            GameObject textObject = existing != null ? existing.gameObject : CreateUiObject(objectName, parent);
            Text text = textObject.GetComponent<Text>();
            if (text == null)
            {
                text = textObject.AddComponent<Text>();
            }

            text.font = AssetDatabase.LoadAssetAtPath<Font>(FontPath)
                ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
            return text;
        }

        private static void ConfigureResultLabel(Text label, Vector2 position, string value)
        {
            RectTransform rectTransform = label.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(720f, 38f);
            rectTransform.anchoredPosition = position;
            label.text = value;
            label.fontSize = 28;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 0.91f, 0.78f, 1f);
        }

        private static GameObject CreateUiObject(string objectName, Transform parent)
        {
            GameObject uiObject = new(objectName, typeof(RectTransform), typeof(CanvasRenderer));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        private static void SetStretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
#endif
