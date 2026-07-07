using UnityEditor;
using UnityEngine;
using UnityOrganizer.Editor.UI;
using UnityOrganizer.Runtime;

namespace UnityOrganizer.Editor.Settings
{
    internal sealed class UnityOrganizerSettingsWindow : EditorWindow
    {
        private const float MinWindowWidth = 340f;
        private const float MinWindowHeight = 230f;
        private const float PalettePreviewWidth = 170f;
        private static readonly EventModifiers[] ShortcutModifierOptions =
        {
            EventModifiers.Alt,
            EventModifiers.Control,
            EventModifiers.Shift,
            EventModifiers.Alt | EventModifiers.Control,
            EventModifiers.Alt | EventModifiers.Shift,
            EventModifiers.Control | EventModifiers.Shift
        };

        private static readonly string[] ShortcutModifierLabels =
        {
            "Alt",
            "Ctrl",
            "Shift",
            "Alt + Ctrl",
            "Alt + Shift",
            "Ctrl + Shift"
        };

        private static readonly int[] ShortcutMouseButtonOptions =
        {
            0,
            1
        };

        private static readonly string[] ShortcutMouseButtonLabels =
        {
            "Left Click",
            "Right Click"
        };

        [MenuItem("Tools/Unity Organizer/Settings")]
        private static void Open()
        {
            var window = GetWindow<UnityOrganizerSettingsWindow>("Unity Organizer");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Unity Organizer Settings", EditorStyles.boldLabel);
            DrawHorizontalRule();

            EditorGUILayout.LabelField("Color Palettes", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These palettes appear in folder and hierarchy color pickers. White and Black stay available as fixed colors.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            var showPastel = DrawPaletteToggle("Pastel", UnityOrganizerColorMode.Pastel, UnityOrganizerUserPreferences.ShowPastelPalette);
            var showVibrant = DrawPaletteToggle("Vibrant", UnityOrganizerColorMode.Vibrant, UnityOrganizerUserPreferences.ShowVibrantPalette);
            var showCustom = DrawCustomColorToggle(UnityOrganizerUserPreferences.ShowCustomColor);
            if (!showPastel && !showVibrant && !showCustom)
            {
                EditorGUILayout.HelpBox("At least one color option must stay enabled.", MessageType.Warning);
                showPastel = UnityOrganizerUserPreferences.ShowPastelPalette;
                showVibrant = UnityOrganizerUserPreferences.ShowVibrantPalette;
                showCustom = UnityOrganizerUserPreferences.ShowCustomColor;
            }

            if (EditorGUI.EndChangeCheck())
            {
                UnityOrganizerUserPreferences.ShowPastelPalette = showPastel;
                UnityOrganizerUserPreferences.ShowVibrantPalette = showVibrant;
                UnityOrganizerUserPreferences.ShowCustomColor = showCustom;
            }

            DrawHorizontalRule();

            EditorGUILayout.LabelField("Popup Shortcut", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var modifierIndex = EditorGUILayout.Popup(
                "Keyboard",
                GetShortcutModifierIndex(UnityOrganizerUserPreferences.ShortcutModifiers),
                ShortcutModifierLabels);
            var mouseButtonIndex = EditorGUILayout.Popup(
                "Mouse",
                GetShortcutMouseButtonIndex(UnityOrganizerUserPreferences.ShortcutMouseButton),
                ShortcutMouseButtonLabels);
            if (EditorGUI.EndChangeCheck())
            {
                UnityOrganizerUserPreferences.ShortcutModifiers = ShortcutModifierOptions[modifierIndex];
                UnityOrganizerUserPreferences.ShortcutMouseButton = ShortcutMouseButtonOptions[mouseButtonIndex];
            }

            DrawHorizontalRule();

            EditorGUILayout.LabelField("Local Effects", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These options are saved only in this editor user profile and are not committed to git.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            var enableFolders = EditorGUILayout.Toggle("Folder colors", UnityOrganizerUserPreferences.EnableFolderColors);
            var enableHierarchy = EditorGUILayout.Toggle("Hierarchy colors", UnityOrganizerUserPreferences.EnableHierarchyColors);
            var enableToolbar = EditorGUILayout.Toggle("Custom project toolbar", UnityOrganizerUserPreferences.EnableProjectToolbar);
            if (EditorGUI.EndChangeCheck())
            {
                UnityOrganizerUserPreferences.EnableFolderColors = enableFolders;
                UnityOrganizerUserPreferences.EnableHierarchyColors = enableHierarchy;
                UnityOrganizerUserPreferences.EnableProjectToolbar = enableToolbar;
            }

            GUILayout.FlexibleSpace();
            DrawHorizontalRule();

            if (GUILayout.Button("Reset Local Settings", GUILayout.Height(24f)))
            {
                UnityOrganizerUserPreferences.ResetDefaults();
            }
        }

        private static void DrawHorizontalRule()
        {
            GUILayout.Space(4f);
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, EditorGUIUtility.isProSkin ? 0.4f : 0.18f));
            GUILayout.Space(4f);
        }

        private static bool DrawPaletteToggle(string label, UnityOrganizerColorMode mode, bool value)
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

        private static int GetShortcutModifierIndex(EventModifiers modifiers)
        {
            modifiers &= UnityOrganizerUserPreferences.ShortcutModifierMask;
            for (var index = 0; index < ShortcutModifierOptions.Length; index++)
            {
                if (ShortcutModifierOptions[index] == modifiers)
                {
                    return index;
                }
            }

            UnityOrganizerUserPreferences.ShortcutModifiers = UnityOrganizerUserPreferences.DefaultShortcutModifiers;
            return 0;
        }

        private static int GetShortcutMouseButtonIndex(int mouseButton)
        {
            for (var index = 0; index < ShortcutMouseButtonOptions.Length; index++)
            {
                if (ShortcutMouseButtonOptions[index] == mouseButton)
                {
                    return index;
                }
            }

            UnityOrganizerUserPreferences.ShortcutMouseButton = UnityOrganizerUserPreferences.DefaultShortcutMouseButton;
            return 0;
        }
    }
}
