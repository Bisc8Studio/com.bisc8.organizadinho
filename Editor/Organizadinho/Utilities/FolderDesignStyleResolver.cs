using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Organizadinho.Editor.Storage;

namespace Organizadinho.Editor.Utilities
{
    internal enum FolderDesignColorSource
    {
        None,
        Direct,
        Inherited
    }

    internal readonly struct FolderDesignResolvedStyle
    {
        internal FolderDesignResolvedStyle(
            bool hasDirectConfiguration,
            bool hasResolvedColor,
            Color resolvedColor,
            FolderDesignColorSource colorSource,
            string iconGuid)
        {
            HasDirectConfiguration = hasDirectConfiguration;
            HasResolvedColor = hasResolvedColor;
            ResolvedColor = resolvedColor;
            ColorSource = colorSource;
            IconGuid = iconGuid ?? string.Empty;
        }

        internal bool HasDirectConfiguration { get; }
        internal bool HasResolvedColor { get; }
        internal Color ResolvedColor { get; }
        internal FolderDesignColorSource ColorSource { get; }
        internal string IconGuid { get; }
        internal bool IsColorInherited => ColorSource == FolderDesignColorSource.Inherited;
        internal bool HasCustomIcon => !string.IsNullOrEmpty(IconGuid);
        internal bool HasVisualOverride => HasResolvedColor || HasCustomIcon;
    }

    internal static class FolderDesignStyleResolver
    {
        private static readonly Dictionary<string, FolderDesignResolvedStyle> StyleCache =
            new Dictionary<string, FolderDesignResolvedStyle>(StringComparer.Ordinal);

        private static readonly Dictionary<string, Texture2D> IconCache =
            new Dictionary<string, Texture2D>(StringComparer.Ordinal);

        static FolderDesignStyleResolver()
        {
            FolderDesignStorage.Changed += Invalidate;
            EditorApplication.projectChanged += Invalidate;
        }

        internal static FolderDesignResolvedStyle Resolve(string folderGuid, string assetPath = null)
        {
            if (string.IsNullOrEmpty(folderGuid) && string.IsNullOrEmpty(assetPath))
                return default;

            if (!string.IsNullOrEmpty(folderGuid) && StyleCache.TryGetValue(folderGuid, out var cachedStyle))
                return cachedStyle;

            assetPath = NormalizePath(assetPath);
            if (string.IsNullOrEmpty(folderGuid))
                folderGuid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(assetPath))
                assetPath = NormalizePath(AssetDatabase.GUIDToAssetPath(folderGuid));

            if (string.IsNullOrEmpty(folderGuid) || !AssetDatabase.IsValidFolder(assetPath))
                return default;

            var storage = FolderDesignStorage.GetOrCreate();
            var directEntry = storage.GetEntry(folderGuid);
            var colorEntry = directEntry != null && directEntry.hasColor
                ? directEntry
                : FindPropagatingAncestor(assetPath, storage);

            var colorSource = FolderDesignColorSource.None;
            if (colorEntry != null && colorEntry.hasColor)
            {
                colorSource = ReferenceEquals(colorEntry, directEntry)
                    ? FolderDesignColorSource.Direct
                    : FolderDesignColorSource.Inherited;
            }

            var hasDirectConfiguration = directEntry != null &&
                                         (directEntry.hasColor || directEntry.propagateChildren || !string.IsNullOrEmpty(directEntry.iconGuid));

            var resolvedStyle = new FolderDesignResolvedStyle(
                hasDirectConfiguration,
                colorEntry != null && colorEntry.hasColor,
                colorEntry != null && colorEntry.hasColor ? PastelColorUtility.FromHue(colorEntry.hue) : default,
                colorSource,
                directEntry?.iconGuid);

            StyleCache[folderGuid] = resolvedStyle;
            return resolvedStyle;
        }

        internal static Texture2D LoadBadgeIcon(string iconGuid)
        {
            if (string.IsNullOrEmpty(iconGuid))
                return null;

            if (IconCache.TryGetValue(iconGuid, out var cachedIcon))
                return cachedIcon;

            var iconPath = AssetDatabase.GUIDToAssetPath(iconGuid);
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            IconCache[iconGuid] = icon;
            return icon;
        }

