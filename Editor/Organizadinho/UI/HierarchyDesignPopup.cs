using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.Drawing;
using Organizadinho.Editor.Utilities;
using Organizadinho.Runtime;

namespace Organizadinho.Editor.UI
{

public class HierarchyDesignPopup : PopupWindowContent
{
    private const string PackageRoot = "Packages/com.bisc8.organizadinho";
    private const string EditorResourcesRoot = PackageRoot + "/Editor Default Resources";
    private const string OrganizadinhoResourcesRoot = EditorResourcesRoot + "/Organizadinho";
    private const float PopupWidth = 300f;

    public const string IconFolder = OrganizadinhoResourcesRoot + "/Icons";
    public const string FontFolder = OrganizadinhoResourcesRoot + "/Fonts";

    private readonly GameObject _go;
    private HierarchyDesign _hd;
    private Vector2 _iconScroll = Vector2.zero;
    private Vector2 _fontScroll = Vector2.zero;

    private static Texture2D[] _cachedIcons;
    private static double _iconCacheTime = -1.0;
    private static Font[] _cachedFonts;
    private static double _fontCacheTime = -1.0;

    public HierarchyDesignPopup(GameObject go)
    {
        _go = go;
        _hd = go.GetComponent<HierarchyDesign>();
    }

    public override Vector2 GetWindowSize()
    {
        float ln = EditorGUIUtility.singleLineHeight + 2f;
        float h = 8f;
        h += ln + 2f;
        h += ln + 4f;

        if (_hd != null && _hd.isOrganizer)
        {
            h += ln * 5f;
            h += ln;
            h += 60f;
            h += ln;
            h += 12f;
            h += ln;
            h += 90f;
            h += 12f;
            h += ln;
            h += 90f;
            h += 12f;
        }

        h += 8f;
        return new Vector2(PopupWidth, h);
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.Space(4f);
        EditorGUILayout.LabelField("Organizer  " + _go.name, EditorStyles.boldLabel);
        DrawHorizontalRule();

        EditorGUI.BeginChangeCheck();
        bool isOrg = EditorGUILayout.Toggle("Is Organizer", _hd != null && _hd.isOrganizer);
        if (EditorGUI.EndChangeCheck())
        {
            if (isOrg)
            {
                if (_hd == null)
                {
                    Undo.AddComponent<HierarchyDesign>(_go);
                    _hd = _go.GetComponent<HierarchyDesign>();
                    _hd.EnsureColorData();
                    _hd.isOrganizer = true;
                }
                else
                {
                    Undo.RecordObject(_hd, "Enable Organizer");
                    _hd.EnsureColorData();
                    _hd.isOrganizer = true;
                }

                _hd.SyncInspectorVisibility();
                EditorUtility.SetDirty(_hd);
            }
            else if (_hd != null)
            {
                Undo.DestroyObjectImmediate(_hd);
                _hd = null;
                HierarchyDesignDrawer.ClearCache();
            }

            EditorApplication.RepaintHierarchyWindow();
        }

        if (_hd == null || !_hd.isOrganizer)
            return;

        var currentHue = _hd != null ? _hd.colorHue : ColorPaletteUtility.DefaultHue;
        var newHue = ColorHueSlider.DrawHueSlider(
            "Base Color",
            currentHue,
            "Organizer preview");
        if (_hd != null && !Mathf.Approximately(currentHue, newHue))
        {
            Undo.RecordObject(_hd, "Edit Organizer Color");
            _hd.colorHue = ColorPaletteUtility.NormalizeHue(newHue);
            EditorUtility.SetDirty(_hd);
            HierarchyDesignDrawer.ClearCache();
            EditorApplication.RepaintHierarchyWindow();
            editorWindow?.Repaint();
        }

        if (_hd != null && _hd.isOrganizer)
        {
            EditorGUI.BeginChangeCheck();
            bool newProp = EditorGUILayout.Toggle(
                new GUIContent("Color in children", "Aplica a variacao de cor em todos os filhos"),
                _hd.propagateToChildren);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_hd, "Edit Children Tint");
                _hd.propagateToChildren = newProp;
                EditorUtility.SetDirty(_hd);
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        GUILayout.Space(4f);

        _hd.EnsureColorData();
        var palette = ColorPaletteUtility.BuildPalette(_hd.colorHue);
        Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(54f));
        GUI.DrawTexture(
            previewRect,
            HierarchyDesignDrawer.GetPreviewTexture(palette.BaseColor),
            ScaleMode.StretchToFill);

