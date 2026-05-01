using UnityEditor;
using UnityEngine;
using Organizadinho.Runtime;

namespace Organizadinho.Editor.Utilities
{
    internal readonly struct ColorPalette
    {
        internal ColorPalette(
            Color baseColor,
            Color foregroundColor,
            Color toolbarForegroundColor,
            Color chromeColor,
            Color hoverColor,
            Color selectedColor,
            Color borderColor,
            Color childrenColor,
            Color shortcutColor,
            Color toolbarColor)
        {
            BaseColor = baseColor;
            ForegroundColor = foregroundColor;
            ToolbarForegroundColor = toolbarForegroundColor;
            ChromeColor = chromeColor;
            HoverColor = hoverColor;
            SelectedColor = selectedColor;
            BorderColor = borderColor;
            ChildrenColor = childrenColor;
            ShortcutColor = shortcutColor;
            ToolbarColor = toolbarColor;
        }

        internal Color BaseColor { get; }
        internal Color ForegroundColor { get; }
        internal Color ToolbarForegroundColor { get; }
        internal Color ChromeColor { get; }
        internal Color HoverColor { get; }
        internal Color SelectedColor { get; }
        internal Color BorderColor { get; }
        internal Color ChildrenColor { get; }
        internal Color ShortcutColor { get; }
        internal Color ToolbarColor { get; }
    }

    internal static class ColorPaletteUtility
    {
        internal const float DefaultHue = 0.58f;
        private const float PastelSaturation = 0.34f;
        private const float PastelValue = 0.96f;
        private const float VibrantSaturation = 0.82f;
        private const float VibrantValue = 0.94f;
        private const float PastelChildrenSaturation = 0.24f;
        private const float PastelChildrenValue = 0.99f;
        private const float VibrantChildrenSaturation = 0.46f;
        private const float VibrantChildrenValue = 0.96f;

        internal static ColorPalette BuildPalette(float hue)
        {
            return BuildPalette(OrganizadinhoColorMode.Pastel, hue);
        }

        internal static ColorPalette BuildPalette(OrganizadinhoColorMode mode, float hue)
        {
            var normalizedHue = Mathf.Repeat(hue, 1f);
            var baseColor = GetBaseColor(mode, normalizedHue);
            var chrome = GetProjectChromeColor();
            var selection = GetUnitySelectionColor();
            var isVibrant = mode == OrganizadinhoColorMode.Vibrant;
            var toolbar = Color.Lerp(baseColor, chrome, EditorGUIUtility.isProSkin
                ? (isVibrant ? 0.86f : 0.8f)
                : (isVibrant ? 0.9f : 0.88f));
            var shortcut = Color.Lerp(baseColor, chrome, EditorGUIUtility.isProSkin
                ? (isVibrant ? 0.58f : 0.42f)
                : (isVibrant ? 0.68f : 0.56f));
            var hover = Color.Lerp(baseColor, chrome, EditorGUIUtility.isProSkin
                ? (isVibrant ? 0.68f : 0.56f)
                : (isVibrant ? 0.76f : 0.7f));
            var selected = Color.Lerp(baseColor, selection, isVibrant ? 0.34f : 0.28f);
            var border = GetBorderColor(mode, baseColor);

            var children = GetChildrenColor(mode, normalizedHue);

            return new ColorPalette(
                baseColor,
                GetReadableTextColor(baseColor),
                GetReadableTextColor(toolbar),
                chrome,
                hover,
                selected,
                border,
                children,
                shortcut,
                toolbar);
        }

        internal static Color FromHue(float hue)
        {
            return GetBaseColor(OrganizadinhoColorMode.Pastel, hue);
        }

        internal static Color FromHue(OrganizadinhoColorMode mode, float hue)
        {
            return GetBaseColor(mode, hue);
        }

