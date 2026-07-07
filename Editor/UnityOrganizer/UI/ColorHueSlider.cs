using UnityEditor;
using UnityEngine;
using UnityOrganizer.Editor.Settings;
using UnityOrganizer.Editor.Utilities;
using UnityOrganizer.Runtime;

namespace UnityOrganizer.Editor.UI
{
    internal static class ColorHueSlider
    {
        private static Texture2D _pastelGradientTexture;
        private static Texture2D _vibrantGradientTexture;
        private static int _activeSliderControl;

        internal static float DrawHueSlider(string label, float currentHue, string previewLabel)
        {
            return DrawColorSlider(label, UnityOrganizerColorMode.Pastel, currentHue, previewLabel, out _).Hue;
        }

        internal static float DrawHueSlider(string label, float currentHue, string previewLabel, out bool pasted)
        {
            return DrawColorSlider(label, UnityOrganizerColorMode.Pastel, currentHue, previewLabel, out pasted).Hue;
        }

        internal static UnityOrganizerColorSelection DrawColorSlider(
            string label,
            UnityOrganizerColorMode currentMode,
            float currentHue,
            string previewLabel)
        {
            return DrawColorSlider(
                label,
                currentMode,
                currentHue,
                ColorPaletteUtility.GetBaseColor(UnityOrganizerColorMode.Pastel, ColorPaletteUtility.DefaultHue),
                previewLabel,
                out _);
        }

        internal static UnityOrganizerColorSelection DrawColorSlider(
            string label,
            UnityOrganizerColorMode currentMode,
            float currentHue,
            Color currentCustomColor,
            string previewLabel)
        {
            return DrawColorSlider(label, currentMode, currentHue, currentCustomColor, previewLabel, out _);
        }

        internal static UnityOrganizerColorSelection DrawColorSlider(
            string label,
            UnityOrganizerColorMode currentMode,
            float currentHue,
            string previewLabel,
            out bool pasted)
        {
            return DrawColorSlider(
                label,
                currentMode,
                currentHue,
                ColorPaletteUtility.GetBaseColor(UnityOrganizerColorMode.Pastel, ColorPaletteUtility.DefaultHue),
                previewLabel,
                out pasted);
        }

        internal static UnityOrganizerColorSelection DrawColorSlider(
            string label,
            UnityOrganizerColorMode currentMode,
            float currentHue,
            Color currentCustomColor,
            string previewLabel,
            out bool pasted)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            pasted = false;

            var selection = new UnityOrganizerColorSelection(currentMode, currentHue, currentCustomColor);
            if (UnityOrganizerUserPreferences.ShowPastelPalette || currentMode == UnityOrganizerColorMode.Pastel)
                selection = DrawSliderRow("Pastel", UnityOrganizerColorMode.Pastel, selection);

            if (UnityOrganizerUserPreferences.ShowVibrantPalette || currentMode == UnityOrganizerColorMode.Vibrant)
                selection = DrawSliderRow("Vibrant", UnityOrganizerColorMode.Vibrant, selection);

            selection = DrawSpecialColorControls(selection);
            if (UnityOrganizerUserPreferences.ShowCustomColor || currentMode == UnityOrganizerColorMode.Custom)
                selection = DrawCustomColorControl(selection);

            selection = DrawClipboardControls(selection, out pasted);

            var previewRect = EditorGUILayout.GetControlRect(GUILayout.Height(18f));
            DrawPreview(previewRect, selection, previewLabel);

            return selection;
        }

        private static UnityOrganizerColorSelection DrawSliderRow(
            string label,
            UnityOrganizerColorMode mode,
            UnityOrganizerColorSelection selection)
        {
            var isActive = selection.Mode == mode;
            var rowRect = GUILayoutUtility.GetRect(1f, 22f, GUILayout.ExpandWidth(true));
            var labelRect = new Rect(rowRect.x, rowRect.y + 2f, 52f, 18f);
            var sliderRect = new Rect(labelRect.xMax + 4f, rowRect.y + 2f, rowRect.width - labelRect.width - 4f, 18f);

            if (Event.current.type == EventType.Repaint)
            {
                var labelStyle = isActive ? EditorStyles.boldLabel : EditorStyles.miniLabel;
                EditorGUI.LabelField(labelRect, label, labelStyle);
                GUI.DrawTexture(sliderRect, GetGradientTexture(mode), ScaleMode.StretchToFill);
                EditorGUI.DrawRect(new Rect(sliderRect.x, sliderRect.y, sliderRect.width, 1f), new Color(0f, 0f, 0f, isActive ? 0.55f : 0.3f));
                EditorGUI.DrawRect(new Rect(sliderRect.x, sliderRect.yMax - 1f, sliderRect.width, 1f), new Color(0f, 0f, 0f, isActive ? 0.6f : 0.4f));
                if (isActive)
                    DrawHandle(sliderRect, selection);
            }

            return HandleSliderInput(sliderRect, mode, selection);
        }

