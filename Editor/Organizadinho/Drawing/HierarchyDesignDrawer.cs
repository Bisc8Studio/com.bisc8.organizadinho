using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.UI;
using Organizadinho.Editor.Utilities;
using Organizadinho.Runtime;

namespace Organizadinho.Editor.Drawing
{

[InitializeOnLoad]
public static class HierarchyDesignDrawer
{
    private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
    private static readonly HashSet<int> _prevVisible = new HashSet<int>();
    private static readonly HashSet<int> _currVisible = new HashSet<int>();
    private static float _lastRectY = float.MaxValue;

    static HierarchyDesignDrawer()
    {
        EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void TrackVisible(int instanceID, float rectY)
    {
        if (rectY < _lastRectY)
        {
            _prevVisible.Clear();
            foreach (int id in _currVisible)
                _prevVisible.Add(id);

            _currVisible.Clear();
        }

        _lastRectY = rectY;
        _currVisible.Add(instanceID);
    }

    private static bool IsItemExpanded(GameObject go)
    {
        if (go.transform.childCount == 0)
            return false;

        foreach (Transform child in go.transform)
        {
            if (_prevVisible.Contains(child.gameObject.GetInstanceID()))
                return true;
        }

        return false;
    }

    private static HierarchyDesign FindPropagatingAncestor(GameObject go)
    {
        for (Transform t = go.transform.parent; t != null; t = t.parent)
        {
            var hd = t.GetComponent<HierarchyDesign>();
            if (hd != null && hd.isOrganizer && hd.propagateToChildren)
                return hd;
        }

        return null;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        TrackVisible(instanceID, selectionRect.y);

#pragma warning disable CS0618
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#pragma warning restore CS0618
        if (go == null)
            return;

        HierarchyDesign hd = go.GetComponent<HierarchyDesign>();

        if (hd != null && hd.isOrganizer)
        {
            hd.EnsureColorData();
            var palette = ColorPaletteUtility.BuildPalette(hd.colorMode, hd.colorHue);
            float bgStartX = selectionRect.x - 28f;
            Rect bgRect = new Rect(bgStartX, selectionRect.y, Screen.width - bgStartX, selectionRect.height);
            Texture2D backgroundTexture = GetOrCreateGradientTexture(palette.BaseColor, HierarchyDesign.FadeMode.LeftToRight);

            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(
                    bgRect,
                    backgroundTexture,
                    ScaleMode.StretchToFill);
            }

            if (go.transform.childCount > 0)
            {
                bool isExpanded = IsItemExpanded(go);
                Rect toggleRect = new Rect(selectionRect.x - 14f, selectionRect.y + 1f, 13f, 13f);

                if (Event.current.type == EventType.Repaint)
                {
                    bool hover = toggleRect.Contains(Event.current.mousePosition);
                    GUI.color = palette.ForegroundColor;
                    EditorStyles.foldout.Draw(toggleRect, GUIContent.none, hover, false, isExpanded, false);
                    GUI.color = Color.white;
                }

                if (Event.current.type == EventType.MouseDown &&
                    toggleRect.Contains(Event.current.mousePosition))
                {
                    HierarchyExpansion.SetExpanded(go.GetInstanceID(), !isExpanded);
                    Event.current.Use();
                }
            }

            if (Event.current.type == EventType.Repaint)
            {
                Texture objIcon = hd.customIcon != null
                    ? hd.customIcon
                    : (EditorGUIUtility.GetIconForObject(go) ?? AssetPreview.GetMiniThumbnail(go));
                if (objIcon != null)
                {
                    float size = selectionRect.height - 2f;
                    GUI.color = palette.ForegroundColor;
                    GUI.DrawTexture(
                        new Rect(selectionRect.x, selectionRect.y + 1f, size, size),
                        objIcon,
                        ScaleMode.ScaleToFit);
                    GUI.color = Color.white;
                }

                DrawCustomLabel(selectionRect, bgRect, backgroundTexture, instanceID, go.name, hd);
            }
        }

        if ((hd == null || !hd.isOrganizer) && Event.current.type == EventType.Repaint)
        {
            var parentOrg = FindPropagatingAncestor(go);
            if (parentOrg != null)
            {
                parentOrg.EnsureColorData();
                float rowStartX = selectionRect.x - 42f;
                Rect rowRect = new Rect(rowStartX, selectionRect.y, Screen.width - rowStartX, selectionRect.height);
                Color childColor = ColorPaletteUtility.BuildPalette(parentOrg.colorMode, parentOrg.colorHue).ChildrenColor;
                GUI.DrawTexture(
                    rowRect,
                    GetOrCreateGradientTexture(childColor, HierarchyDesign.FadeMode.LeftToRight),
                    ScaleMode.StretchToFill);
            }
        }

        const float dotSize = 12f;
        Rect dotRect = new Rect(
            selectionRect.xMax - dotSize - 2f,
            selectionRect.y + (selectionRect.height - dotSize) * 0.5f,
            dotSize,
            dotSize);

        if (Event.current.type == EventType.Repaint)
        {
            Color dotColor = (hd != null && hd.isOrganizer)
                ? ColorPaletteUtility.BuildPalette(hd.colorMode, hd.colorHue).BaseColor
                : new Color(0.5f, 0.5f, 0.5f, 0.25f);
            GUI.DrawTexture(dotRect, GetOrCreateCircleTexture(dotColor), ScaleMode.StretchToFill);
        }

        if (Event.current.type == EventType.MouseDown &&
            dotRect.Contains(Event.current.mousePosition))
        {
            Event.current.Use();
            PopupWindow.Show(dotRect, new HierarchyDesignPopup(go));
        }
    }

