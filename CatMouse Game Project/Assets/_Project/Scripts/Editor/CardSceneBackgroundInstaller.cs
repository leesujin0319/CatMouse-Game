#if UNITY_EDITOR
using CatMouse.Game.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatMouse.Game.Editor
{
    public static class CardSceneBackgroundInstaller
    {
        private const string ScenePath = "Assets/_Project/Scenes/Development/CardScene.unity";
        private const string BackgroundSpritePath = "Assets/_Project/Art/Backgrounds/CardScene/CardScene_Pantry_Background.png";
        private const string BackgroundRootName = "PantryBackgroundRoot";
        private const string BackdropLayerName = "CardSceneBackdropLayer";
        private const int TileCount = 5;
        private const float TileScale = 0.7f;
        private const float BackdropCenterY = 0.5f;

        [MenuItem("CatMouse/Development/Install CardScene Pantry Background")]
        public static void Install()
        {
            Sprite sprite = LoadBackgroundSprite();
            if (sprite == null)
            {
                Debug.LogError("[CardSceneBackgroundInstaller] Background sprite could not be loaded.");
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject backgroundRoot = GameObject.Find(BackgroundRootName);
            GameObject scrollDriverObject = GameObject.Find("ScrollDriver");
            Camera worldCamera = Camera.main;
            if (backgroundRoot == null || scrollDriverObject == null || worldCamera == null)
            {
                Debug.LogError("[CardSceneBackgroundInstaller] CardScene background references are missing.");
                return;
            }

            GameObject backdropLayer = GetOrCreateLayer(backgroundRoot.transform);
            SpriteRenderer[] tiles = ConfigureTiles(backdropLayer.transform, sprite);

            EndlessPantryBackground endlessBackground = backdropLayer.GetComponent<EndlessPantryBackground>();
            if (endlessBackground == null)
            {
                endlessBackground = backdropLayer.AddComponent<EndlessPantryBackground>();
            }

            endlessBackground.Configure(scrollDriverObject.transform, worldCamera, tiles, 1f, true);
            endlessBackground.LayoutTilesAroundCamera();

            HideLegacyVisualLayers(backgroundRoot.transform);

            EditorUtility.SetDirty(backdropLayer);
            EditorUtility.SetDirty(endlessBackground);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[CardSceneBackgroundInstaller] CardScene pantry background configured.");
        }

        private static Sprite LoadBackgroundSprite()
        {
            TextureImporter importer = AssetImporter.GetAtPath(BackgroundSpritePath) as TextureImporter;
            if (importer == null)
            {
                return null;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath);
        }

        private static GameObject GetOrCreateLayer(Transform backgroundRoot)
        {
            Transform existingLayer = backgroundRoot.Find(BackdropLayerName);
            GameObject layer = existingLayer != null
                ? existingLayer.gameObject
                : new GameObject(BackdropLayerName);

            if (existingLayer == null)
            {
                layer.transform.SetParent(backgroundRoot, false);
            }

            layer.transform.localPosition = new Vector3(0f, BackdropCenterY, 0f);
            layer.transform.localRotation = Quaternion.identity;
            layer.transform.localScale = Vector3.one;
            layer.transform.SetAsFirstSibling();
            return layer;
        }

        private static SpriteRenderer[] ConfigureTiles(Transform layer, Sprite sprite)
        {
            SpriteRenderer[] tiles = new SpriteRenderer[TileCount];
            for (int index = 0; index < TileCount; index++)
            {
                Transform tileTransform = layer.Find($"BackdropTile_{index:00}");
                GameObject tile = tileTransform != null
                    ? tileTransform.gameObject
                    : new GameObject($"BackdropTile_{index:00}");

                if (tileTransform == null)
                {
                    tile.transform.SetParent(layer, false);
                }

                tile.transform.localScale = Vector3.one * TileScale;
                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = tile.AddComponent<SpriteRenderer>();
                }

                renderer.sprite = sprite;
                renderer.sortingLayerName = "FarBackground";
                renderer.sortingOrder = -100;
                renderer.color = Color.white;
                renderer.enabled = true;
                tiles[index] = renderer;
                EditorUtility.SetDirty(renderer);
            }

            return tiles;
        }

        private static void HideLegacyVisualLayers(Transform backgroundRoot)
        {
            string[] layerNames =
            {
                "FarBackgroundLayer",
                "GameplayFloorLayer",
                "ForegroundBaseboardLayer",
            };

            for (int layerIndex = 0; layerIndex < layerNames.Length; layerIndex++)
            {
                Transform layer = backgroundRoot.Find(layerNames[layerIndex]);
                if (layer == null)
                {
                    continue;
                }

                SpriteRenderer[] renderers = layer.GetComponentsInChildren<SpriteRenderer>(true);
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    renderers[rendererIndex].enabled = false;
                    EditorUtility.SetDirty(renderers[rendererIndex]);
                }
            }
        }
    }
}
#endif
