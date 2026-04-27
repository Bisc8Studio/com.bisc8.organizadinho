using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.Drawing;
using Organizadinho.Editor.Utilities;
using Organizadinho.Runtime;

namespace Organizadinho.Editor.UI
{

[CustomEditor(typeof(HierarchyDesign))]
public class HierarchyDesignInspector : UnityEditor.Editor
{
    private void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    private void OnUndoRedo()
    {
        HierarchyDesignDrawer.ClearCache();
        EditorApplication.RepaintHierarchyWindow();
    }

    public override void OnInspectorGUI()
    {
        HierarchyDesign hd = (HierarchyDesign)target;

        if (hd.isOrganizer)
        {
            hd.EnsureColorData();
            var palette = ColorPaletteUtility.BuildPalette(hd.colorHue);
            GUILayout.Space(4f);
            Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(28f));

            Texture2D previewTex = HierarchyDesignDrawer.GetPreviewTexture(palette.BaseColor);
            GUI.DrawTexture(previewRect, previewTex, ScaleMode.StretchToFill);

            GUIStyle style = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = hd.fontSize,
                fontStyle = hd.fontStyle,
                normal = { textColor = palette.ForegroundColor }
            };
            if (hd.customFont != null)
                style.font = hd.customFont;

            GUI.Label(
                new Rect(previewRect.x + 6f, previewRect.y, previewRect.width - 10f, previewRect.height),
                hd.gameObject.name,
                style);

            GUILayout.Space(4f);
            Rect rule = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
            EditorGUI.DrawRect(rule, palette.BorderColor);
            GUILayout.Space(4f);
        }

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            HierarchyDesignDrawer.ClearCache();
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}
}
