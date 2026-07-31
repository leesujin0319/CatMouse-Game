#if UNITY_EDITOR
using System.Collections.Generic;
using CatMouse.Game.Enemy;
using CatMouse.Game.Player;
using CatMouse.Game.Run;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatMouse.Game.Editor
{
    public static class CardSceneCombatEvolutionInstaller
    {
        private const string ScenePath = "Assets/_Project/Scenes/Development/CardScene.unity";
        private const string ItemFolderPath = "Assets/_Project/Data/Run/Items";

        [MenuItem("CatMouse/Development/Install CardScene Combat Evolutions")]
        public static void Install()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            PlayerRunStats runStats = Object.FindFirstObjectByType<PlayerRunStats>();
            PlayerSceneAutoAttack autoAttack = Object.FindFirstObjectByType<PlayerSceneAutoAttack>();
            RunItemChoiceController choiceController = Object.FindFirstObjectByType<RunItemChoiceController>();
            PrototypeEnemySpawner enemySpawner = Object.FindFirstObjectByType<PrototypeEnemySpawner>();
            if (runStats == null || autoAttack == null || choiceController == null || enemySpawner == null)
            {
                Debug.LogError("[CardSceneCombatEvolutionInstaller] CardScene\uC5D0 \uD544\uC694\uD55C \uCEF4\uD3EC\uB10C\uD2B8\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4.");
                return;
            }

            PlayerRunCombatEvolution combatEvolution = runStats.GetComponent<PlayerRunCombatEvolution>();
            if (combatEvolution == null)
            {
                combatEvolution = runStats.gameObject.AddComponent<PlayerRunCombatEvolution>();
            }

            RunItemDefinition[] combatItems = CreateCombatItems();
            ConfigureCombatEvolution(combatEvolution, enemySpawner);
            ConfigureAutoAttack(autoAttack, combatEvolution);
            ConfigureChoiceController(choiceController, combatEvolution, combatItems);
            ConfigureEnemyTemplate(enemySpawner);

            EditorUtility.SetDirty(combatEvolution);
            EditorUtility.SetDirty(autoAttack);
            EditorUtility.SetDirty(choiceController);
            EditorUtility.SetDirty(enemySpawner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[CardSceneCombatEvolutionInstaller] CardScene \uC804\uD22C \uC9C4\uD654 \uAD6C\uC131\uC744 \uC644\uB8CC\uD588\uC2B5\uB2C8\uB2E4.");
        }

        private static RunItemDefinition[] CreateCombatItems()
        {
            EnsureFolder("Assets/_Project/Data");
            EnsureFolder("Assets/_Project/Data/Run");
            EnsureFolder(ItemFolderPath);

            return new[]
            {
                CreateOrUpdateItem("RunItem_GiantAcorn", "\uAC70\uB300 \uB3C4\uD1A0\uB9AC", "\uB3C4\uD1A0\uB9AC \uD06C\uAE30\uC640 \uC801\uC911 \uBC94\uC704\uAC00 \uC99D\uAC00\uD569\uB2C8\uB2E4.", 3, RunCombatEffectType.LargeProjectile),
                CreateOrUpdateItem("RunItem_DoubleAcorn", "\uC30D\uB3C4\uD1A0\uB9AC", "\uCD94\uAC00 \uB3C4\uD1A0\uB9AC 1\uAC1C\uB97C \uBC1C\uC0AC\uD569\uB2C8\uB2E4.", 3, RunCombatEffectType.AdditionalProjectile),
                CreateOrUpdateItem("RunItem_SeekingWhisker", "\uC720\uB3C4 \uC218\uC5FC", "\uAC00\uC7A5 \uAC00\uAE4C\uC6B4 \uC801\uC744 \uCD94\uC801\uD558\uB294 \uB3C4\uD1A0\uB9AC\uB97C \uBC1C\uC0AC\uD569\uB2C8\uB2E4.", 1, RunCombatEffectType.HomingProjectile),
                CreateOrUpdateItem("RunItem_ChestnutBurst", "\uBC24\uC1A1\uC774 \uD3ED\uBC1C", "\uC120\uD0DD \uC989\uC2DC \uC8FC\uBCC0 \uC801\uC5D0\uAC8C \uD53C\uD574\uB97C \uC90D\uB2C8\uB2E4.", 1, RunCombatEffectType.InstantBurst),
                CreateOrUpdateItem("RunItem_ToxicAcorn", "\uB3C5\uB3C4\uD1A0\uB9AC", "\uBA85\uC911\uD55C \uC801\uC5D0\uAC8C 3\uCD08 \uB3D9\uC548 \uB3C5 \uD53C\uD574\uB97C \uC90D\uB2C8\uB2E4.", 3, RunCombatEffectType.PoisonProjectile),
                CreateOrUpdateItem("RunItem_CheeseMagnet", "\uCE58\uC988 \uC790\uC11D", "\uAC00\uAE4C\uC6B4 \uCE58\uC988\uB97C \uB04C\uC5B4\uB2F9\uACA8 \uD68D\uB4DD\uD569\uB2C8\uB2E4. \uC911\uCCA9 \uC2DC \uBC94\uC704\uC640 \uC18D\uB3C4\uAC00 \uC99D\uAC00\uD569\uB2C8\uB2E4.", 3, RunCombatEffectType.CheeseMagnet),
            };
        }

        private static RunItemDefinition CreateOrUpdateItem(
            string assetName,
            string displayName,
            string description,
            int maximumStacks,
            RunCombatEffectType combatEffectType)
        {
            string assetPath = ItemFolderPath + "/" + assetName + ".asset";
            RunItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<RunItemDefinition>(assetPath);
            if (itemDefinition == null)
            {
                itemDefinition = ScriptableObject.CreateInstance<RunItemDefinition>();
                AssetDatabase.CreateAsset(itemDefinition, assetPath);
            }

            SerializedObject serializedItem = new(itemDefinition);
            serializedItem.FindProperty("_displayName").stringValue = displayName;
            serializedItem.FindProperty("_description").stringValue = description;
            serializedItem.FindProperty("_maximumStacks").intValue = maximumStacks;
            serializedItem.FindProperty("_temporaryDuration").floatValue = 0f;
            serializedItem.FindProperty("_combatEffectType").enumValueIndex = (int)combatEffectType;
            serializedItem.FindProperty("_modifiers").arraySize = 0;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static void ConfigureCombatEvolution(
            PlayerRunCombatEvolution combatEvolution,
            PrototypeEnemySpawner enemySpawner)
        {
            SerializedObject serializedEvolution = new(combatEvolution);
            serializedEvolution.FindProperty("_enemySpawner").objectReferenceValue = enemySpawner;
            serializedEvolution.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureAutoAttack(
            PlayerSceneAutoAttack autoAttack,
            PlayerRunCombatEvolution combatEvolution)
        {
            SerializedObject serializedAutoAttack = new(autoAttack);
            serializedAutoAttack.FindProperty("_combatEvolution").objectReferenceValue = combatEvolution;
            serializedAutoAttack.FindProperty("_projectilePoolCapacity").intValue = 20;
            serializedAutoAttack.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureChoiceController(
            RunItemChoiceController choiceController,
            PlayerRunCombatEvolution combatEvolution,
            IReadOnlyList<RunItemDefinition> combatItems)
        {
            SerializedObject serializedController = new(choiceController);
            serializedController.FindProperty("_combatEvolution").objectReferenceValue = combatEvolution;

            SerializedProperty availableItems = serializedController.FindProperty("_availableItems");
            List<RunItemDefinition> allItems = new();
            for (int index = 0; index < availableItems.arraySize; index++)
            {
                RunItemDefinition existingItem = availableItems.GetArrayElementAtIndex(index).objectReferenceValue as RunItemDefinition;
                if (existingItem != null && !allItems.Contains(existingItem))
                {
                    allItems.Add(existingItem);
                }
            }

            for (int index = 0; index < combatItems.Count; index++)
            {
                RunItemDefinition combatItem = combatItems[index];
                if (combatItem != null && !allItems.Contains(combatItem))
                {
                    allItems.Add(combatItem);
                }
            }

            availableItems.arraySize = allItems.Count;
            for (int index = 0; index < allItems.Count; index++)
            {
                availableItems.GetArrayElementAtIndex(index).objectReferenceValue = allItems[index];
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureEnemyTemplate(PrototypeEnemySpawner enemySpawner)
        {
            SerializedObject serializedSpawner = new(enemySpawner);
            PrototypeEnemyMover enemyTemplate = serializedSpawner.FindProperty("_enemyTemplate").objectReferenceValue as PrototypeEnemyMover;
            if (enemyTemplate == null)
            {
                return;
            }

            SerializedObject serializedEnemy = new(enemyTemplate);
            serializedEnemy.FindProperty("_maximumHealth").intValue = 3;
            serializedEnemy.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(enemyTemplate);
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
