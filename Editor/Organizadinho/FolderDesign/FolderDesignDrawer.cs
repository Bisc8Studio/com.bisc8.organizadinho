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
    private const float FolderTintHorizontalPadding = 0.75f;
    private const float FolderTintTopPadding = 0.5f;
    private const float FolderTintBottomPadding = 1f;

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

        var renderData = FolderDesignRenderCache.Get(guid, path);
        if (!renderData.Style.HasVisualOverride)
            return;

        var isIconView = selectionRect.height > 20f;
        var iconRect = GetFolderIconRect(selectionRect, isIconView);

        DrawFolderIcon(renderData, iconRect, IsSelected(guid), selectionRect.Contains(Event.current.mousePosition));
        DrawBadgeIcon(renderData.Style, iconRect, isIconView);
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
        FolderDesignRenderData renderData,
        Rect iconRect,
        bool isSelected,
        bool isHovered)
    {
        var style = renderData.Style;
        if (!style.HasResolvedColor)
            return;

        if (renderData.FolderTexture == null)
            return;

        var tintRect = GetFolderTintRect(iconRect);
        var previousColor = GUI.color;
        GUI.color = FolderDesignStyleResolver.GetProjectItemIconTint(style, isSelected);
        GUI.DrawTexture(tintRect, renderData.FolderTexture, ScaleMode.ScaleAndCrop, true);
        DrawFolderTintGradient(tintRect, renderData.FolderTexture);
        GUI.color = previousColor;

        if (renderData.IsEmptyFolder)
            DrawEmptyFolderInterior(tintRect, isSelected, isHovered);
    }

    private static Rect GetFolderTintRect(Rect iconRect)
    {
        var tintRect = iconRect;
        tintRect.xMin -= FolderTintHorizontalPadding;
        tintRect.xMax += FolderTintHorizontalPadding;
        tintRect.yMin -= FolderTintTopPadding;
        tintRect.yMax += FolderTintBottomPadding;
        return tintRect;
    }

    private static void DrawFolderTintGradient(Rect tintRect, Texture folderTexture)
    {
        DrawFolderTextureOverlay(
            tintRect,
            folderTexture,
            new Color(1f, 1f, 1f, EditorGUIUtility.isProSkin ? 0.035f : 0.025f));

        DrawFolderTextureOverlay(
            new Rect(tintRect.x, tintRect.y + tintRect.height * 0.58f, tintRect.width, tintRect.height * 0.42f),
            folderTexture,
            new Color(0f, 0f, 0f, EditorGUIUtility.isProSkin ? 0.028f : 0.02f));
    }

    private static void DrawFolderTextureOverlay(Rect drawRect, Texture texture, Color color)
    {
        if (drawRect.width <= 0f || drawRect.height <= 0f || color.a <= 0f)
            return;

        var previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(drawRect, texture, ScaleMode.ScaleAndCrop, true);
        GUI.color = previousColor;
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
