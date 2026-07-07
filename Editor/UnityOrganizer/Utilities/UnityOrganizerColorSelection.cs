using UnityOrganizer.Runtime;
using UnityEngine;

namespace UnityOrganizer.Editor.Utilities
{
    internal readonly struct UnityOrganizerColorSelection
    {
        internal UnityOrganizerColorSelection(UnityOrganizerColorMode mode, float hue)
            : this(mode, hue, ColorPaletteUtility.GetBaseColor(UnityOrganizerColorMode.Pastel, ColorPaletteUtility.DefaultHue))
        {
        }

        internal UnityOrganizerColorSelection(UnityOrganizerColorMode mode, float hue, Color customColor)
        {
            Mode = mode;
            Hue = ColorPaletteUtility.NormalizeHue(hue);
            CustomColor = NormalizeCustomColor(customColor);
        }

        internal UnityOrganizerColorMode Mode { get; }
        internal float Hue { get; }
        internal Color CustomColor { get; }

        internal static UnityOrganizerColorSelection Pastel(float hue)
        {
            return new UnityOrganizerColorSelection(UnityOrganizerColorMode.Pastel, hue);
        }

        internal static UnityOrganizerColorSelection Vibrant(float hue)
        {
            return new UnityOrganizerColorSelection(UnityOrganizerColorMode.Vibrant, hue);
        }

        internal static UnityOrganizerColorSelection Custom(Color color)
        {
            return new UnityOrganizerColorSelection(UnityOrganizerColorMode.Custom, ColorPaletteUtility.DefaultHue, color);
        }

        private static Color NormalizeCustomColor(Color color)
        {
            color.a = 1f;
            return color;
        }
    }
}