        private static UnityOrganizerColorSelection DrawSpecialColorControls(UnityOrganizerColorSelection selection)
        {
            EditorGUILayout.BeginHorizontal();
            selection = DrawColorModeButton("White", UnityOrganizerColorMode.White, ColorPaletteUtility.GetBaseColor(UnityOrganizerColorMode.White, selection.Hue), selection);
            selection = DrawColorModeButton("Black", UnityOrganizerColorMode.Black, ColorPaletteUtility.GetBaseColor(UnityOrganizerColorMode.Black, selection.Hue), selection);
            EditorGUILayout.EndHorizontal();

            return selection;
        }

        private static UnityOrganizerColorSelection DrawColorModeButton(
            string label,
            UnityOrganizerColorMode mode,
            Color swatchColor,
            UnityOrganizerColorSelection selection)
        {
            var previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = selection.Mode == mode
                ? ColorPaletteUtility.BuildPalette(mode, selection.Hue, selection.CustomColor).SelectedColor
                : previousBackground;

            var content = new GUIContent("  " + label);
            var rect = GUILayoutUtility.GetRect(content, GUI.skin.button, GUILayout.Height(20f));
            if (GUI.Button(rect, content))
                selection = new UnityOrganizerColorSelection(mode, selection.Hue, selection.CustomColor);

            var swatchRect = new Rect(rect.x + 6f, rect.y + 4f, 12f, rect.height - 8f);
            EditorGUI.DrawRect(swatchRect, swatchColor);
            EditorGUI.DrawRect(new Rect(swatchRect.x, swatchRect.yMax - 1f, swatchRect.width, 1f),
                ColorPaletteUtility.BuildPalette(mode, selection.Hue, selection.CustomColor).BorderColor);

            GUI.backgroundColor = previousBackground;
            return selection;
        }

        private static UnityOrganizerColorSelection DrawCustomColorControl(UnityOrganizerColorSelection selection)
        {
            EditorGUILayout.BeginHorizontal();

            var isActive = selection.Mode == UnityOrganizerColorMode.Custom;
            var previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = isActive
                ? ColorPaletteUtility.BuildPalette(UnityOrganizerColorMode.Custom, selection.Hue, selection.CustomColor).SelectedColor
                : previousBackground;

            if (GUILayout.Button("Custom", GUILayout.Width(76f), GUILayout.Height(20f)))
                selection = new UnityOrganizerColorSelection(UnityOrganizerColorMode.Custom, selection.Hue, selection.CustomColor);

            GUI.backgroundColor = previousBackground;

            EditorGUI.BeginChangeCheck();
            var customColor = EditorGUILayout.ColorField(GUIContent.none, selection.CustomColor, false, false, false, GUILayout.Height(20f));
            if (EditorGUI.EndChangeCheck())
                selection = new UnityOrganizerColorSelection(UnityOrganizerColorMode.Custom, selection.Hue, customColor);

            EditorGUILayout.EndHorizontal();
            return selection;
        }

