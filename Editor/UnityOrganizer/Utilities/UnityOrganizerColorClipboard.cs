namespace UnityOrganizer.Editor.Utilities
{
    internal static class UnityOrganizerColorClipboard
    {
        private static bool _hasColor;
        private static UnityOrganizerColorSelection _color;

        internal static bool HasColor => _hasColor;

        internal static void CopyColor(UnityOrganizerColorSelection color)
        {
            _color = color;
            _hasColor = true;
        }

        internal static bool TryGetColor(out UnityOrganizerColorSelection color)
        {
            color = _hasColor ? _color : UnityOrganizerColorSelection.Pastel(ColorPaletteUtility.DefaultHue);
            return _hasColor;
        }
    }
}
