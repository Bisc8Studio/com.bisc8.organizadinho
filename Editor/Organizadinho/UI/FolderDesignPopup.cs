using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.Storage;
using Organizadinho.Editor.Utilities;

namespace Organizadinho.Editor.UI
{

public class FolderDesignPopup : PopupWindowContent
{
    private readonly string _guid;
    private readonly string _path;
    private FolderDesignEntry _entry;
    private Vector2 _iconScroll;

    private const float PopupWidth = 280f;

    private static Texture2D[] _cachedIcons;
    private static double _iconCacheTime = -1.0;

    public FolderDesignPopup(string guid, string path)
    {
        _guid = guid;
        _path = path;
        _entry = FolderDesignStorage.GetOrCreate().GetOrCreateEntry(guid);
    }

    public override Vector2 GetWindowSize()
    {
        float ln = EditorGUIUtility.singleLineHeight + 2f;
        float h = 8f;
        h += ln + 2f;
        h += ln * 6f;
        h += 7f;
        h += ln;
        h += 90f;
        h += 7f;
        h += ln + 4f;
        h += 8f;
        return new Vector2(PopupWidth, h);
    }

    public override void OnGUI(Rect rect)
    {
        var storage = FolderDesignStorage.GetOrCreate();
        string folderName = System.IO.Path.GetFileName(_path);

        GUILayout.Space(4f);
        EditorGUILayout.LabelField("Folder  " + folderName, EditorStyles.boldLabel);
        DrawHRule();

        var currentHue = _entry.hue;
        var newHue = ColorHueSlider.DrawHueSlider(
            "Base Color",
            currentHue,
            "Folder preview",
            out var pastedColor);

        EditorGUI.BeginChangeCheck();
        bool newHasColor = EditorGUILayout.Toggle("Aplly Color", _entry.hasColor);
        if (pastedColor)
            newHasColor = true;

        EditorGUI.BeginDisabledGroup(!newHasColor);
        bool newPropagate = EditorGUILayout.Toggle(
            new GUIContent("Apply to sub-folders", "All child folders inherit this color style"),
            _entry.propagateChildren);
        EditorGUI.EndDisabledGroup();

        if (EditorGUI.EndChangeCheck() || pastedColor || !Mathf.Approximately(currentHue, newHue))
        {
            _entry.hasColor = newHasColor;
            _entry.propagateChildren = newPropagate;
            _entry.hue = ColorPaletteUtility.NormalizeHue(newHue);
            storage.NotifyChanged();
            editorWindow?.Repaint();
        }

        DrawHRule();
        DrawIconPicker(storage);
        DrawHRule();

        GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
        if (GUILayout.Button("Reset Folder Style"))
        {
            storage.entries.Remove(_entry);
            storage.NotifyChanged(true);
            _entry = storage.GetOrCreateEntry(_guid);
            editorWindow?.Repaint();
        }
        GUI.backgroundColor = Color.white;
    }

    public override void OnClose()
    {
        var storage = FolderDesignStorage.GetOrCreate();
        storage.PruneEntry(_guid);
        storage.NotifyChanged(true);
    }

    private void DrawIconPicker(FolderDesignStorage storage)
    {
        var palette = ColorPaletteUtility.BuildPalette(_entry.hue);
        HierarchyDesignPopup.EnsureIconFolderExists();
        var icons = GetIcons();

        EditorGUILayout.LabelField(
            "Badge Icon  (" + HierarchyDesignPopup.IconFolder + "/)",
            EditorStyles.centeredGreyMiniLabel);

        _iconScroll = EditorGUILayout.BeginScrollView(_iconScroll, GUILayout.Height(86f));

        if (icons.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "Pasta vazia.\nColoque PNG/JPG em:\n" + HierarchyDesignPopup.IconFolder,
                MessageType.Info);
        }
        else
        {
            const float iconSize = 34f;
            const float padding = 3f;
            int cols = Mathf.Max(1, Mathf.FloorToInt((PopupWidth - 12f) / (iconSize + padding)));
            int col = 0;
            EditorGUILayout.BeginHorizontal();

            bool noneSelected = string.IsNullOrEmpty(_entry.iconGuid);
            GUI.backgroundColor = noneSelected ? palette.SelectedColor : Color.white;
            if (GUILayout.Button("X", GUILayout.Width(iconSize), GUILayout.Height(iconSize)))
            {
                _entry.iconGuid = "";
                storage.NotifyChanged();
            }
            GUI.backgroundColor = Color.white;
            if (++col >= cols) { EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); col = 0; }

            foreach (var icon in icons)
            {
                string iconGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(icon));
                bool selected = _entry.iconGuid == iconGuid;
                GUI.backgroundColor = selected ? palette.SelectedColor : Color.white;
                if (GUILayout.Button(new GUIContent(icon, icon.name),
                        GUILayout.Width(iconSize), GUILayout.Height(iconSize)))
                {
                    _entry.iconGuid = selected ? "" : iconGuid;
                    storage.NotifyChanged();
                }
                GUI.backgroundColor = Color.white;
                if (++col >= cols) { EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); col = 0; }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private static Texture2D[] GetIcons()
    {
        if (_cachedIcons != null && EditorApplication.timeSinceStartup - _iconCacheTime < 3.0)
            return _cachedIcons;
        _iconCacheTime = EditorApplication.timeSinceStartup;

        string folder = HierarchyDesignPopup.IconFolder;
        if (!AssetDatabase.IsValidFolder(folder))
        {
            _cachedIcons = new Texture2D[0];
            return _cachedIcons;
        }

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        var list = new List<Texture2D>(guids.Length);
        foreach (var guid in guids)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
            if (tex != null)
                list.Add(tex);
        }

        _cachedIcons = list.ToArray();
        return _cachedIcons;
    }

    private static void DrawHRule()
    {
        GUILayout.Space(3f);
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
        EditorGUI.DrawRect(r, new Color(0f, 0f, 0f, EditorGUIUtility.isProSkin ? 0.4f : 0.18f));
        GUILayout.Space(3f);
    }
}
}
