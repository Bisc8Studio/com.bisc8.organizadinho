using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.UI;
using Organizadinho.Runtime;

namespace Organizadinho.Editor.Settings
{
    internal sealed class OrganizadinhoSettingsWindow : EditorWindow
    {
        private const float MinWindowWidth = 340f;
        private const float MinWindowHeight = 320f;
        private const float PalettePreviewWidth = 170f;

        [MenuItem("Organizadinho/Settings")]
        internal static void Open()
        {
            var window = GetWindow<OrganizadinhoSettingsWindow>("Organizadinho");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Organizadinho Settings", EditorStyles.boldLabel);
            DrawHorizontalRule();

            EditorGUILayout.LabelField("Color Palettes", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These palettes appear in folder and hierarchy color pickers. White and Black stay available as fixed colors.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            var showPastel = DrawPaletteToggle("Pastel", OrganizadinhoColorMode.Pastel, OrganizadinhoUserPreferences.ShowPastelPalette);
            var showVibrant = DrawPaletteToggle("Vibrant", OrganizadinhoColorMode.Vibrant, OrganizadinhoUserPreferences.ShowVibrantPalette);
            var showCustom = DrawCustomColorToggle(OrganizadinhoUserPreferences.ShowCustomColor);
            if (!showPastel && !showVibrant && !showCustom)
            {
                EditorGUILayout.HelpBox("At least one color option must stay enabled.", MessageType.Warning);
                showPastel = OrganizadinhoUserPreferences.ShowPastelPalette;
                showVibrant = OrganizadinhoUserPreferences.ShowVibrantPalette;
                showCustom = OrganizadinhoUserPreferences.ShowCustomColor;
            }

            if (EditorGUI.EndChangeCheck())
            {
                OrganizadinhoUserPreferences.ShowPastelPalette = showPastel;
                OrganizadinhoUserPreferences.ShowVibrantPalette = showVibrant;
                OrganizadinhoUserPreferences.ShowCustomColor = showCustom;
            }

            DrawHorizontalRule();

            EditorGUILayout.LabelField("Local Effects", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These options are saved only in this editor user profile and are not committed to git.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            var enableFolders = EditorGUILayout.Toggle("Folder colors", OrganizadinhoUserPreferences.EnableFolderColors);
            var enableHierarchy = EditorGUILayout.Toggle("Hierarchy colors", OrganizadinhoUserPreferences.EnableHierarchyColors);
            var enableToolbar = EditorGUILayout.Toggle("Custom project toolbar", OrganizadinhoUserPreferences.EnableProjectToolbar);
            if (EditorGUI.EndChangeCheck())
            {
                OrganizadinhoUserPreferences.EnableFolderColors = enableFolders;
                OrganizadinhoUserPreferences.EnableHierarchyColors = enableHierarchy;
                OrganizadinhoUserPreferences.EnableProjectToolbar = enableToolbar;
            }

            DrawHorizontalRule();

            EditorGUILayout.LabelField("Unity Color Test", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Experimental local tint over Unity editor windows. It does not change Unity's native skin files.",
                MessageType.Warning);

            EditorGUI.BeginChangeCheck();
            var enableUnityTint = EditorGUILayout.Toggle("Tint Unity editor", OrganizadinhoUserPreferences.EnableUnityEditorTint);
            using (new EditorGUI.DisabledScope(!enableUnityTint))
            {
                var tintColor = EditorGUILayout.ColorField("Tint color", OrganizadinhoUserPreferences.UnityEditorTintColor);
                var tintStrength = EditorGUILayout.Slider("Tint strength", OrganizadinhoUserPreferences.UnityEditorTintStrength, 0.02f, 0.35f);

                if (EditorGUI.EndChangeCheck())
                {
                    OrganizadinhoUserPreferences.EnableUnityEditorTint = enableUnityTint;
                    OrganizadinhoUserPreferences.UnityEditorTintColor = tintColor;
                    OrganizadinhoUserPreferences.UnityEditorTintStrength = tintStrength;
                    UnityEditorTint.ApplyToOpenWindows();
                }
            }

            GUILayout.FlexibleSpace();
            DrawHorizontalRule();

            if (GUILayout.Button("Reset Local Settings", GUILayout.Height(24f)))
            {
                OrganizadinhoUserPreferences.ResetDefaults();
                UnityEditorTint.ApplyToOpenWindows();
            }
        }

        private static void DrawHorizontalRule()
        {
            GUILayout.Space(4f);
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, EditorGUIUtility.isProSkin ? 0.4f : 0.18f));
            GUILayout.Space(4f);
        }

        private static bool DrawPaletteToggle(string label, OrganizadinhoColorMode mode, bool value)
        {
            var rowRect = EditorGUILayout.GetControlRect(GUILayout.Height(24f));
            var toggleRect = new Rect(rowRect.x, rowRect.y + 2f, 82f, EditorGUIUtility.singleLineHeight);
            var previewRect = new Rect(
                toggleRect.xMax + 8f,
                rowRect.y + 3f,
                Mathf.Min(PalettePreviewWidth, rowRect.width - toggleRect.width - 8f),
                18f);

            value = EditorGUI.ToggleLeft(toggleRect, label, value);
            ColorHueSlider.DrawPalettePreview(previewRect, mode, value);
            return value;
        }

        private static bool DrawCustomColorToggle(bool value)
        {
            var rowRect = EditorGUILayout.GetControlRect(GUILayout.Height(24f));
            var toggleRect = new Rect(rowRect.x, rowRect.y + 2f, 82f, EditorGUIUtility.singleLineHeight);
            var previewRect = new Rect(
                toggleRect.xMax + 8f,
                rowRect.y + 2f,
                70f,
                EditorGUIUtility.singleLineHeight);

            value = EditorGUI.ToggleLeft(toggleRect, "Custom", value);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.ColorField(previewRect, GUIContent.none, new Color(0.62f, 0.78f, 0.96f, value ? 1f : 0.45f), false, false, false);
            }

            return value;
        }
    }
}
