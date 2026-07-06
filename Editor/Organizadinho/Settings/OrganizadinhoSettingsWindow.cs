using UnityEditor;
using UnityEngine;

namespace Organizadinho.Editor.Settings
{
    internal sealed class OrganizadinhoSettingsWindow : EditorWindow
    {
        private const float MinWindowWidth = 340f;
        private const float MinWindowHeight = 230f;

        [MenuItem("Organizadinho/Settings")]
        private static void Open()
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
            var showPastel = EditorGUILayout.Toggle("Pastel", OrganizadinhoUserPreferences.ShowPastelPalette);
            var showVibrant = EditorGUILayout.Toggle("Vibrant", OrganizadinhoUserPreferences.ShowVibrantPalette);
            if (!showPastel && !showVibrant)
            {
                EditorGUILayout.HelpBox("At least one palette must stay enabled.", MessageType.Warning);
                showPastel = OrganizadinhoUserPreferences.ShowPastelPalette;
                showVibrant = OrganizadinhoUserPreferences.ShowVibrantPalette;
            }

            if (EditorGUI.EndChangeCheck())
            {
                OrganizadinhoUserPreferences.ShowPastelPalette = showPastel;
                OrganizadinhoUserPreferences.ShowVibrantPalette = showVibrant;
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

            GUILayout.FlexibleSpace();
            DrawHorizontalRule();

            if (GUILayout.Button("Reset Local Settings", GUILayout.Height(24f)))
            {
                OrganizadinhoUserPreferences.ResetDefaults();
            }
        }

        private static void DrawHorizontalRule()
        {
            GUILayout.Space(4f);
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, EditorGUIUtility.isProSkin ? 0.4f : 0.18f));
            GUILayout.Space(4f);
        }
    }
}
