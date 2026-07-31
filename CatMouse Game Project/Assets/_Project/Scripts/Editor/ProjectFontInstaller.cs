#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CatMouse.Editor
{
    public static class ProjectFontInstaller
    {
        private const string FontAssetPath = "Assets/_Project/Font/MemomentKkukkukk.ttf";

        private static readonly string[] ScenePaths =
        {
            "Assets/_Project/Scenes/Development/CardScene.unity",
            "Assets/_Project/Scenes/Development/Dev_PantryBackground.unity",
            "Assets/_Project/Scenes/Development/PlayerScene.unity",
            "Assets/_Project/Scenes/Gameplay/GameScene.unity",
        };

        [MenuItem("CatMouse/Development/Apply Memoment Font to All UI Text")]
        private static void ApplyToAllUiText()
        {
            Font projectFont = AssetDatabase.LoadAssetAtPath<Font>(FontAssetPath);
            if (projectFont == null)
            {
                Debug.LogError($"[ProjectFontInstaller] Font not found: {FontAssetPath}");
                return;
            }

            int changedTextCount = 0;
            foreach (string scenePath in ScenePaths)
            {
                Scene scene = SceneManager.GetSceneByPath(scenePath);
                bool closeAfterApply = !scene.isLoaded;
                if (closeAfterApply)
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }

                bool sceneChanged = false;
                Text[] texts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (Text text in texts)
                {
                    if (text.gameObject.scene != scene || text.font == projectFont)
                    {
                        continue;
                    }

                    text.font = projectFont;
                    EditorUtility.SetDirty(text);
                    changedTextCount++;
                    sceneChanged = true;
                }

                if (sceneChanged)
                {
                    EditorSceneManager.SaveScene(scene);
                }

                if (closeAfterApply)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[ProjectFontInstaller] Applied {projectFont.name} to {changedTextCount} UI Text components.");
        }
    }
}
#endif