        internal static Color GetBaseColor(OrganizadinhoColorMode mode, float hue)
        {
            switch (mode)
            {
                case OrganizadinhoColorMode.White:
                    return EditorGUIUtility.isProSkin
                        ? new Color(0.92f, 0.92f, 0.9f, 1f)
                        : new Color(0.98f, 0.98f, 0.96f, 1f);
                case OrganizadinhoColorMode.Black:
                    return EditorGUIUtility.isProSkin
                        ? new Color(0.08f, 0.08f, 0.085f, 1f)
                        : new Color(0.14f, 0.14f, 0.145f, 1f);
                case OrganizadinhoColorMode.Vibrant:
                    return FromHue(hue, VibrantSaturation, VibrantValue);
                default:
                    return FromHue(hue, PastelSaturation, PastelValue);
            }
        }

        private static Color GetChildrenColor(OrganizadinhoColorMode mode, float hue)
        {
            Color children;
            switch (mode)
            {
                case OrganizadinhoColorMode.White:
                    children = EditorGUIUtility.isProSkin
                        ? new Color(0.9f, 0.9f, 0.88f, 1f)
                        : new Color(1f, 1f, 1f, 1f);
                    break;
                case OrganizadinhoColorMode.Black:
                    children = EditorGUIUtility.isProSkin
                        ? new Color(0.16f, 0.16f, 0.17f, 1f)
                        : new Color(0.08f, 0.08f, 0.085f, 1f);
                    break;
                case OrganizadinhoColorMode.Vibrant:
                    children = FromHue(hue, VibrantChildrenSaturation, VibrantChildrenValue);
                    break;
                default:
                    children = FromHue(hue, PastelChildrenSaturation, PastelChildrenValue);
                    break;
            }

            children.a = mode == OrganizadinhoColorMode.Black
                ? 0.28f
                : (mode == OrganizadinhoColorMode.Vibrant ? 0.24f : 0.35f);
            return children;
        }

        private static Color GetBorderColor(OrganizadinhoColorMode mode, Color baseColor)
        {
            Color border;
            switch (mode)
            {
                case OrganizadinhoColorMode.White:
                    border = EditorGUIUtility.isProSkin
                        ? new Color(0.72f, 0.72f, 0.7f, 1f)
                        : new Color(0.58f, 0.58f, 0.56f, 1f);
                    break;
                case OrganizadinhoColorMode.Black:
                    border = EditorGUIUtility.isProSkin
                        ? new Color(0.42f, 0.42f, 0.44f, 1f)
                        : new Color(0.02f, 0.02f, 0.025f, 1f);
                    break;
                default:
                    border = Color.Lerp(
                        baseColor,
                        EditorGUIUtility.isProSkin ? Color.black : Color.white,
                        EditorGUIUtility.isProSkin ? 0.42f : 0.34f);
                    break;
            }

            border.a = 0.85f;
            return border;
        }

        internal static float NormalizeHue(float hue)
        {
            return Mathf.Repeat(hue, 1f);
        }

        internal static Color FromHue(float hue, float saturation, float value)
        {
            var color = Color.HSVToRGB(NormalizeHue(hue), saturation, value);
            color.a = 1f;
            return color;
        }

        internal static Color GetReadableTextColor(Color backgroundColor)
        {
            var color = backgroundColor.linear;
            var luminance = (0.2126f * color.r) + (0.7152f * color.g) + (0.0722f * color.b);
            return luminance > 0.42f
                ? new Color(0.12f, 0.12f, 0.12f, 1f)
                : new Color(0.96f, 0.96f, 0.96f, 1f);
        }

        internal static Color GetMonochromeAccent(Color backgroundColor)
        {
            var foreground = GetReadableTextColor(backgroundColor);
            return foreground.r < 0.5f ? Color.black : Color.white;
        }

        internal static Color GetProjectChromeColor()
        {
            return EditorGUIUtility.isProSkin
                ? new Color(0.2f, 0.2f, 0.2f, 1f)
                : new Color(0.76f, 0.76f, 0.76f, 1f);
        }

        internal static Color GetUnitySelectionColor()
        {
            return EditorGUIUtility.isProSkin
                ? new Color(0.24f, 0.49f, 0.9f, 1f)
                : new Color(0.29f, 0.53f, 0.91f, 1f);
        }
    }
}
