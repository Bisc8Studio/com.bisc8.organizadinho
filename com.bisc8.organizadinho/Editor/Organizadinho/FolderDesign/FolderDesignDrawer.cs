using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.Storage;
using Organizadinho.Editor.UI;
using Organizadinho.Editor.Utilities;

namespace Organizadinho.Editor.FolderDesign
{

[InitializeOnLoad]
public static class FolderDesignDrawer
{
    static FolderDesignDrawer()
    {
        EditorApplication.projectWindowItemOnGUI -= OnProjectGUI;
        EditorApplication.projectWindowItemOnGUI += OnProjectGUI;
    }

    private static void OnProjectGUI(string guid, Rect selectionRect)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (!AssetDatabase.IsValidFolder(path))
            return;

        if (Event.current.type == EventType.MouseDown &&
            Event.current.alt &&
            selectionRect.Contains(Event.current.mousePosition))
        {
            var window = EditorWindow.mouseOverWindow ?? EditorWindow.focusedWindow;
            var screenPosition = (window != null ? window.position.position : Vector2.zero) + Event.current.mousePosition;
            Event.current.Use();
            OpenPopupForFolder(guid, path, screenPosition);
            return;
        }

        if (Event.current.type != EventType.Repaint)
            return;

        var style = FolderDesignStyleResolver.Resolve(guid, path);
        if (!style.HasVisualOverride)
            return;

        var isIconView = selectionRect.height > 20f;
        var iconRect = GetFolderIconRect(selectionRect, isIconView);

        DrawFolderIcon(path, iconRect, style, IsSelected(guid));
        DrawBadgeIcon(style, iconRect, isIconView);
    }

    [MenuItem("Assets/Hierarchy Design/Customize Folder...", true)]
    private static bool ValidateCustomize()
    {
        if (Selection.activeObject == null)
            return false;

        return AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
    }

    [MenuItem("Assets/Hierarchy Design/Customize Folder...")]
    private static void OpenCustomize()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        OpenPopupForFolder(AssetDatabase.AssetPathToGUID(path), path, null);
    }

    [MenuItem("Assets/Hierarchy Design/Reset Folder Style", true)]
    private static bool ValidateReset()
    {
        if (Selection.activeObject == null)
            return false;

        return AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
    }

    [MenuItem("Assets/Hierarchy Design/Reset Folder Style")]
    private static void ResetFolder()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        var guid = AssetDatabase.AssetPathToGUID(path);
        var storage = FolderDesignStorage.GetOrCreate();
        var entry = storage.GetEntry(guid);
        if (entry == null)
            return;

        storage.entries.Remove(entry);
        storage.NotifyChanged(true);
    }

    private static void DrawFolderIcon(string path, Rect iconRect, FolderDesignResolvedStyle style, bool isSelected)
    {
        if (!style.HasResolvedColor)
            return;

        var folderTexture = AssetDatabase.GetCachedIcon(path) ?? EditorGUIUtility.FindTexture("Folder Icon");
        if (folderTexture == null)
            return;

        var previousColor = GUI.color;
        GUI.color = FolderDesignStyleResolver.GetProjectItemIconTint(style, isSelected);
        GUI.DrawTexture(iconRect, folderTexture, ScaleMode.ScaleAndCrop, true);
        GUI.color = previousColor;
    }

    private static void DrawBadgeIcon(FolderDesignResolvedStyle style, Rect iconRect, bool isIconView)
    {
        if (!style.HasCustomIcon)
            return;

        var badgeIcon = FolderDesignStyleResolver.LoadBadgeIcon(style.IconGuid);
        if (badgeIcon == null)
            return;

        var badgeSize = Mathf.Max(8f, iconRect.width * (isIconView ? 0.38f : 0.55f));
        var badgeRect = new Rect(iconRect.xMax - badgeSize, iconRect.yMax - badgeSize, badgeSize, badgeSize);
        GUI.DrawTexture(badgeRect, badgeIcon, ScaleMode.ScaleToFit, true);
    }

    private static Rect GetFolderIconRect(Rect selectionRect, bool isIconView)
    {
        if (isIconView)
        {
            var size = selectionRect.width;
            return new Rect(selectionRect.x, selectionRect.y, size, size);
        }

        var sizeList = selectionRect.height;
        return new Rect(selectionRect.x, selectionRect.y, sizeList, sizeList);
    }

    private static bool IsSelected(string guid)
    {
        var selectedGuids = Selection.assetGUIDs;
        for (var index = 0; index < selectedGuids.Length; index++)
        {
            if (selectedGuids[index] == guid)
                return true;
        }

        return false;
    }

    private static void OpenPopupForFolder(string guid, string path, Vector2? screenClick)
    {
        Rect showRect;
        if (screenClick.HasValue)
        {
            const float popupWidth = 200f;
            const float popupHeight = 150f;
            var clickPosition = screenClick.Value;
            var window = EditorWindow.mouseOverWindow ?? EditorWindow.focusedWindow;
            if (window != null)
                clickPosition -= window.position.position;

            showRect = new Rect(
                clickPosition.x - popupWidth * 0.5f,
                clickPosition.y - popupHeight * 0.5f,
                popupWidth,
                popupHeight);
        }
        else
        {
            var window = EditorWindow.focusedWindow ?? EditorWindow.mouseOverWindow;
            showRect = window != null
                ? new Rect(
                    window.position.x + window.position.width * 0.5f - 140f,
                    window.position.y + window.position.height * 0.5f - 130f,
                    1f,
                    1f)
                : new Rect(500f, 300f, 1f, 1f);
        }

        PopupWindow.Show(showRect, new FolderDesignPopup(guid, path));
    }
}
}
