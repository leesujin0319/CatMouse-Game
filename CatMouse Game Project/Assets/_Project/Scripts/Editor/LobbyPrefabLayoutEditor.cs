#if UNITY_EDITOR
using CatMouse.Game.UI;
using UnityEditor;
using UnityEngine;

namespace CatMouse.Game.Editor
{
    [CustomEditor(typeof(LobbyPrefabLayout))]
    public sealed class LobbyPrefabLayoutEditor : UnityEditor.Editor
    {
        private const string PreviewPreferencePrefix = "CatMouse.LobbyPrefabLayout.Preview.";

        public override void OnInspectorGUI()
        {
            LobbyPrefabLayout layout = (LobbyPrefabLayout)target;
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "이 프리팹이 직접 관리하는 위치와 버튼입니다. 중첩된 다른 프리팹의 정보는 표시하지 않습니다.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_gridUnit"), new GUIContent("배치 단위"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_referenceResolution"), new GUIContent("기준 해상도"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_elements"), new GUIContent("위치 목록"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_buttons"), new GUIContent("이 프리팹의 버튼"), true);
            bool layoutChanged = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();

            if (layoutChanged)
            {
                layout.SnapLayoutValues();
                layout.ApplyLayout();
                EditorUtility.SetDirty(layout);
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("작업", EditorStyles.boldLabel);
            if (GUILayout.Button("현재 위치 저장"))
            {
                Undo.RecordObject(layout, "Capture Lobby Prefab Layout");
                layout.CaptureFromHierarchy();
                EditorUtility.SetDirty(layout);
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("저장한 위치로 되돌리기"))
            {
                Undo.RecordObject(layout, "Apply Lobby Prefab Layout");
                layout.ApplyLayout();
                EditorUtility.SetDirty(layout);
                SceneView.RepaintAll();
            }

            string previewKey = PreviewPreferencePrefix + GlobalObjectId.GetGlobalObjectIdSlow(layout);
            bool previewEnabled = SessionState.GetBool(previewKey, true);
            bool nextPreviewEnabled = EditorGUILayout.ToggleLeft("위치 가이드 보기", previewEnabled);
            if (nextPreviewEnabled != previewEnabled)
            {
                SessionState.SetBool(previewKey, nextPreviewEnabled);
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            LobbyPrefabLayout layout = (LobbyPrefabLayout)target;
            string previewKey = PreviewPreferencePrefix + GlobalObjectId.GetGlobalObjectIdSlow(layout);
            if (!SessionState.GetBool(previewKey, true))
            {
                return;
            }

            for (int index = 0; index < layout.Elements.Count; index++)
            {
                LobbyLayoutElement element = layout.Elements[index];
                RectTransform target = element.Target;
                if (target == null)
                {
                    continue;
                }

                DrawGuide(target, element.Path);
            }
        }

        private static void DrawGuide(RectTransform target, string label)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);

            Color fill = new(0.14f, 0.74f, 0.9f, 0.06f);
            Color outline = new(0.14f, 0.74f, 0.9f, 0.9f);
            Handles.DrawSolidRectangleWithOutline(corners, fill, outline);
            Handles.Label((corners[0] + corners[2]) * 0.5f, label, EditorStyles.miniBoldLabel);
        }
    }
}
#endif
