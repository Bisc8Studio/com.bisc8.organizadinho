namespace Organizadinho.Editor.Utilities
{
    internal static class OrganizadinhoColorClipboard
    {
        private static bool _hasHue;
        private static float _hue;

        internal static bool HasHue => _hasHue;

        internal static void CopyHue(float hue)
        {
            _hue = ColorPaletteUtility.NormalizeHue(hue);
            _hasHue = true;
        }

        internal static bool TryGetHue(out float hue)
        {
            hue = _hasHue ? _hue : ColorPaletteUtility.DefaultHue;
            return _hasHue;
        }
    }
}
