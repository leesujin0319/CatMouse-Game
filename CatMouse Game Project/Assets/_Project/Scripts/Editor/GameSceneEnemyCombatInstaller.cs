#if UNITY_EDITOR
using CatMouse.Game.Enemy;
using CatMouse.Game.Player;
using CatMouse.Game.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatMouse.Game.Editor
{
    public static class GameSceneEnemyCombatInstaller
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Gameplay/GameScene.unity";
        private const string GroundShadowAssetPath = "Assets/_Project/Art/Presentation/Effects/GroundShadow.png";
        private const string DefaultArchetypePath = "Assets/_Project/Data/Run/Enemies/PrototypeCat_Default.asset";
        private const string SwiftArchetypePath = "Assets/_Project/Data/Run/Enemies/PrototypeCat_Swift.asset";
        private const string TankArchetypePath = "Assets/_Project/Data/Run/Enemies/PrototypeCat_Tank.asset";
        private const string RangedArchetypePath = "Assets/_Project/Data/Run/Enemies/PrototypeCat_Ranged.asset";
        private const string Wave000Path = "Assets/_Project/Data/Run/PrototypeWave_000.asset";
        private const string Wave080Path = "Assets/_Project/Data/Run/PrototypeWave_080.asset";
        private const string Wave180Path = "Assets/_Project/Data/Run/PrototypeWave_180.asset";
        private const string CharacterSortingLayer = "Characters";
        private const string GroundShadowName = "GroundShadow";
        private const float GroundShadowPixelsPerUnit = 192f;

        private static readonly Color GroundShadowColor = new(1f, 1f, 1f, 0.24f);
        private static readonly Vector3 GroundShadowLocalPosition = new(0f, 0.03f, 0f);
        private static readonly Vector3 GroundShadowLocalScale = new(0.6f, 0.42f, 1f);

        [MenuItem("CatMouse/Gameplay/Install GameScene Enemy Combat")]
        public static void Install()
        {
            EnemyArchetypeDefinition rangedArchetype = EnsureRangedArchetype();
            Sprite groundShadow = GetGroundShadowSprite();
            if (rangedArchetype == null || groundShadow == null)
            {
                return;
            }

            ConfigureSpawnPatterns(rangedArchetype);

            Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            PrototypeEnemySpawner spawner = Object.FindFirstObjectByType<PrototypeEnemySpawner>();
            PlayerRunHealth playerHealth = Object.FindFirstObjectByType<PlayerRunHealth>();
            if (spawner == null || playerHealth == null)
            {
                Debug.LogError("[GameSceneEnemyCombatInstaller] GameScene의 적 스포너 또는 플레이어 체력을 찾지 못했습니다.");
                return;
            }

            ConfigureSpawner(spawner, playerHealth.transform);
            ConfigureMotion(spawner, playerHealth, groundShadow);

            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(playerHealth.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[GameSceneEnemyCombatInstaller] 적 스폰·공격·그림자를 GameScene에 설치했습니다.");
        }

        private static EnemyArchetypeDefinition EnsureRangedArchetype()
        {
            EnemyArchetypeDefinition ranged = AssetDatabase.LoadAssetAtPath<EnemyArchetypeDefinition>(RangedArchetypePath);
            if (ranged == null)
            {
                EnemyArchetypeDefinition source = AssetDatabase.LoadAssetAtPath<EnemyArchetypeDefinition>(SwiftArchetypePath);
                if (source == null)
                {
                    Debug.LogError($"[GameSceneEnemyCombatInstaller] 원거리 적의 기준 에셋을 찾지 못했습니다: {SwiftArchetypePath}");
                    return null;
                }

                ranged = Object.Instantiate(source);
                ranged.name = "PrototypeCat_Ranged";
                AssetDatabase.CreateAsset(ranged, RangedArchetypePath);
            }

            SerializedObject serializedRanged = new(ranged);
            serializedRanged.FindProperty("_maximumHealth").intValue = 2;
            serializedRanged.FindProperty("_contactDamage").intValue = 4;
            serializedRanged.FindProperty("_speedMultiplier").floatValue = 0.85f;
            serializedRanged.FindProperty("_attackType").enumValueIndex = (int)EnemyAttackType.Ranged;
            serializedRanged.FindProperty("_rangedAttackRange").floatValue = 6f;
            serializedRanged.FindProperty("_rangedAttackCooldown").floatValue = 2.25f;
            serializedRanged.FindProperty("_rangedProjectileSpeed").floatValue = 5.5f;
            serializedRanged.FindProperty("_rangedProjectileDamage").intValue = 7;
            serializedRanged.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(ranged);
            return ranged;
        }

        private static void ConfigureSpawnPatterns(EnemyArchetypeDefinition rangedArchetype)
        {
            EnemyArchetypeDefinition defaultArchetype = AssetDatabase.LoadAssetAtPath<EnemyArchetypeDefinition>(DefaultArchetypePath);
            EnemyArchetypeDefinition swiftArchetype = AssetDatabase.LoadAssetAtPath<EnemyArchetypeDefinition>(SwiftArchetypePath);
            EnemyArchetypeDefinition tankArchetype = AssetDatabase.LoadAssetAtPath<EnemyArchetypeDefinition>(TankArchetypePath);
            if (defaultArchetype == null || swiftArchetype == null || tankArchetype == null)
            {
                Debug.LogError("[GameSceneEnemyCombatInstaller] 적 아키타입을 찾지 못했습니다.");
                return;
            }

            ConfigurePattern(Wave000Path, new[] { defaultArchetype, swiftArchetype }, new[] { -1.05f, 1.05f }, 0.8f);
            ConfigurePattern(Wave080Path, new[] { swiftArchetype, rangedArchetype, defaultArchetype }, new[] { 1.15f, -1.15f, 0f }, 0.9f);
            ConfigurePattern(Wave180Path, new[] { tankArchetype, rangedArchetype, swiftArchetype }, new[] { 0f, -1.55f, 1.55f }, 0.95f);
        }

        private static void ConfigurePattern(
            string patternPath,
            EnemyArchetypeDefinition[] enemyArchetypes,
            float[] verticalOffsets,
            float horizontalSpacing)
        {
            SpawnPatternDefinition pattern = AssetDatabase.LoadAssetAtPath<SpawnPatternDefinition>(patternPath);
            if (pattern == null)
            {
                Debug.LogError($"[GameSceneEnemyCombatInstaller] 스폰 패턴을 찾지 못했습니다: {patternPath}");
                return;
            }

            SerializedObject serializedPattern = new(pattern);
            serializedPattern.FindProperty("_enemyCount").intValue = enemyArchetypes.Length;
            serializedPattern.FindProperty("_horizontalSpacing").floatValue = horizontalSpacing;

            SerializedProperty archetypes = serializedPattern.FindProperty("_enemyArchetypes");
            archetypes.arraySize = enemyArchetypes.Length;
            for (int index = 0; index < enemyArchetypes.Length; index++)
            {
                archetypes.GetArrayElementAtIndex(index).objectReferenceValue = enemyArchetypes[index];
            }

            SerializedProperty offsets = serializedPattern.FindProperty("_verticalOffsets");
            offsets.arraySize = verticalOffsets.Length;
            for (int index = 0; index < verticalOffsets.Length; index++)
            {
                offsets.GetArrayElementAtIndex(index).floatValue = verticalOffsets[index];
            }

            serializedPattern.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pattern);
        }

        private static Sprite GetGroundShadowSprite()
        {
            TextureImporter importer = AssetImporter.GetAtPath(GroundShadowAssetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[GameSceneEnemyCombatInstaller] 그림자 이미지를 찾지 못했습니다: {GroundShadowAssetPath}");
                return null;
            }

            if (importer.textureType != TextureImporterType.Sprite
                || importer.spriteImportMode != SpriteImportMode.Single
                || !Mathf.Approximately(importer.spritePixelsPerUnit, GroundShadowPixelsPerUnit)
                || !importer.alphaIsTransparency
                || importer.filterMode != FilterMode.Bilinear)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = GroundShadowPixelsPerUnit;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(GroundShadowAssetPath);
        }

        private static void ConfigureSpawner(PrototypeEnemySpawner spawner, Transform playerTransform)
        {
            SerializedObject serializedSpawner = new(spawner);
            Transform enemyRoot = serializedSpawner.FindProperty("_enemyRoot").objectReferenceValue as Transform;
            PrototypeEnemyMover enemyTemplate = serializedSpawner.FindProperty("_enemyTemplate").objectReferenceValue as PrototypeEnemyMover;
            if (enemyRoot == null || enemyTemplate == null)
            {
                Debug.LogError("[GameSceneEnemyCombatInstaller] 적 루트 또는 템플릿 참조를 찾지 못했습니다.");
                return;
            }

            Transform projectileRoot = enemyRoot.Find("EnemyProjectiles");
            if (projectileRoot == null)
            {
                projectileRoot = new GameObject("EnemyProjectiles").transform;
                projectileRoot.SetParent(enemyRoot, false);
            }

            PrototypeEnemyProjectile projectileTemplate = projectileRoot.GetComponentInChildren<PrototypeEnemyProjectile>(true);
            if (projectileTemplate == null)
            {
                GameObject templateObject = new GameObject("EnemyProjectileTemplate");
                templateObject.transform.SetParent(projectileRoot, false);
                projectileTemplate = templateObject.AddComponent<PrototypeEnemyProjectile>();
            }

            projectileTemplate.gameObject.SetActive(false);
            serializedSpawner.FindProperty("_collector").objectReferenceValue = playerTransform;
            serializedSpawner.FindProperty("_enemyProjectileRoot").objectReferenceValue = projectileRoot;
            serializedSpawner.FindProperty("_enemyProjectileTemplate").objectReferenceValue = projectileTemplate;
            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(projectileTemplate);
        }

        private static void ConfigureMotion(
            PrototypeEnemySpawner spawner,
            PlayerRunHealth playerHealth,
            Sprite groundShadow)
        {
            SpriteRenderer playerRenderer = FindCharacterRenderer(playerHealth.transform);
            if (playerRenderer != null)
            {
                RunSpriteMotion playerMotion = playerRenderer.GetComponent<RunSpriteMotion>();
                if (playerMotion == null)
                {
                    playerMotion = playerRenderer.gameObject.AddComponent<RunSpriteMotion>();
                }

                PlayerSceneAutoAttack autoAttack = playerHealth.GetComponent<PlayerSceneAutoAttack>();
                if (autoAttack != null)
                {
                    SerializedObject serializedAutoAttack = new(autoAttack);
                    serializedAutoAttack.FindProperty("_presentation").objectReferenceValue = playerMotion;
                    serializedAutoAttack.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(autoAttack);
                }
            }

            EnsureGroundShadow(playerHealth.transform, groundShadow);

            SerializedObject serializedSpawner = new(spawner);
            PrototypeEnemyMover enemyTemplate = serializedSpawner.FindProperty("_enemyTemplate").objectReferenceValue as PrototypeEnemyMover;
            SpriteRenderer enemyRenderer = enemyTemplate != null
                ? FindCharacterRenderer(enemyTemplate.transform)
                : null;
            if (enemyRenderer == null)
            {
                return;
            }

            RunSpriteMotion enemyMotion = enemyRenderer.GetComponent<RunSpriteMotion>();
            if (enemyMotion == null)
            {
                enemyMotion = enemyRenderer.gameObject.AddComponent<RunSpriteMotion>();
            }

            SerializedObject serializedEnemyMotion = new(enemyMotion);
            serializedEnemyMotion.FindProperty("_attackDirection").floatValue = -1f;
            serializedEnemyMotion.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedEnemy = new(enemyTemplate);
            serializedEnemy.FindProperty("_presentation").objectReferenceValue = enemyMotion;
            serializedEnemy.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(enemyTemplate);
            EnsureGroundShadow(enemyTemplate.transform, groundShadow);
        }

        private static SpriteRenderer FindCharacterRenderer(Transform owner)
        {
            SpriteRenderer[] renderers = owner.GetComponentsInChildren<SpriteRenderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                if (renderers[index].gameObject.name != GroundShadowName)
                {
                    return renderers[index];
                }
            }

            return null;
        }

        private static void EnsureGroundShadow(Transform owner, Sprite shadowSprite)
        {
            Transform shadowTransform = owner.Find(GroundShadowName);
            if (shadowTransform == null)
            {
                shadowTransform = new GameObject(GroundShadowName).transform;
                shadowTransform.SetParent(owner, false);
            }

            shadowTransform.localPosition = GroundShadowLocalPosition;
            shadowTransform.localRotation = Quaternion.identity;
            shadowTransform.localScale = GroundShadowLocalScale;
            shadowTransform.SetAsFirstSibling();

            SpriteRenderer shadowRenderer = shadowTransform.GetComponent<SpriteRenderer>();
            if (shadowRenderer == null)
            {
                shadowRenderer = shadowTransform.gameObject.AddComponent<SpriteRenderer>();
            }

            shadowRenderer.sprite = shadowSprite;
            shadowRenderer.color = GroundShadowColor;
            shadowRenderer.sortingLayerName = CharacterSortingLayer;

            SortingOrderOffset sortingOffset = shadowTransform.GetComponent<SortingOrderOffset>();
            if (sortingOffset == null)
            {
                sortingOffset = shadowTransform.gameObject.AddComponent<SortingOrderOffset>();
            }

            RunSpriteMotion shadowMotion = shadowTransform.GetComponent<RunSpriteMotion>();
            if (shadowMotion != null)
            {
                Object.DestroyImmediate(shadowMotion);
            }

            sortingOffset.Configure(-1);
            owner.GetComponent<YAxisSortingOrder>()?.RefreshRenderers();
            EditorUtility.SetDirty(shadowTransform.gameObject);
        }
    }
}
#endif
