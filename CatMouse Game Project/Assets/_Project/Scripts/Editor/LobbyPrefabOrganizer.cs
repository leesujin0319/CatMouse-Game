#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using CatMouse.Game.UI;
using UnityEditor;
using UnityEngine;

namespace CatMouse.Game.Editor
{
    public static class LobbyPrefabOrganizer
    {
        private const string LobbyPrefabFolderPath = "Assets/_Project/Prefabs/UI/Lobby";
        private const string LobbyPanelFolderPath = LobbyPrefabFolderPath + "/Modules/Panels";
        private const string LobbySlotFolderPath = LobbyPrefabFolderPath + "/Components/Slots";
        private const string UpgradePanelPrefabPath = LobbyPanelFolderPath + "/LobbyPanel_Upgrade.prefab";
        private const string EquipmentPanelPrefabPath = LobbyPanelFolderPath + "/LobbyPanel_Equipment.prefab";
        private const string UpgradeSlotPrefabPath = LobbySlotFolderPath + "/LobbyUpgradeSlot.prefab";
        private const string EquipmentSlotPrefabPath = LobbySlotFolderPath + "/LobbyEquipmentSlot.prefab";
        private const string StagePrefabPath = LobbyPrefabFolderPath + "/Modules/Stage/LobbyStage.prefab";
        private const string NavigationPrefabPath = LobbyPrefabFolderPath + "/Modules/Navigation/LobbyNavigation.prefab";
        private const string SettingsPanelPrefabPath = LobbyPanelFolderPath + "/LobbyPanel_Settings.prefab";
        private const string LegacyButtonPrefabFolderPath = LobbyPrefabFolderPath + "/Buttons";
        private const string LegacyPanelPrefabFolderPath = LobbyPrefabFolderPath + "/Panels";

        [MenuItem("CatMouse/Install/Organize Lobby Prefabs")]
        public static void Organize()
        {
            EnsureFolder(LobbyPrefabFolderPath + "/Components");
            EnsureFolder(LobbySlotFolderPath);

            ConvertRowsToNestedSlots(UpgradePanelPrefabPath, "UpgradeDrawer", "Upgrade_", UpgradeSlotPrefabPath);
            ConvertRowsToNestedSlots(EquipmentPanelPrefabPath, "EquipmentDrawer", "Equipment_", EquipmentSlotPrefabPath);
            CapturePrefabContent(StagePrefabPath);
            CapturePrefabContent(NavigationPrefabPath);
            CapturePrefabContent(SettingsPanelPrefabPath);
            CapturePrefabContent(UpgradeSlotPrefabPath);
            CapturePrefabContent(EquipmentSlotPrefabPath);
            SetPrefabActiveForEditing(StagePrefabPath);
            SetPrefabActiveForEditing(NavigationPrefabPath);
            SetPrefabActiveForEditing(UpgradePanelPrefabPath);
            SetPrefabActiveForEditing(EquipmentPanelPrefabPath);
            SetPrefabActiveForEditing(SettingsPanelPrefabPath);
            SetPrefabActiveForEditing(UpgradeSlotPrefabPath);
            SetPrefabActiveForEditing(EquipmentSlotPrefabPath);
            DeleteLegacyPrefabFolders();

            AssetDatabase.SaveAssets();
            Debug.Log("[LobbyPrefabOrganizer] 패널·슬롯 프리팹 구조를 정리했습니다.");
        }

