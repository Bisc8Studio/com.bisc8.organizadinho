namespace Organizadinho.Editor.Utilities
{
    internal static class OrganizadinhoColorClipboard
    {
        private static bool _hasColor;
        private static OrganizadinhoColorSelection _color;

        internal static bool HasColor => _hasColor;

        internal static void CopyColor(OrganizadinhoColorSelection color)
        {
            _color = color;
            _hasColor = true;
        }

        internal static bool TryGetColor(out OrganizadinhoColorSelection color)
        {
            color = _hasColor ? _color : OrganizadinhoColorSelection.Pastel(ColorPaletteUtility.DefaultHue);
            return _hasColor;
        }
    }
}
