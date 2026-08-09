#if UNITY_EDITOR
using CatMouse.Game.Enemy;
using CatMouse.Game.Player;
using CatMouse.Game.Presentation;
using CatMouse.Game.Run;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatMouse.Game.Editor
{
    public static class DevPantryVisualSliceInstaller
    {
        private const string DevScenePath = "Assets/_Project/Scenes/Development/Dev_PantryBackground.unity";
        private const string GameScenePath = "Assets/_Project/Scenes/Gameplay/GameScene.unity";
        private const string BackgroundAssetRoot = "Assets/_Project/Art/Backgrounds/Gameplay";
        private const string WallSpritePath = BackgroundAssetRoot + "/PantryCellar_BackWall_Tile_v1.png";
        private const string FloorSpritePath = BackgroundAssetRoot + "/PantryCellar_CombatFloor_Tile_v1.png";
        private const string ForegroundSpritePath = BackgroundAssetRoot + "/PantryCellar_ForegroundWall_Tile_v1.png";
        private const string GroundShadowSpritePath = "Assets/_Project/Art/Presentation/Effects/GroundShadow.png";
        private const string AcornSpritePath = "Assets/_Project/Art/Sprites/Projectiles/AcornProjectile_v1.png";
        private const string CheeseSpritePath = "Assets/_Project/Art/Sprites/Pickups/CheesePickup_v1.png";
        private const string CoinSpritePath = "Assets/_Project/Art/Sprites/Pickups/AcornCoinPickup_v1.png";
        private const string PickupSparkleSpritePath = "Assets/_Project/Art/Presentation/Effects/PickupSparkle_v1.png";
        private const string ImpactSpritePath = "Assets/_Project/Art/Presentation/Effects/AcornImpact_v1.png";
        private const float BackgroundPixelsPerUnit = 96f;
        private const float ProjectilePixelsPerUnit = 768f;
        private const float PickupPixelsPerUnit = 512f;
        private const float EffectPixelsPerUnit = 512f;

        [MenuItem("CatMouse/Development/Install Pantry Visual Slice")]
        public static void Install()
        {
            InstallScene(DevScenePath, true);
        }

        [MenuItem("CatMouse/Gameplay/Install Run Visual Presentation")]
        public static void InstallGameScene()
        {
            InstallScene(GameScenePath, false);
        }

        private static void InstallScene(string scenePath, bool installDevelopmentShadows)
        {
            ImportBackgroundSprites();

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Sprite wallSprite = LoadSprite(WallSpritePath);
            Sprite floorSprite = LoadSprite(FloorSpritePath);
            Sprite foregroundSprite = LoadSprite(ForegroundSpritePath);
            Sprite shadowSprite = LoadSprite(GroundShadowSpritePath);
            Sprite acornSprite = LoadSprite(AcornSpritePath);
            Sprite cheeseSprite = LoadSprite(CheeseSpritePath);
            Sprite coinSprite = LoadSprite(CoinSpritePath);
            Sprite pickupSparkleSprite = LoadSprite(PickupSparkleSpritePath);
            Sprite impactSprite = LoadSprite(ImpactSpritePath);
            if (wallSprite == null || floorSprite == null || foregroundSprite == null || shadowSprite == null
                || acornSprite == null || cheeseSprite == null || coinSprite == null
                || pickupSparkleSprite == null || impactSprite == null)
            {
                Debug.LogError("[DevPantryVisualSliceInstaller] 필요한 시각 에셋을 불러오지 못했습니다.");
                return;
            }

            ConfigureTiledLayer("FarBackgroundLayer", wallSprite);
            ConfigureTiledLayer("GameplayFloorLayer", floorSprite);
            ConfigureTiledLayer("ForegroundBaseboardLayer", foregroundSprite);
            DisableKenneyGridOverlays();

            if (installDevelopmentShadows)
            {
                EnsureGroundShadow("TestMouse", shadowSprite);
                EnsureGroundShadow("PrototypeEnemyTemplate", shadowSprite);
            }

            ConfigureProjectileSprites(acornSprite);
            ConfigurePickupSprites(cheeseSprite, coinSprite);
            ConfigureVisualEffectReferences(pickupSparkleSprite, impactSprite);

            PantryVerticalLayout layout = Object.FindFirstObjectByType<PantryVerticalLayout>();
            layout?.ApplyLayout();

            EndlessPantryBackground[] backgrounds = Object.FindObjectsByType<EndlessPantryBackground>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < backgrounds.Length; index++)
            {
                backgrounds[index].LayoutTilesAroundCamera();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[DevPantryVisualSliceInstaller] 식료품 저장실 시각 완성 구간을 적용했습니다.");
        }

        private static void ImportBackgroundSprites()
        {
            ConfigureSpriteImport(WallSpritePath, false);
            ConfigureSpriteImport(FloorSpritePath, false);
            ConfigureSpriteImport(ForegroundSpritePath, true);
            ConfigureSpriteImport(AcornSpritePath, true, ProjectilePixelsPerUnit);
            ConfigureSpriteImport(CheeseSpritePath, true, PickupPixelsPerUnit, TextureWrapMode.Clamp);
            ConfigureSpriteImport(CoinSpritePath, true, PickupPixelsPerUnit, TextureWrapMode.Clamp);
            ConfigureSpriteImport(PickupSparkleSpritePath, true, EffectPixelsPerUnit, TextureWrapMode.Clamp);
            ConfigureSpriteImport(ImpactSpritePath, true, EffectPixelsPerUnit, TextureWrapMode.Clamp);
        }

        private static void ConfigureSpriteImport(string assetPath, bool hasTransparency)
        {
            ConfigureSpriteImport(assetPath, hasTransparency, BackgroundPixelsPerUnit);
        }

        private static void ConfigureSpriteImport(string assetPath, bool hasTransparency, float pixelsPerUnit)
        {
            ConfigureSpriteImport(assetPath, hasTransparency, pixelsPerUnit, TextureWrapMode.Repeat);
        }

        private static void ConfigureSpriteImport(
            string assetPath,
            bool hasTransparency,
            float pixelsPerUnit,
            TextureWrapMode wrapMode)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.alphaIsTransparency = hasTransparency;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = wrapMode;
            importer.SaveAndReimport();
        }

        private static Sprite LoadSprite(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void ConfigureTiledLayer(string layerName, Sprite sprite)
        {
            Transform layer = FindTransform(layerName);
            if (layer == null)
            {
                Debug.LogError($"[DevPantryVisualSliceInstaller] {layerName} 레이어를 찾지 못했습니다.");
                return;
            }

            SpriteRenderer[] renderers = layer.GetComponentsInChildren<SpriteRenderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                SpriteRenderer renderer = renderers[index];
                if (renderer == null || renderer.name == "KenneyGridOverlay")
                {
                    continue;
                }

                renderer.sprite = sprite;
                renderer.drawMode = SpriteDrawMode.Simple;
                renderer.color = Color.white;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static void DisableKenneyGridOverlays()
        {
            Transform[] transforms = Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < transforms.Length; index++)
            {
                Transform transform = transforms[index];
                if (transform != null && transform.name == "KenneyGridOverlay")
                {
                    transform.gameObject.SetActive(false);
                }
            }
        }

        private static void ConfigureProjectileSprites(Sprite sprite)
        {
            PrototypeAcornProjectile[] prototypeProjectiles = Object.FindObjectsByType<PrototypeAcornProjectile>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < prototypeProjectiles.Length; index++)
            {
                SetPresentationSprite(prototypeProjectiles[index], sprite);
            }

            PlayerSceneAcornProjectile[] runProjectiles = Object.FindObjectsByType<PlayerSceneAcornProjectile>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < runProjectiles.Length; index++)
            {
                SetPresentationSprite(runProjectiles[index], sprite);
            }
        }

        private static void ConfigurePickupSprites(Sprite cheeseSprite, Sprite coinSprite)
        {
            PrototypeCheeseDrop[] cheeseDrops = Object.FindObjectsByType<PrototypeCheeseDrop>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < cheeseDrops.Length; index++)
            {
                SetPresentationSprite(cheeseDrops[index], cheeseSprite);
            }

            RunCoinPickup[] coins = Object.FindObjectsByType<RunCoinPickup>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < coins.Length; index++)
            {
                SetPresentationSprite(coins[index], coinSprite);
            }
        }

        private static void ConfigureVisualEffectReferences(Sprite pickupSparkleSprite, Sprite impactSprite)
        {
            RunVisualEffectPool visualEffectPool = Object.FindFirstObjectByType<RunVisualEffectPool>();
            if (visualEffectPool == null)
            {
                visualEffectPool = new GameObject("RunVisualEffects").AddComponent<RunVisualEffectPool>();
            }

            SetObjectReference(visualEffectPool, "_effectRoot", visualEffectPool.transform);
            SetObjectReference(visualEffectPool, "_pickupSparkleSprite", pickupSparkleSprite);
            SetObjectReference(visualEffectPool, "_impactSprite", impactSprite);

            PrototypeCheeseDropPool[] cheesePools = Object.FindObjectsByType<PrototypeCheeseDropPool>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < cheesePools.Length; index++)
            {
                SetObjectReference(cheesePools[index], "_visualEffectPool", visualEffectPool);
            }

            RunCoinCollector[] coinCollectors = Object.FindObjectsByType<RunCoinCollector>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < coinCollectors.Length; index++)
            {
                SetObjectReference(coinCollectors[index], "_visualEffectPool", visualEffectPool);
            }

            PrototypeAcornProjectile[] prototypeProjectiles = Object.FindObjectsByType<PrototypeAcornProjectile>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < prototypeProjectiles.Length; index++)
            {
                SetObjectReference(prototypeProjectiles[index], "_visualEffectPool", visualEffectPool);
            }

            PlayerSceneAcornProjectile[] runProjectiles = Object.FindObjectsByType<PlayerSceneAcornProjectile>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < runProjectiles.Length; index++)
            {
                SetObjectReference(runProjectiles[index], "_visualEffectPool", visualEffectPool);
            }
        }

        private static void SetPresentationSprite(Object target, Sprite sprite)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty presentationSprite = serializedObject.FindProperty("_presentationSprite");
            if (presentationSprite == null)
            {
                return;
            }

            presentationSprite.objectReferenceValue = sprite;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void EnsureGroundShadow(string actorName, Sprite shadowSprite)
        {
            Transform actor = FindTransform(actorName);
            if (actor == null)
            {
                Debug.LogWarning($"[DevPantryVisualSliceInstaller] {actorName}을(를) 찾지 못해 그림자를 생략했습니다.");
                return;
            }

            Transform shadow = actor.Find("GroundShadow");
            if (shadow == null)
            {
                shadow = new GameObject("GroundShadow").transform;
                shadow.SetParent(actor, false);
            }

            shadow.localPosition = new Vector3(0f, 0.05f, 0f);
            shadow.localScale = new Vector3(0.55f, 0.16f, 1f);

            SpriteRenderer shadowRenderer = shadow.GetComponent<SpriteRenderer>();
            if (shadowRenderer == null)
            {
                shadowRenderer = shadow.gameObject.AddComponent<SpriteRenderer>();
            }

            SpriteRenderer actorRenderer = actor.GetComponentInChildren<SpriteRenderer>(true);
            shadowRenderer.sprite = shadowSprite;
            shadowRenderer.color = new Color(1f, 1f, 1f, 0.24f);
            shadowRenderer.sortingLayerID = actorRenderer != null
                ? actorRenderer.sortingLayerID
                : shadowRenderer.sortingLayerID;

            SortingOrderOffset orderOffset = shadow.GetComponent<SortingOrderOffset>();
            if (orderOffset == null)
            {
                orderOffset = shadow.gameObject.AddComponent<SortingOrderOffset>();
            }

            orderOffset.Configure(-1);
            EditorUtility.SetDirty(shadowRenderer);
        }

        private static Transform FindTransform(string objectName)
        {
            Transform[] transforms = Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int index = 0; index < transforms.Length; index++)
            {
                Transform transform = transforms[index];
                if (transform != null && transform.name == objectName)
                {
                    return transform;
                }
            }

            return null;
        }
    }
}
#endif
