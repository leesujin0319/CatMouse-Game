#if UNITY_EDITOR
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
    public static class CardSceneCoinPickupInstaller
    {
        private const string ScenePath = "Assets/_Project/Scenes/Development/CardScene.unity";

        [MenuItem("CatMouse/Development/Install CardScene Coin Pickups")]
        public static void Install()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Camera worldCamera = Camera.main;
            RunVerticalBounds verticalBounds = Object.FindFirstObjectByType<RunVerticalBounds>();
            RunProgressController runProgress = Object.FindFirstObjectByType<RunProgressController>();
            PlayerRunHealth runHealth = Object.FindFirstObjectByType<PlayerRunHealth>();
            PlayerRunGameOverController gameOverController =
                Object.FindFirstObjectByType<PlayerRunGameOverController>();
            RunDistanceHudView distanceHud = Object.FindFirstObjectByType<RunDistanceHudView>();

            if (worldCamera == null
                || verticalBounds == null
                || runProgress == null
                || runHealth == null
                || gameOverController == null
                || distanceHud == null)
            {
                Debug.LogError("[CardSceneCoinPickupInstaller] Required CardScene components are missing.");
                return;
            }

            RunCoinCollector coinCollector = Object.FindFirstObjectByType<RunCoinCollector>();
            if (coinCollector == null)
            {
                GameObject collectorObject = new GameObject("RunCoinCollector");
                coinCollector = collectorObject.AddComponent<RunCoinCollector>();
            }

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

            SerializedObject serializedCollector = new SerializedObject(coinCollector);
            serializedCollector.FindProperty("_worldCamera").objectReferenceValue = worldCamera;
            serializedCollector.FindProperty("_verticalBounds").objectReferenceValue = verticalBounds;
            serializedCollector.FindProperty("_runProgress").objectReferenceValue = runProgress;
            serializedCollector.FindProperty("_collector").objectReferenceValue = runHealth.transform;
            serializedCollector.FindProperty("_coinRoot").objectReferenceValue = coinRoot;
            serializedCollector.FindProperty("_coinTemplate").objectReferenceValue = coinTemplate;
            serializedCollector.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedGameOver = new SerializedObject(gameOverController);
            serializedGameOver.FindProperty("_runCoinCollector").objectReferenceValue = coinCollector;
            serializedGameOver.ApplyModifiedPropertiesWithoutUndo();


            ConfigureCoinHud(distanceHud.transform, coinCollector);
            EditorUtility.SetDirty(coinCollector);
            EditorUtility.SetDirty(gameOverController);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[CardSceneCoinPickupInstaller] CardScene coin pickups configured.");
        }

        private static void ConfigureCoinHud(Transform hudRoot, RunCoinCollector coinCollector)
        {
            Transform panelTransform = hudRoot.Find("CoinPanel");
            GameObject panelObject;
            if (panelTransform == null)
            {
                panelObject = new GameObject(
                    "CoinPanel",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(RunCoinHudView));
                panelObject.transform.SetParent(hudRoot, false);
            }
            else
            {
                panelObject = panelTransform.gameObject;
            }

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-24f, -24f);
            panelRect.sizeDelta = new Vector2(210f, 62f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.3f, 0.18f, 0.04f, 0.86f);
            panelImage.raycastTarget = false;

            Transform labelTransform = panelObject.transform.Find("CoinText");
            Text coinLabel;
            if (labelTransform == null)
            {
                GameObject labelObject = new GameObject(
                    "CoinText",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text));
                labelObject.transform.SetParent(panelObject.transform, false);
                coinLabel = labelObject.GetComponent<Text>();
            }
            else
            {
                coinLabel = labelTransform.GetComponent<Text>();
            }

            RectTransform labelRect = coinLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            coinLabel.font = AssetDatabase.LoadAssetAtPath<Font>(
                "Assets/_Project/Font/MemomentKkukkukk.ttf");
            coinLabel.fontSize = 34;
            coinLabel.alignment = TextAnchor.MiddleCenter;
            coinLabel.color = new Color(1f, 0.82f, 0.2f, 1f);
            coinLabel.raycastTarget = false;
            coinLabel.text = "0000 C";

            RunCoinHudView coinHud = panelObject.GetComponent<RunCoinHudView>();
            SerializedObject serializedCoinHud = new SerializedObject(coinHud);
            serializedCoinHud.FindProperty("_coinCollector").objectReferenceValue = coinCollector;
            serializedCoinHud.FindProperty("_coinLabel").objectReferenceValue = coinLabel;
            serializedCoinHud.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(coinHud);
        }
    }
}
#endif
