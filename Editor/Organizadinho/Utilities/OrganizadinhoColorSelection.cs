using Organizadinho.Runtime;

namespace Organizadinho.Editor.Utilities
{
    internal readonly struct OrganizadinhoColorSelection
    {
        internal OrganizadinhoColorSelection(OrganizadinhoColorMode mode, float hue)
        {
            Mode = mode;
            Hue = ColorPaletteUtility.NormalizeHue(hue);
        }

        internal OrganizadinhoColorMode Mode { get; }
        internal float Hue { get; }

        internal static OrganizadinhoColorSelection Base(float hue)
        {
            return new OrganizadinhoColorSelection(OrganizadinhoColorMode.Base, hue);
        }
    }
}