        private static UnityOrganizerColorSelection DrawClipboardControls(UnityOrganizerColorSelection selection, out bool pasted)
        {
            pasted = false;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Color", GUILayout.Height(20f)))
            {
                UnityOrganizerColorClipboard.CopyColor(selection);
            }

            EditorGUI.BeginDisabledGroup(!UnityOrganizerColorClipboard.HasColor);
            if (GUILayout.Button("Paste Color", GUILayout.Height(20f)) &&
                UnityOrganizerColorClipboard.TryGetColor(out var copiedColor))
            {
                selection = copiedColor;
                pasted = true;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            return selection;
        }

        private static UnityOrganizerColorSelection HandleSliderInput(
            Rect sliderRect,
            UnityOrganizerColorMode mode,
            UnityOrganizerColorSelection selection)
        {
            var currentEvent = Event.current;
            if (currentEvent == null)
            {
                return selection;
            }

            int controlId = GUIUtility.GetControlID(FocusType.Passive, sliderRect);
            bool isActive = GUIUtility.hotControl == controlId && _activeSliderControl == controlId;

            switch (currentEvent.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (currentEvent.button == 0 && sliderRect.Contains(currentEvent.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        _activeSliderControl = controlId;
                        currentEvent.Use();
                        GUI.changed = true;
                        RepaintActiveWindow();
                        return SelectionFromMouse(sliderRect, currentEvent.mousePosition, mode, selection.CustomColor);
                    }
                    break;

                case EventType.MouseDrag:
                    if (currentEvent.button == 0 && isActive)
                    {
                        currentEvent.Use();
                        GUI.changed = true;
                        RepaintActiveWindow();
                        return SelectionFromMouse(sliderRect, currentEvent.mousePosition, mode, selection.CustomColor);
                    }
                    break;

                case EventType.MouseUp:
                    if (isActive)
                    {
                        ReleaseSliderControl(controlId);
                        currentEvent.Use();
                        RepaintActiveWindow();
                    }
                    break;

                case EventType.KeyDown:
                    if (isActive && currentEvent.keyCode == KeyCode.Escape)
                    {
                        ReleaseSliderControl(controlId);
                        currentEvent.Use();
                    }
                    break;

                case EventType.Ignore:
                    if (isActive)
                        ReleaseSliderControl(controlId);
                    break;
            }

            return selection;
        }

        private static UnityOrganizerColorSelection SelectionFromMouse(
            Rect sliderRect,
            Vector2 mousePosition,
            UnityOrganizerColorMode mode,
            Color customColor)
        {
            return new UnityOrganizerColorSelection(
                mode,
                Mathf.Clamp01((mousePosition.x - sliderRect.x) / sliderRect.width),
                customColor);
        }

        private static void ReleaseSliderControl(int controlId)
        {
            if (GUIUtility.hotControl == controlId)
                GUIUtility.hotControl = 0;

            if (_activeSliderControl == controlId)
                _activeSliderControl = 0;
        }

        private static void RepaintActiveWindow()
        {
            EditorWindow.focusedWindow?.Repaint();
            if (!ReferenceEquals(EditorWindow.mouseOverWindow, EditorWindow.focusedWindow))
                EditorWindow.mouseOverWindow?.Repaint();
        }

        private static void DrawHandle(Rect sliderRect, UnityOrganizerColorSelection selection)
        {
            var x = Mathf.Lerp(sliderRect.x, sliderRect.xMax, selection.Hue);
            var handleRect = new Rect(x - 4f, sliderRect.y - 2f, 8f, sliderRect.height + 4f);
            EditorGUI.DrawRect(handleRect, new Color(0f, 0f, 0f, 0.55f));
            EditorGUI.DrawRect(
                new Rect(handleRect.x + 1f, handleRect.y + 1f, handleRect.width - 2f, handleRect.height - 2f),
                ColorPaletteUtility.GetReadableTextColor(ColorPaletteUtility.GetBaseColor(selection.Mode, selection.Hue, selection.CustomColor)));
        }

        private static void DrawPreview(Rect previewRect, UnityOrganizerColorSelection selection, string previewLabel)
        {
            var palette = ColorPaletteUtility.BuildPalette(selection.Mode, selection.Hue, selection.CustomColor);
            var swatchRect = new Rect(previewRect.x, previewRect.y + 2f, 26f, previewRect.height - 4f);
            EditorGUI.DrawRect(swatchRect, palette.BaseColor);
            EditorGUI.DrawRect(new Rect(swatchRect.x, swatchRect.yMax - 1f, swatchRect.width, 1f), palette.BorderColor);

            var labelRect = new Rect(swatchRect.xMax + 6f, previewRect.y, previewRect.width - swatchRect.width - 6f, previewRect.height);
            EditorGUI.LabelField(
                labelRect,
                string.IsNullOrEmpty(previewLabel) ? "Color preview" : previewLabel,
                EditorStyles.miniLabel);
        }

        private static Texture2D GetGradientTexture(UnityOrganizerColorMode mode)
        {
            switch (mode)
            {
                case UnityOrganizerColorMode.Vibrant:
                    return _vibrantGradientTexture ?? (_vibrantGradientTexture = CreateGradientTexture(mode));
                default:
                    return _pastelGradientTexture ?? (_pastelGradientTexture = CreateGradientTexture(UnityOrganizerColorMode.Pastel));
            }
        }

        internal static void DrawPalettePreview(Rect rect, UnityOrganizerColorMode mode, bool active)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var previousColor = GUI.color;
            GUI.color = active ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            GUI.DrawTexture(rect, GetGradientTexture(mode), ScaleMode.StretchToFill);
            GUI.color = previousColor;

            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), new Color(0f, 0f, 0f, active ? 0.55f : 0.3f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(0f, 0f, 0f, active ? 0.6f : 0.4f));
        }

        private static Texture2D CreateGradientTexture(UnityOrganizerColorMode mode)
        {
            const int width = 256;
            var texture = new Texture2D(width, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            for (var index = 0; index < width; index++)
            {
                var hue = index / (float)(width - 1);
                texture.SetPixel(index, 0, ColorPaletteUtility.FromHue(mode, hue));
            }

            texture.Apply();
            return texture;
        }
    }
}
