using Organizadinho.Runtime;
using UnityEngine;

namespace Organizadinho.Editor.Utilities
{
    internal readonly struct OrganizadinhoColorSelection
    {
        internal OrganizadinhoColorSelection(OrganizadinhoColorMode mode, float hue)
            : this(mode, hue, ColorPaletteUtility.GetBaseColor(OrganizadinhoColorMode.Pastel, ColorPaletteUtility.DefaultHue))
        {
        }

        internal OrganizadinhoColorSelection(OrganizadinhoColorMode mode, float hue, Color customColor)
        {
            Mode = mode;
            Hue = ColorPaletteUtility.NormalizeHue(hue);
            CustomColor = NormalizeCustomColor(customColor);
        }

        internal OrganizadinhoColorMode Mode { get; }
        internal float Hue { get; }
        internal Color CustomColor { get; }

        internal static OrganizadinhoColorSelection Pastel(float hue)
        {
            return new OrganizadinhoColorSelection(OrganizadinhoColorMode.Pastel, hue);
        }

        internal static OrganizadinhoColorSelection Vibrant(float hue)
        {
            return new OrganizadinhoColorSelection(OrganizadinhoColorMode.Vibrant, hue);
        }

        internal static OrganizadinhoColorSelection Custom(Color color)
        {
            return new OrganizadinhoColorSelection(OrganizadinhoColorMode.Custom, ColorPaletteUtility.DefaultHue, color);
        }

        private static Color NormalizeCustomColor(Color color)
        {
            color.a = 1f;
            return color;
        }
    }
}
