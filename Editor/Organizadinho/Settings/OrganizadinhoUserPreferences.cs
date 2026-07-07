using System;
using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.Drawing;
using Organizadinho.Editor.Utilities;

namespace Organizadinho.Editor.Settings
{
    internal static class OrganizadinhoUserPreferences
    {
        private const string KeyPrefix = "com.bisc8.organizadinho.";
        private const string PastelPaletteKey = KeyPrefix + "palette.pastel";
        private const string VibrantPaletteKey = KeyPrefix + "palette.vibrant";
        private const string CustomColorKey = KeyPrefix + "palette.custom";
        private const string FolderColorsKey = KeyPrefix + "effects.folderColors";
        private const string HierarchyColorsKey = KeyPrefix + "effects.hierarchyColors";
        private const string ProjectToolbarKey = KeyPrefix + "effects.projectToolbar";
        private const string UnityEditorTintKey = KeyPrefix + "unity.tint.enabled";
        private const string UnityEditorTintColorKey = KeyPrefix + "unity.tint.color";
        private const string UnityEditorTintStrengthKey = KeyPrefix + "unity.tint.strength";

        internal static event Action Changed;

        internal static bool ShowPastelPalette
        {
            get => EditorPrefs.GetBool(PastelPaletteKey, true);
            set => SetBool(PastelPaletteKey, value);
        }

        internal static bool ShowVibrantPalette
        {
            get => EditorPrefs.GetBool(VibrantPaletteKey, true);
            set => SetBool(VibrantPaletteKey, value);
        }

        internal static bool ShowCustomColor
        {
            get => EditorPrefs.GetBool(CustomColorKey, true);
            set => SetBool(CustomColorKey, value);
        }

        internal static bool EnableFolderColors
        {
            get => EditorPrefs.GetBool(FolderColorsKey, true);
            set => SetBool(FolderColorsKey, value);
        }

        internal static bool EnableHierarchyColors
        {
            get => EditorPrefs.GetBool(HierarchyColorsKey, true);
            set => SetBool(HierarchyColorsKey, value);
        }

        internal static bool EnableProjectToolbar
        {
            get => EditorPrefs.GetBool(ProjectToolbarKey, true);
            set => SetBool(ProjectToolbarKey, value);
        }

        internal static bool EnableUnityEditorTint
        {
            get => EditorPrefs.GetBool(UnityEditorTintKey, false);
            set => SetBool(UnityEditorTintKey, value, false);
        }

        internal static Color UnityEditorTintColor
        {
            get => GetColor(UnityEditorTintColorKey, new Color(0.18f, 0.48f, 0.9f, 1f));
            set => SetColor(UnityEditorTintColorKey, value);
        }

        internal static float UnityEditorTintStrength
        {
            get => EditorPrefs.GetFloat(UnityEditorTintStrengthKey, 0.12f);
            set => SetFloat(UnityEditorTintStrengthKey, Mathf.Clamp01(value), 0.12f);
        }

        internal static void ResetDefaults()
        {
            EditorPrefs.DeleteKey(PastelPaletteKey);
            EditorPrefs.DeleteKey(VibrantPaletteKey);
            EditorPrefs.DeleteKey(CustomColorKey);
            EditorPrefs.DeleteKey(FolderColorsKey);
            EditorPrefs.DeleteKey(HierarchyColorsKey);
            EditorPrefs.DeleteKey(ProjectToolbarKey);
            EditorPrefs.DeleteKey(UnityEditorTintKey);
            EditorPrefs.DeleteKey(UnityEditorTintColorKey);
            EditorPrefs.DeleteKey(UnityEditorTintStrengthKey);
            NotifyChanged();
        }

        private static void SetBool(string key, bool value, bool defaultValue = true)
        {
            if (EditorPrefs.GetBool(key, defaultValue) == value)
            {
                return;
            }

            EditorPrefs.SetBool(key, value);
            NotifyChanged();
        }

        private static Color GetColor(string key, Color defaultColor)
        {
            var stored = EditorPrefs.GetString(key, string.Empty);
            if (ColorUtility.TryParseHtmlString("#" + stored, out var color))
            {
                color.a = 1f;
                return color;
            }

            return defaultColor;
        }

        private static void SetColor(string key, Color value)
        {
            value.a = 1f;
            var stored = ColorUtility.ToHtmlStringRGBA(value);
            if (EditorPrefs.GetString(key, string.Empty) == stored)
            {
                return;
            }

            EditorPrefs.SetString(key, stored);
            NotifyChanged();
        }

        private static void SetFloat(string key, float value, float defaultValue)
        {
            if (Mathf.Approximately(EditorPrefs.GetFloat(key, defaultValue), value))
            {
                return;
            }

            EditorPrefs.SetFloat(key, value);
            NotifyChanged();
        }

        private static void NotifyChanged()
        {
            FolderDesignRenderCache.InvalidateStyles();
            HierarchyDesignDrawer.ClearCache();
            EditorApplication.RepaintProjectWindow();
            EditorApplication.RepaintHierarchyWindow();
            Changed?.Invoke();
        }
    }
}