    private static void DrawCustomLabel(
        Rect selectionRect,
        Rect backgroundRect,
        Texture2D backgroundTexture,
        int instanceID,
        string text,
        HierarchyDesign hd)
    {
        if (EditorGUIUtility.editingTextField && IsActiveSelection(instanceID) && IsHierarchyFocused())
            return;

        var palette = ColorPaletteUtility.BuildPalette(hd.colorMode, hd.colorHue);
        const float iconWidth = 16f;
        const float gap = 2f;
        Rect labelRect = new Rect(
            selectionRect.x + iconWidth + gap,
            selectionRect.y,
            selectionRect.width - iconWidth - gap - 20f,
            selectionRect.height);

        ClearNativeLabel(labelRect, selectionRect, backgroundRect, backgroundTexture, instanceID);

        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = palette.ForegroundColor },
            fontStyle = FontStyle.Normal,
            fontSize = hd.fontSize,
            alignment = TextAnchor.MiddleLeft
        };
        if (hd.customFont != null)
            style.font = hd.customFont;

        GUI.Label(labelRect, text, style);
    }

    private static void ClearNativeLabel(
        Rect labelRect,
        Rect selectionRect,
        Rect backgroundRect,
        Texture2D backgroundTexture,
        int instanceID)
    {
        Rect clearRect = labelRect;
        clearRect.xMin -= 1f;
        clearRect.xMax += 2f;

        EditorGUI.DrawRect(clearRect, GetHierarchyRowColor(selectionRect, instanceID));

        GUI.BeginClip(clearRect);
        GUI.DrawTexture(
            new Rect(
                backgroundRect.x - clearRect.x,
                backgroundRect.y - clearRect.y,
                backgroundRect.width,
                backgroundRect.height),
            backgroundTexture,
            ScaleMode.StretchToFill);
        GUI.EndClip();
    }

    private static Color GetHierarchyRowColor(Rect rect, int instanceID)
    {
        bool selected = IsSelected(instanceID);
        bool focused = IsHierarchyFocused();

        if (selected)
        {
            if (focused)
                return EditorGUIUtility.isProSkin
                    ? new Color(0.172f, 0.365f, 0.529f, 1f)
                    : new Color(0.243f, 0.490f, 0.902f, 1f);

            return EditorGUIUtility.isProSkin
                ? new Color(0.300f, 0.300f, 0.300f, 1f)
                : new Color(0.680f, 0.680f, 0.680f, 1f);
        }

        if (rect.Contains(Event.current.mousePosition))
        {
            return EditorGUIUtility.isProSkin
                ? new Color(0.270f, 0.270f, 0.270f, 1f)
                : new Color(0.800f, 0.800f, 0.800f, 1f);
        }

        return EditorGUIUtility.isProSkin
            ? new Color(0.219f, 0.219f, 0.219f, 1f)
            : new Color(0.760f, 0.760f, 0.760f, 1f);
    }

    private static bool IsSelected(int instanceID)
    {
        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            if (selectedObject != null && selectedObject.GetInstanceID() == instanceID)
                return true;
        }

        return false;
    }

    private static bool IsActiveSelection(int instanceID)
    {
        GameObject activeObject = Selection.activeGameObject;
        return activeObject != null && activeObject.GetInstanceID() == instanceID;
    }

    private static bool IsHierarchyFocused()
    {
        return EditorWindow.focusedWindow != null &&
               EditorWindow.focusedWindow.GetType().Name == "SceneHierarchyWindow";
    }

    public static Texture2D GetOrCreateGradientTexture(Color color, HierarchyDesign.FadeMode mode)
    {
        string key = $"grad_{ColorKey(color)}_{mode}";
        if (!Cache.TryGetValue(key, out Texture2D tex) || tex == null)
            Cache[key] = tex = CreateGradientTexture(color, mode);

        return tex;
    }

    private static Texture2D CreateGradientTexture(Color color, HierarchyDesign.FadeMode mode)
    {
        const int width = 64;
        Texture2D tex = new Texture2D(width, 1, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color transparent = new Color(color.r, color.g, color.b, 0f);
        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1);
            Color pixel;
            switch (mode)
            {
                case HierarchyDesign.FadeMode.LeftToRight:
                    pixel = Color.Lerp(color, transparent, t);
                    break;
                case HierarchyDesign.FadeMode.RightToLeft:
                    pixel = Color.Lerp(transparent, color, t);
                    break;
                case HierarchyDesign.FadeMode.CenterOut:
                    pixel = Color.Lerp(color, transparent, Mathf.Abs(t - 0.5f) * 2f);
                    break;
                case HierarchyDesign.FadeMode.CenterIn:
                    pixel = Color.Lerp(transparent, color, Mathf.Abs(t - 0.5f) * 2f);
                    break;
                default:
                    pixel = color;
                    break;
            }

            tex.SetPixel(x, 0, pixel);
        }

        tex.Apply();
        return tex;
    }

    public static Texture2D GetOrCreateCircleTexture(Color color, int size = 12)
    {
        string key = $"circle_{ColorKey(color)}_{size}";
        if (!Cache.TryGetValue(key, out Texture2D tex) || tex == null)
            Cache[key] = tex = CreateCircleTexture(color, size);

        return tex;
    }

    private static Texture2D CreateCircleTexture(Color color, int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f - 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                tex.SetPixel(
                    x,
                    y,
                    new Color(color.r, color.g, color.b, color.a * (1f - Mathf.Clamp01((distance - radius) / 1f))));
            }
        }

        tex.Apply();
        return tex;
    }

    public static Texture2D GetPreviewTexture(Color background)
    {
        return GetOrCreateGradientTexture(background, HierarchyDesign.FadeMode.LeftToRight);
    }

    public static void ClearCache()
    {
        foreach (var tex in Cache.Values)
        {
            if (tex != null)
                Object.DestroyImmediate(tex);
        }

        Cache.Clear();
    }

    private static string ColorKey(Color color) => $"{color.r:F3},{color.g:F3},{color.b:F3},{color.a:F3}";

    private static class HierarchyExpansion
    {
        private static bool _init;
        private static bool _failed;
        private static System.Type _winType;
        private static FieldInfo _hierField;
        private static FieldInfo _tvsField;
        private static FieldInfo _tvField;
        private static PropertyInfo _stateProp;
        private static FieldInfo _expandedIDsField;

        private static void Init()
        {
            _init = true;
            try
            {
                _winType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
                if (_winType == null)
                {
                    _failed = true;
                    return;
                }

                _hierField = _winType.GetField("m_SceneHierarchy", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_hierField == null)
                    _failed = true;
            }
            catch
            {
                _failed = true;
            }
        }

        private static List<int> GetExpandedIDs()
        {
            if (_failed)
                return null;

            if (!_init)
                Init();

            if (_failed)
                return null;

            try
            {
                var wins = Resources.FindObjectsOfTypeAll(_winType);
                if (wins == null || wins.Length == 0)
                    return null;

                var win = (EditorWindow)wins[0];
                var sh = _hierField.GetValue(win);
                if (sh == null)
                    return null;

                var hierarchyType = sh.GetType();
                object tvsObj = null;

                if (_tvsField == null)
                {
                    _tvsField = hierarchyType.GetField("m_TreeViewState", BindingFlags.Instance | BindingFlags.NonPublic);
                }

                tvsObj = _tvsField?.GetValue(sh);

                if (tvsObj == null)
                {
                    if (_tvField == null)
                    {
                        _tvField = hierarchyType.GetField("m_TreeView", BindingFlags.Instance | BindingFlags.NonPublic);
                    }

                    var tv = _tvField?.GetValue(sh);
                    if (tv != null)
                    {
                        if (_stateProp == null)
                        {
                            _stateProp = tv.GetType().GetProperty(
                                "state",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        }

                        tvsObj = _stateProp?.GetValue(tv);
                    }
                }

                if (tvsObj == null)
                    return null;

                if (_expandedIDsField == null || _expandedIDsField.DeclaringType != tvsObj.GetType())
                {
                    _expandedIDsField = tvsObj.GetType().GetField(
                        "expandedIDs",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }

                return _expandedIDsField?.GetValue(tvsObj) as List<int>;
            }
            catch
            {
                return null;
            }
        }

        public static void SetExpanded(int instanceID, bool expand)
        {
            var ids = GetExpandedIDs();
            if (ids == null)
                return;

            int idx = ids.BinarySearch(instanceID);
            if (expand && idx < 0)
                ids.Insert(~idx, instanceID);
            if (!expand && idx >= 0)
                ids.RemoveAt(idx);

            if (_winType == null)
                return;

            var wins = Resources.FindObjectsOfTypeAll(_winType);
            if (wins?.Length > 0)
                ((EditorWindow)wins[0]).Repaint();
        }
    }
}
}