        private static void ConvertRowsToNestedSlots(string panelPrefabPath, string drawerName, string rowPrefix, string slotPrefabPath)
        {
            GameObject panel = PrefabUtility.LoadPrefabContents(panelPrefabPath);
            try
            {
                LobbyPrefabLayout panelLayout = RequireComponent<LobbyPrefabLayout>(panel.transform, string.Empty);
                Transform drawer = RequireTransform(panel.transform, drawerName);
                List<Transform> rows = FindRows(drawer, rowPrefix);
                if (rows.Count == 0)
                {
                    throw new InvalidOperationException($"재사용할 슬롯을 찾지 못했습니다: {panelPrefabPath}");
                }

                GameObject slotPrefab = CreateSlotPrefabIfMissing(rows[0].gameObject, panelLayout, slotPrefabPath);
                for (int index = 0; index < rows.Count; index++)
                {
                    Transform row = rows[index];
                    if (PrefabUtility.GetCorrespondingObjectFromSource(row.gameObject) == slotPrefab)
                    {
                        continue;
                    }

                    ReplaceWithSlotPrefab(row, slotPrefab);
                }

                panelLayout.CaptureFromHierarchy();
                PrefabUtility.SaveAsPrefabAsset(panel, panelPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(panel);
            }
        }

        private static GameObject CreateSlotPrefabIfMissing(GameObject source, LobbyPrefabLayout panelLayout, string slotPrefabPath)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPrefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            GameObject slotTemplate = UnityEngine.Object.Instantiate(source);
            slotTemplate.name = System.IO.Path.GetFileNameWithoutExtension(slotPrefabPath);
            slotTemplate.transform.SetParent(null, false);

            LobbyPrefabLayout slotLayout = slotTemplate.GetComponent<LobbyPrefabLayout>();
            if (slotLayout == null)
            {
                slotLayout = slotTemplate.AddComponent<LobbyPrefabLayout>();
            }

            slotLayout.Initialize(panelLayout.GridUnit, panelLayout.ReferenceResolution);
            slotLayout.CaptureFromHierarchy();
            GameObject slotPrefab = PrefabUtility.SaveAsPrefabAsset(slotTemplate, slotPrefabPath);
            UnityEngine.Object.DestroyImmediate(slotTemplate);
            return slotPrefab;
        }

        private static void CapturePrefabContent(string prefabPath)
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                RequireComponent<LobbyPrefabLayout>(prefab.transform, string.Empty).CaptureFromHierarchy();
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        private static void SetPrefabActiveForEditing(string prefabPath)
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                prefab.SetActive(true);
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        private static void ReplaceWithSlotPrefab(Transform source, GameObject slotPrefab)
        {
            RectTransform sourceRect = source.GetComponent<RectTransform>();
            int siblingIndex = source.GetSiblingIndex();
            string sourceName = source.name;
            bool sourceActive = source.gameObject.activeSelf;

            GameObject replacement = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab);
            replacement.name = sourceName;
            replacement.transform.SetParent(source.parent, false);
            replacement.transform.SetSiblingIndex(siblingIndex);
            replacement.SetActive(sourceActive);

            RectTransform replacementRect = replacement.GetComponent<RectTransform>();
            replacementRect.anchorMin = sourceRect.anchorMin;
            replacementRect.anchorMax = sourceRect.anchorMax;
            replacementRect.pivot = sourceRect.pivot;
            replacementRect.anchoredPosition = sourceRect.anchoredPosition;
            replacementRect.sizeDelta = sourceRect.sizeDelta;

            UnityEngine.Object.DestroyImmediate(source.gameObject);
        }

        private static List<Transform> FindRows(Transform drawer, string rowPrefix)
        {
            List<Transform> rows = new();
            for (int index = 0; index < drawer.childCount; index++)
            {
                Transform child = drawer.GetChild(index);
                if (child.name.StartsWith(rowPrefix, StringComparison.Ordinal))
                {
                    rows.Add(child);
                }
            }

            return rows;
        }

        private static void DeleteLegacyPrefabFolders()
        {
            DeleteFolderIfPresent(LegacyButtonPrefabFolderPath);
            DeleteFolderIfPresent(LegacyPanelPrefabFolderPath);
        }

        private static void DeleteFolderIfPresent(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.DeleteAsset(folderPath);
            }
        }

        private static Transform RequireTransform(Transform root, string relativePath)
        {
            Transform target = string.IsNullOrEmpty(relativePath) ? root : root.Find(relativePath);
            if (target == null)
            {
                throw new InvalidOperationException($"프리팹 구성 경로를 찾지 못했습니다: {root.name}/{relativePath}");
            }

            return target;
        }

        private static T RequireComponent<T>(Transform root, string relativePath) where T : Component
        {
            T component = RequireTransform(root, relativePath).GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"필수 컴포넌트를 찾지 못했습니다: {root.name}/{relativePath} ({typeof(T).Name})");
            }

            return component;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            int separatorIndex = folderPath.LastIndexOf('/');
            AssetDatabase.CreateFolder(folderPath[..separatorIndex], folderPath[(separatorIndex + 1)..]);
        }
    }
}
#endif