        internal static Color GetProjectItemFillColor(FolderDesignResolvedStyle style, bool isSelected, bool isHovered)
        {
            if (!style.HasResolvedColor)
                return Color.clear;

            var baseColor = style.IsColorInherited
                ? Color.Lerp(style.ResolvedColor, PastelColorUtility.GetProjectChromeColor(), 0.42f)
                : style.ResolvedColor;

            var alpha = style.IsColorInherited ? 0.14f : 0.2f;
            if (isHovered)
                alpha += 0.035f;

            if (isSelected)
            {
                baseColor = Color.Lerp(style.ResolvedColor, PastelColorUtility.GetUnitySelectionColor(), 0.52f);
                alpha = 0.28f;
            }

            baseColor.a = alpha;
            return baseColor;
        }

        internal static Color GetProjectItemAccentColor(FolderDesignResolvedStyle style, bool isSelected)
        {
            if (!style.HasResolvedColor)
                return Color.clear;

            var accent = style.IsColorInherited
                ? Color.Lerp(style.ResolvedColor, Color.white, 0.1f)
                : style.ResolvedColor;

            if (isSelected)
                accent = Color.Lerp(accent, PastelColorUtility.GetUnitySelectionColor(), 0.3f);

            accent.a = isSelected ? 0.95f : 0.9f;
            return accent;
        }

        internal static Color GetProjectItemIconTint(FolderDesignResolvedStyle style, bool isSelected)
        {
            if (!style.HasResolvedColor)
                return Color.white;

            var tint = style.IsColorInherited
                ? Color.Lerp(style.ResolvedColor, PastelColorUtility.GetProjectChromeColor(), 0.24f)
                : style.ResolvedColor;

            if (isSelected)
                tint = Color.Lerp(tint, PastelColorUtility.GetUnitySelectionColor(), 0.18f);

            tint.a = 1f;
            return tint;
        }

        internal static Color GetShortcutBackgroundColor(FolderDesignResolvedStyle style)
        {
            if (!style.HasResolvedColor)
                return EditorGUIUtility.isProSkin
                    ? new Color(0.23f, 0.23f, 0.23f, 1f)
                    : new Color(0.84f, 0.84f, 0.84f, 1f);

            var chrome = PastelColorUtility.GetProjectChromeColor();
            var background = style.IsColorInherited
                ? Color.Lerp(style.ResolvedColor, chrome, 0.35f)
                : Color.Lerp(style.ResolvedColor, chrome, 0.18f);
            background.a = 1f;
            return background;
        }

        internal static Color GetShortcutBorderColor(FolderDesignResolvedStyle style)
        {
            if (!style.HasResolvedColor)
                return EditorGUIUtility.isProSkin
                    ? new Color(0f, 0f, 0f, 0.5f)
                    : new Color(0f, 0f, 0f, 0.2f);

            var border = Color.Lerp(style.ResolvedColor, Color.black, 0.42f);
            border.a = 0.85f;
            return border;
        }

        internal static Color GetReadableTextColor(Color backgroundColor)
        {
            return PastelColorUtility.GetReadableTextColor(backgroundColor);
        }

        internal static void Invalidate()
        {
            StyleCache.Clear();
            IconCache.Clear();
        }

        private static FolderDesignEntry FindPropagatingAncestor(string folderPath, FolderDesignStorage storage)
        {
            var current = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            while (!string.IsNullOrEmpty(current) && current != ".")
            {
                var parentGuid = AssetDatabase.AssetPathToGUID(current);
                if (!string.IsNullOrEmpty(parentGuid))
                {
                    var entry = storage.GetEntry(parentGuid);
                    if (entry != null && entry.hasColor && entry.propagateChildren)
                        return entry;
                }

                current = Path.GetDirectoryName(current)?.Replace('\\', '/');
            }

            return null;
        }

        private static string NormalizePath(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Replace('\\', '/').Trim();
        }
    }
}
