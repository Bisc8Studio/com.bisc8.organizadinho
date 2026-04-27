using UnityEditor;
using UnityEngine;

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
        private const float BaseSaturation = 0.34f;
        private const float BaseValue = 0.96f;
        private const float ChildrenSaturation = 0.24f;
        private const float ChildrenValue = 0.99f;

        internal static ColorPalette BuildPalette(float hue)
        {
            var normalizedHue = Mathf.Repeat(hue, 1f);
            var baseColor = FromHue(normalizedHue, BaseSaturation, BaseValue);
            var chrome = GetProjectChromeColor();
            var selection = GetUnitySelectionColor();
            var toolbar = Color.Lerp(baseColor, chrome, EditorGUIUtility.isProSkin ? 0.8f : 0.88f);
            var shortcut = Color.Lerp(baseColor, chrome, EditorGUIUtility.isProSkin ? 0.42f : 0.56f);
            var hover = Color.Lerp(baseColor, chrome, EditorGUIUtility.isProSkin ? 0.56f : 0.7f);
            var selected = Color.Lerp(baseColor, selection, 0.28f);
            var border = Color.Lerp(
                baseColor,
                EditorGUIUtility.isProSkin ? Color.black : Color.white,
                EditorGUIUtility.isProSkin ? 0.42f : 0.34f);
            border.a = 0.85f;

            var children = FromHue(normalizedHue, ChildrenSaturation, ChildrenValue);
            children.a = 0.35f;

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
            return FromHue(hue, BaseSaturation, BaseValue);
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