        GUIStyle previewStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = _hd.fontSize,
            fontStyle = FontStyle.Normal,
            normal = { textColor = palette.ForegroundColor }
        };
        if (_hd.customFont != null)
            previewStyle.font = _hd.customFont;

        GUI.Label(
            new Rect(previewRect.x + 6f, previewRect.y, previewRect.width - 10f, previewRect.height),
            _go.name,
            previewStyle);

        DrawHorizontalRule();

        EditorGUI.BeginChangeCheck();
        int newSize = EditorGUILayout.IntSlider("Font Size", _hd.fontSize, 8, 20);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_hd, "Edit Hierarchy Design");
            _hd.fontSize = newSize;
            EditorUtility.SetDirty(_hd);
            HierarchyDesignDrawer.ClearCache();
            EditorApplication.RepaintHierarchyWindow();
            editorWindow?.Repaint();
        }

        DrawHorizontalRule();
        DrawFontPicker();
        DrawHorizontalRule();
        DrawIconPicker();
    }

    private void DrawFontPicker()
    {
        var palette = ColorPaletteUtility.BuildPalette(_hd.colorHue);
        EnsureFolderExists(OrganizadinhoResourcesRoot, "Fonts");
        var fonts = GetFolderFonts();
        EditorGUILayout.LabelField("Font  (" + FontFolder + "/)", EditorStyles.centeredGreyMiniLabel);

        _fontScroll = EditorGUILayout.BeginScrollView(_fontScroll, GUILayout.Height(86f));
        if (fonts.Length == 0)
        {
            EditorGUILayout.HelpBox("Pasta vazia.\nColoque .ttf/.otf em:\n" + FontFolder, MessageType.Info);
        }
        else
        {
            bool noneSelected = _hd.customFont == null;
            GUI.backgroundColor = noneSelected ? palette.SelectedColor : Color.white;
            if (GUILayout.Button("Default", GUILayout.Height(22f)))
            {
                Undo.RecordObject(_hd, "Clear Custom Font");
                _hd.customFont = null;
                EditorUtility.SetDirty(_hd);
                EditorApplication.RepaintHierarchyWindow();
            }
            GUI.backgroundColor = Color.white;

            foreach (var font in fonts)
            {
                bool selected = _hd.customFont == font;
                GUI.backgroundColor = selected ? palette.SelectedColor : Color.white;
                var style = new GUIStyle(GUI.skin.button) { font = font, fontSize = 12 };
                if (GUILayout.Button(font.name, style, GUILayout.Height(22f)))
                {
                    Undo.RecordObject(_hd, "Set Custom Font");
                    _hd.customFont = selected ? null : font;
                    EditorUtility.SetDirty(_hd);
                    EditorApplication.RepaintHierarchyWindow();
                }
                GUI.backgroundColor = Color.white;
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private static Font[] GetFolderFonts()
    {
        if (_cachedFonts != null && EditorApplication.timeSinceStartup - _fontCacheTime < 3.0)
            return _cachedFonts;

        _fontCacheTime = EditorApplication.timeSinceStartup;
        if (!AssetDatabase.IsValidFolder(FontFolder))
        {
            _cachedFonts = new Font[0];
            return _cachedFonts;
        }

        var guids = AssetDatabase.FindAssets("t:Font", new[] { FontFolder });
        var list = new List<Font>(guids.Length);
        foreach (var guid in guids)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(guid));
            if (font != null)
                list.Add(font);
        }

        _cachedFonts = list.ToArray();
        return _cachedFonts;
    }

    private void DrawIconPicker()
    {
        var palette = ColorPaletteUtility.BuildPalette(_hd.colorHue);
        EnsureIconFolderExists();
        var icons = GetFolderIcons();
        EditorGUILayout.LabelField("Custom Icon  (" + IconFolder + "/)", EditorStyles.centeredGreyMiniLabel);

        _iconScroll = EditorGUILayout.BeginScrollView(_iconScroll, GUILayout.Height(86f));
        if (icons.Length == 0)
        {
            EditorGUILayout.HelpBox("Pasta vazia.\nColoque PNG/JPG em:\n" + IconFolder, MessageType.Info);
        }
        else
        {
            const float iconSize = 34f;
            const float padding = 3f;
            int cols = Mathf.Max(1, Mathf.FloorToInt((PopupWidth - 12f) / (iconSize + padding)));
            int col = 0;
            EditorGUILayout.BeginHorizontal();

            bool noneSelected = _hd.customIcon == null;
            GUI.backgroundColor = noneSelected ? palette.SelectedColor : Color.white;
            if (GUILayout.Button("X", GUILayout.Width(iconSize), GUILayout.Height(iconSize)))
            {
                Undo.RecordObject(_hd, "Clear Custom Icon");
                _hd.customIcon = null;
                EditorUtility.SetDirty(_hd);
                EditorApplication.RepaintHierarchyWindow();
            }
            GUI.backgroundColor = Color.white;
            if (++col >= cols) { EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); col = 0; }

            foreach (var icon in icons)
            {
                bool selected = _hd.customIcon == icon;
                GUI.backgroundColor = selected ? palette.SelectedColor : Color.white;
                if (GUILayout.Button(new GUIContent(icon, icon.name), GUILayout.Width(iconSize), GUILayout.Height(iconSize)))
                {
                    Undo.RecordObject(_hd, "Set Custom Icon");
                    _hd.customIcon = selected ? null : icon;
                    EditorUtility.SetDirty(_hd);
                    EditorApplication.RepaintHierarchyWindow();
                }
                GUI.backgroundColor = Color.white;
                if (++col >= cols) { EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); col = 0; }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private static Texture2D[] GetFolderIcons()
    {
        if (_cachedIcons != null && EditorApplication.timeSinceStartup - _iconCacheTime < 3.0)
            return _cachedIcons;

        _iconCacheTime = EditorApplication.timeSinceStartup;
        if (!AssetDatabase.IsValidFolder(IconFolder))
        {
            _cachedIcons = new Texture2D[0];
            return _cachedIcons;
        }

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { IconFolder });
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

    public static void EnsureIconFolderExists() => EnsureFolderExists(OrganizadinhoResourcesRoot, "Icons");
    public static void EnsureFontFolderExists() => EnsureFolderExists(OrganizadinhoResourcesRoot, "Fonts");
    public static void RefreshIconCache() => _cachedIcons = null;
    public static void RefreshFontCache() => _cachedFonts = null;

    private static void EnsureFolderExists(string parent, string child)
    {
        EnsureFolderChainExists(EditorResourcesRoot);
        EnsureFolderChainExists(OrganizadinhoResourcesRoot);

        string full = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(full))
        {
            AssetDatabase.CreateFolder(parent, child);
            AssetDatabase.Refresh();
        }
    }

    private static void EnsureFolderChainExists(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parts = path.Split('/');
        var current = parts[0];

        for (var index = 1; index < parts.Length; index++)
        {
            var next = current + "/" + parts[index];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[index]);
            }

            current = next;
        }
    }

    private static void DrawHorizontalRule()
    {
        GUILayout.Space(3f);
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
        EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, EditorGUIUtility.isProSkin ? 0.4f : 0.18f));
        GUILayout.Space(3f);
    }
}
}
