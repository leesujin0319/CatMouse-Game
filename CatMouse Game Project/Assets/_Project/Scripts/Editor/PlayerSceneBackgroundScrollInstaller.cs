#if UNITY_EDITOR
using CatMouse.Game.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatMouse.Game.Editor
{
    public static class PlayerSceneBackgroundScrollInstaller
    {
        private const string ScenePath = "Assets/_Project/Scenes/Development/PlayerScene.unity";

        [MenuItem("CatMouse/Development/Install PlayerScene Background Scroll")]
        public static void Install()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            EndlessPantryBackground[] backgrounds = Object.FindObjectsByType<EndlessPantryBackground>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int index = 0; index < backgrounds.Length; index++)
            {
                EndlessPantryBackground background = backgrounds[index];
                SerializedObject serializedBackground = new(background);
                serializedBackground.FindProperty("_scrollFactor").floatValue = GetScrollFactor(background.name);
                serializedBackground.FindProperty("_isCameraRelative").boolValue = true;
                serializedBackground.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(background);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        private static float GetScrollFactor(string layerName)
        {
            if (layerName.Contains("Far"))
            {
                return 0.25f;
            }

            if (layerName.Contains("Foreground"))
            {
                return 1f;
            }

            return 0.65f;
        }
    }
}
#endif
