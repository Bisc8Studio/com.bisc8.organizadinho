using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Organizadinho.Editor.Storage;
using Organizadinho.Editor.UI;
using Organizadinho.Editor.Utilities;

namespace Organizadinho.Editor.FolderDesign
{

[InitializeOnLoad]
public static class FolderDesignDrawer
{
    private static readonly Dictionary<string, bool> EmptyFolderCache = new Dictionary<string, bool>(StringComparer.Ordinal);

    static FolderDesignDrawer()
    {
        EditorApplication.projectWindowItemOnGUI -= OnProjectGUI;
        EditorApplication.projectWindowItemOnGUI += OnProjectGUI;
        EditorApplication.projectChanged -= ClearFolderStateCache;
        EditorApplication.projectChanged += ClearFolderStateCache;
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

        DrawFolderIcon(path, iconRect, style, IsSelected(guid), selectionRect.Contains(Event.current.mousePosition));
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

    private static void DrawFolderIcon(
        string path,
        Rect iconRect,
        FolderDesignResolvedStyle style,
        bool isSelected,
        bool isHovered)
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

        if (IsEmptyFolder(path))
            DrawEmptyFolderInterior(iconRect, isSelected, isHovered);
    }

    private static void DrawEmptyFolderInterior(Rect iconRect, bool isSelected, bool isHovered)
    {
        var innerRect = GetEmptyFolderInteriorRect(iconRect);
        if (innerRect.width < 2f || innerRect.height < 2f)
            return;

        EditorGUI.DrawRect(innerRect, GetProjectWindowSurfaceColor(isSelected, isHovered));
    }

    private static Rect GetEmptyFolderInteriorRect(Rect iconRect)
    {
        return new Rect(
            iconRect.x + iconRect.width * 0.18f,
            iconRect.y + iconRect.height * 0.46f,
            iconRect.width * 0.64f,
            iconRect.height * 0.29f);
    }

    private static Color GetProjectWindowSurfaceColor(bool isSelected, bool isHovered)
    {
        if (isSelected)
        {
            return EditorGUIUtility.isProSkin
                ? new Color(0.172f, 0.365f, 0.529f, 1f)
                : new Color(0.243f, 0.490f, 0.902f, 1f);
        }

        if (isHovered)
        {
            return EditorGUIUtility.isProSkin
                ? new Color(0.270f, 0.270f, 0.270f, 1f)
                : new Color(0.800f, 0.800f, 0.800f, 1f);
        }

        return EditorGUIUtility.isProSkin
            ? new Color(0.219f, 0.219f, 0.219f, 1f)
            : new Color(0.760f, 0.760f, 0.760f, 1f);
    }

    private static bool IsEmptyFolder(string path)
    {
        path = NormalizeAssetPath(path);
        if (string.IsNullOrEmpty(path))
            return false;

        if (EmptyFolderCache.TryGetValue(path, out var isEmpty))
            return isEmpty;

        isEmpty = !FolderHasDirectContent(path);
        EmptyFolderCache[path] = isEmpty;
        return isEmpty;
    }

    private static bool FolderHasDirectContent(string path)
    {
        if (AssetDatabase.GetSubFolders(path).Length > 0)
            return true;

        var guids = AssetDatabase.FindAssets(string.Empty, new[] { path });
        for (var index = 0; index < guids.Length; index++)
        {
            var assetPath = NormalizeAssetPath(AssetDatabase.GUIDToAssetPath(guids[index]));
            if (string.IsNullOrEmpty(assetPath) || assetPath == path)
                continue;

            if (NormalizeAssetPath(Path.GetDirectoryName(assetPath)) == path)
                return true;
        }

        return false;
    }

    private static string NormalizeAssetPath(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.Replace('\\', '/').TrimEnd('/');
    }

    private static void ClearFolderStateCache()
    {
        EmptyFolderCache.Clear();
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
        var popup = new FolderDesignPopup(guid, path);
        var popupSize = popup.GetWindowSize();
        Rect showRect;
        if (screenClick.HasValue)
        {
            var clickPosition = screenClick.Value;
            var window = EditorWindow.mouseOverWindow ?? EditorWindow.focusedWindow;
            if (window != null)
                clickPosition -= window.position.position;

            showRect = new Rect(
                clickPosition.x - popupSize.x * 0.5f,
                clickPosition.y - popupSize.y * 0.5f,
                popupSize.x,
                popupSize.y);
        }
        else
        {
            var window = EditorWindow.focusedWindow ?? EditorWindow.mouseOverWindow;
            showRect = window != null
                ? new Rect(
                    window.position.x + window.position.width * 0.5f - popupSize.x * 0.5f,
                    window.position.y + window.position.height * 0.5f - popupSize.y * 0.5f,
                    1f,
                    1f)
                : new Rect(500f, 300f, 1f, 1f);
        }

        PopupWindow.Show(showRect, popup);
    }
}
}
