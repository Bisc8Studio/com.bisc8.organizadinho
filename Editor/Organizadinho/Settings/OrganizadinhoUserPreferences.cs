using System;
using UnityEditor;
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

        internal static void ResetDefaults()
        {
            EditorPrefs.DeleteKey(PastelPaletteKey);
            EditorPrefs.DeleteKey(VibrantPaletteKey);
            EditorPrefs.DeleteKey(CustomColorKey);
            EditorPrefs.DeleteKey(FolderColorsKey);
            EditorPrefs.DeleteKey(HierarchyColorsKey);
            EditorPrefs.DeleteKey(ProjectToolbarKey);
            NotifyChanged();
        }

        private static void SetBool(string key, bool value)
        {
            if (EditorPrefs.GetBool(key, true) == value)
            {
                return;
            }

            EditorPrefs.SetBool(key, value);
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
