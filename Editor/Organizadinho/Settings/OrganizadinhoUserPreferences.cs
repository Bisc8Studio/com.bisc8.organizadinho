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
        private const string ShortcutModifiersKey = KeyPrefix + "shortcut.modifiers";
        private const string ShortcutMouseButtonKey = KeyPrefix + "shortcut.mouseButton";

        internal const EventModifiers DefaultShortcutModifiers = EventModifiers.Alt;
        internal const int DefaultShortcutMouseButton = 0;
        internal const EventModifiers ShortcutModifierMask =
            EventModifiers.Alt | EventModifiers.Control | EventModifiers.Shift | EventModifiers.Command;

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

        internal static EventModifiers ShortcutModifiers
        {
            get => (EventModifiers)EditorPrefs.GetInt(ShortcutModifiersKey, (int)DefaultShortcutModifiers) & ShortcutModifierMask;
            set => SetShortcutModifiers(value);
        }

        internal static int ShortcutMouseButton
        {
            get => GetShortcutMouseButton();
            set => SetShortcutMouseButton(value);
        }

        /// <summary>
        /// Returns true when the event's modifier keys match the configured shortcut combination
        /// used to open folder/hierarchy customization popups.
        /// </summary>
        internal static bool MatchesShortcutModifiers(Event e)
        {
            return (e.modifiers & ShortcutModifierMask) == ShortcutModifiers;
        }

        internal static bool MatchesPopupShortcut(Event e)
        {
            return e != null &&
                   e.type == EventType.MouseDown &&
                   e.button == ShortcutMouseButton &&
                   MatchesShortcutModifiers(e);
        }

        internal static void ResetDefaults()
        {
            EditorPrefs.DeleteKey(PastelPaletteKey);
            EditorPrefs.DeleteKey(VibrantPaletteKey);
            EditorPrefs.DeleteKey(CustomColorKey);
            EditorPrefs.DeleteKey(FolderColorsKey);
            EditorPrefs.DeleteKey(HierarchyColorsKey);
            EditorPrefs.DeleteKey(ProjectToolbarKey);
            EditorPrefs.DeleteKey(ShortcutModifiersKey);
            EditorPrefs.DeleteKey(ShortcutMouseButtonKey);
            NotifyChanged();
        }

        private static void SetShortcutModifiers(EventModifiers value)
        {
            value &= ShortcutModifierMask;
            if (((EventModifiers)EditorPrefs.GetInt(ShortcutModifiersKey, (int)DefaultShortcutModifiers) & ShortcutModifierMask) == value)
            {
                return;
            }

            EditorPrefs.SetInt(ShortcutModifiersKey, (int)value);
            NotifyChanged();
        }

        private static int GetShortcutMouseButton()
        {
            var button = EditorPrefs.GetInt(ShortcutMouseButtonKey, DefaultShortcutMouseButton);
            return button == 0 || button == 1 ? button : DefaultShortcutMouseButton;
        }

        private static void SetShortcutMouseButton(int value)
        {
            value = value == 0 ? 0 : 1;
            if (GetShortcutMouseButton() == value)
            {
                return;
            }

            EditorPrefs.SetInt(ShortcutMouseButtonKey, value);
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
